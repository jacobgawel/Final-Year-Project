using Amazon.S3;
using Amazon.S3.Model;
using AutoMapper;
using Fines_Domain.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Fines_Infrastructure.S3;

public class S3UploadHelper : IS3UploadHelper
{
    private readonly IAmazonS3 _amazonS3;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly ILogger<S3UploadHelper> _logger;

    public S3UploadHelper(IAmazonS3 amazonS3, IConfiguration configuration, 
        IMapper mapper, ILogger<S3UploadHelper> logger)
    {
        _amazonS3 = amazonS3;
        _configuration = configuration;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FineImageDto> UploadToParklinkS3(IFormFile file, string bookingId)
    {
        var bucketName = "prlnk.cdn";
        var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_amazonS3, bucketName);
        var awsConfig = _configuration.GetValue<bool>("LocalStack:UseLocalStack");
        var serviceUrl = awsConfig ? "http://localhost:4566" : "https://s3.eu-west-2.amazonaws.com";
        
        if(!bucketExists) // if the bucket doesn't exist, it will automatically populate S3 with a new one
        {
            var bucketRequest = new PutBucketRequest()
            {
                BucketName = bucketName,
                CannedACL = S3CannedACL.PublicRead
            };
            await _amazonS3.PutBucketAsync(bucketRequest);
        }

        var directory = bookingId;
        var fileName = Guid.NewGuid();
        var path = $"fines/{directory}/{fileName}.png";
        
        var fileRequest = new PutObjectRequest()
        {
            BucketName = bucketName,
            Key = path,
            InputStream = file.OpenReadStream(),
            CannedACL = S3CannedACL.PublicRead
        };

        var pathUri = $"{serviceUrl}/{bucketName}/{path}";
        
        fileRequest.Metadata.Add("Content-Type", file.ContentType);
        await _amazonS3.PutObjectAsync(fileRequest);
        
        var s3ImageObject = new FineImageDto
        {
            ImageUri = pathUri,
            Directory = directory
        };

        return s3ImageObject;
    }
}