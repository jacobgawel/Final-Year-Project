using Amazon.S3;
using Amazon.S3.Model;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Parking_Domain.Data;
using Parking_Domain.Entities;
using Parking_Infrastructure.Repositories;

namespace Parking_Infrastructure.S3
{
    public class S3UploadHelper : IS3UploadHelper
    {
        private readonly IAmazonS3 _amazonS3;
        private readonly IConfiguration _configuration;
        private readonly IParkingRepository _parkingRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<S3UploadHelper> _logger;

        public S3UploadHelper(IAmazonS3 amazonS3, IConfiguration configuration, 
            IParkingRepository parkingRepository, IMapper mapper, 
            ILogger<S3UploadHelper> logger)
        {
            _amazonS3 = amazonS3;
            _configuration = configuration;
            _parkingRepository = parkingRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ParkingImageDto> UploadToParklinkS3(List<IFormFile> files)
        {
            /*
             * This function creates a directory for the images of the parking spot. The files are saved in that directory with a unique prefix
             * files are stored at: bucketName/data/directoryId/ImageId.png
             */
            var bucketName = "prlnk.cdn"; // if the cdn bucket ever changes - change it here
            var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_amazonS3, bucketName);
            var awsConfig = _configuration.GetValue<bool>("LocalStack:UseLocalStack");

            if(!bucketExists) // if the bucket doesn't exist, it will automatically populate S3 with a new one
            {
                var request = new PutBucketRequest()
                {
                    BucketName = bucketName,
                    CannedACL = S3CannedACL.PublicRead
                };
                await _amazonS3.PutBucketAsync(request);
            }

            var s3ImageObjects = new List<S3Image>();
            var directoryName = Guid.NewGuid().ToString();
            foreach (var file in files)
            {
                var fileName = Guid.NewGuid();
                var pathPrefix = $"data/{directoryName}/{fileName}.png";
                var request = new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = pathPrefix,
                    InputStream = file.OpenReadStream(),
                    CannedACL = S3CannedACL.PublicRead
                };
                request.Metadata.Add("Content-Type", file.ContentType);
                await _amazonS3.PutObjectAsync(request);
                // The configuration variable below swaps the serviceUrl depending on if the app is in production mode,
                // which would then swap the localstack config for the real AWS services.
                // For local development, I am using localstack to emulate S3
                var serviceUrl = awsConfig ? "http://localhost:4566" : "https://s3.eu-west-2.amazonaws.com";
                var fileUri = $"{serviceUrl}/{bucketName}/{pathPrefix}";
                var imageObj = new S3Image
                {
                    fileName = fileName.ToString(),
                    fileUri = fileUri
                };
                s3ImageObjects.Add(imageObj);
            }

            var parkingImageDto = new ParkingImageDto
            {
                s3ImageUris = s3ImageObjects,
                directoryPrefix = directoryName
            };

            return parkingImageDto;
        }

        public async Task<Parking> UpdateParkingImageS3(ParkingImageUpdateDto imageUpdateDto, Parking existingParking)
        {
            /*  
             * this function is responsible for updating the states of the images
             * based on the params that are passed through in the imageUpdateDto
             * .e.g add new images and delete
             */
            
            var bucketName = "prlnk.cdn"; // if the cdn bucket ever changes - change it here
            var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_amazonS3, bucketName);
            var awsConfig = _configuration.GetValue<bool>("LocalStack:UseLocalStack");

            if(!bucketExists) // if the bucket doesn't exist, it will automatically populate S3 with a new one
            {
                var request = new PutBucketRequest()
                {
                    BucketName = bucketName,
                    CannedACL = S3CannedACL.PublicRead
                };
                await _amazonS3.PutBucketAsync(request);
            }
            
            // EARLY RETURN THAT MAY BE RELATED TO OBJECTS BEING EMPTY
            if (existingParking.SlotImages is null)
            {
                if (imageUpdateDto.ImageList is not null)
                {
                    var parkingImages = await UploadToParklinkS3(imageUpdateDto.ImageList);
                    existingParking.SlotImages = JsonConvert.SerializeObject(parkingImages);
                    
                    var parkingUpdate = new ParkingUpdateDto();
                    _mapper.Map(existingParking, parkingUpdate);
                    await _parkingRepository.UpdateParking(parkingUpdate);
                    
                    return existingParking;
                }

                _logger.LogWarning("The parking spot doesn't have slot images and " +
                                   "the request doesn't have any new images to add. Update has been ignored.");
                return existingParking;
            }

            var existingSlotImages = JsonConvert.DeserializeObject<ParkingImageDto>(existingParking.SlotImages);
            
            // EARLY RETURN THAT MAY BE RELATED TO OBJECTS BEING EMPTY
            if (existingSlotImages is null)
            {
                if (imageUpdateDto.ImageList is null) return existingParking;
                
                // Creating a new directory and updating the parking spot
                // this is because the update has been request after the initial 
                // creation of the parking spot itself.
                // A directory is only made when necessary.
                var parkingImages = await UploadToParklinkS3(imageUpdateDto.ImageList);
                existingParking.SlotImages = JsonConvert.SerializeObject(parkingImages);
                
                var parkingUpdate = new ParkingUpdateDto();
                _mapper.Map(existingParking, parkingUpdate);
                await _parkingRepository.UpdateParking(parkingUpdate);
                
                return existingParking;
            }
            
            if (imageUpdateDto.DeleteList is not null)
            {
                // Create a list to store the items to remove
                var itemsToRemove = new List<S3Image>();

                // Iterate over the collection
                foreach (var s3Image in existingSlotImages.s3ImageUris)
                {
                    // Extract the URI from the S3Image object
                    var s3ImageUri = s3Image.fileName;

                    // Check if the URI exists in the DeleteList
                    if (imageUpdateDto.DeleteList.Contains(s3ImageUri))
                    {
                        // Add the S3Image object to the list of items to remove
                        itemsToRemove.Add(s3Image);
                    }
                }

                // Remove the items after the loop
                foreach (var itemToRemove in itemsToRemove)
                {
                    existingSlotImages.s3ImageUris.Remove(itemToRemove);
                }
            }

            if (imageUpdateDto.ImageList is not null)
            {
                var directoryName = "";
                
                if (existingSlotImages.directoryPrefix is null or "")
                {
                    // If in some crazy happenstance, the directory prefix is somehow deleted after creation
                    // generate a new directoryName and assign it.
                    directoryName = Guid.NewGuid().ToString();
                    existingSlotImages.directoryPrefix = directoryName;
                }
                else
                {
                    directoryName = existingSlotImages.directoryPrefix;
                }

                foreach (var file in imageUpdateDto.ImageList)
                {
                    var fileName = Guid.NewGuid();
                    var pathPrefix = $"data/{directoryName}/{fileName}.png";
                    var request = new PutObjectRequest()
                    {
                        BucketName = bucketName,
                        Key = pathPrefix,
                        InputStream = file.OpenReadStream(),
                        CannedACL = S3CannedACL.PublicRead
                    };
                        
                    request.Metadata.Add("Content-Type", file.ContentType);
                    await _amazonS3.PutObjectAsync(request);
                    var serviceUrl = awsConfig ? "http://localhost:4566" : "https://s3.eu-west-2.amazonaws.com";
                    var fileUri = $"{serviceUrl}/{bucketName}/{pathPrefix}";
                    var imageObj = new S3Image
                    {
                        fileName = fileName.ToString(),
                        fileUri = fileUri
                    };
                        
                    // serialize and update an existing list of images
                    existingSlotImages.s3ImageUris.Add(imageObj);
                }
            }
            
            var serializedNewImageUris = JsonConvert.SerializeObject(existingSlotImages);
            existingParking.SlotImages = serializedNewImageUris;
            
            // utilising parkingUpdateDto alongside the repository
            // to update the parking instead of doing it manually
            var parkingUpdateDto = new ParkingUpdateDto();
            
            // map the existing values including the updated images
            _mapper.Map(existingParking, parkingUpdateDto);

            await _parkingRepository.UpdateParking(parkingUpdateDto);

            return existingParking;
        }
    }
}
