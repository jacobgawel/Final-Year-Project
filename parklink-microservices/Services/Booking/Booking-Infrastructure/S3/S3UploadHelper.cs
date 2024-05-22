using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using AutoMapper;
using Booking_Domain.Data;
using Booking_Domain.Entities;
using Booking_Infrastructure.Persistence.Repositories;
using Hangfire;
using Microsoft.Extensions.Configuration;
using SkiaSharp;
using ZXing;
using ZXing.QrCode;
using ZXing.SkiaSharp;

namespace Booking_Infrastructure.S3;

public class S3UploadHelper : IS3UploadHelper
{
    private readonly IAmazonS3 _amazonS3;
    private readonly IBookingRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public S3UploadHelper(IAmazonS3 amazonS3, IBookingRepository repository, IConfiguration configuration, IMapper mapper)
    {
        _amazonS3 = amazonS3;
        _repository = repository;
        _configuration = configuration;
        _mapper = mapper;
    }
    
    [Queue("booking")]
    public async Task<bool> UploadQrCode(Guid bookingId, string address)
    {
        // this is a helper function that is responsible for uploading the generated qr code to the s3 bucket 
        // which will be passed to an email and will also be displayed on the user account
        
        var bucketName = "prlnk.cdn";
        var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_amazonS3, bucketName);
        var qrCodeString = $"https://www.google.com/maps/dir/?api=1&travelmode=driving&layer=traffic&destination={address}";
        var awsConfig = _configuration.GetValue<bool>("LocalStack:UseLocalStack");
        // update the service url based on the environment, Production uses AWS and Development uses LocalStack
        var serviceUrl = awsConfig ? "http://localhost:4566" : "https://s3.eu-west-2.amazonaws.com";
        
        if(!bucketExists) // if the bucket doesnt exist, it will automatically populate S3 with a new one
        {
            var request = new PutBucketRequest()
            {
                BucketName = bucketName,
                CannedACL = S3CannedACL.PublicRead
            };
            await _amazonS3.PutBucketAsync(request);
        }
        
        const int widthPixels = 500;
        const int heightPixels = 500;
        
        BarcodeWriter barcodeWriter = new BarcodeWriter()
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions()
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Height = heightPixels,
                Width = widthPixels
            }
        };
        
        var pathPrefix = $"qrcodes/{bookingId}.png";

        using (MemoryStream stream = new MemoryStream()) // initiate a memory stream to save the png file to
        {
            using SKBitmap qrCode = barcodeWriter.Write(qrCodeString);
            using SKImage skImage = SKImage.FromBitmap(qrCode);
            using SKData encoded = skImage.Encode(SKEncodedImageFormat.Png, 100);
            encoded.SaveTo(stream);
            
            var putRequest = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = pathPrefix,
                InputStream = stream,
                CannedACL = S3CannedACL.PublicRead
            };
            
            var response = await _amazonS3.PutObjectAsync(putRequest);
            
            if (response.HttpStatusCode != HttpStatusCode.OK) 
                return false; // if the qr code wasn't successfully added, then the function should end and return false
        }
        
        var fileUri = $"{serviceUrl}/{bucketName}/{pathPrefix}"; // set the file uri using the details
        
        var booking = await _repository.GetBooking(bookingId);
        
        var bookingUpdateRequest = new BookingUpdateDto();
        booking!.QrCodeLink = fileUri;
        _mapper.Map(booking, bookingUpdateRequest);
        
        await _repository.UpdateBooking(bookingUpdateRequest); // update the existing booking with the uri of the qr code
        
        return true;
    }
}