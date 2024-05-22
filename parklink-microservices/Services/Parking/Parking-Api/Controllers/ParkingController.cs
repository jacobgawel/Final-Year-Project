using System.Net;
using System.Security.Claims;
using Amazon.S3;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Parking_Domain.Data;
using Parking_Domain.Entities;
using Parking_Infrastructure.Repositories;
using Parking_Infrastructure.S3;
using Parking_Infrastructure.Services;
using Parking_ServiceBus.Services;

namespace Parking_Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ParkingController : ControllerBase
{
    private readonly IParkingRepository _repository;
    private readonly ILogger<Parking> _logger;
    private readonly IS3UploadHelper _s3UploadHelper;
    private readonly IParkingSb _messageService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly INotificationService _notificationService;

    public ParkingController(IParkingRepository repository, ILogger<Parking> logger, IS3UploadHelper s3ImageHelper, IParkingSb messageService, IBackgroundJobClient backgroundJobClient, INotificationService notificationService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _s3UploadHelper = s3ImageHelper ?? throw new ArgumentNullException(nameof(s3ImageHelper));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _backgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    [HttpGet]
    [ProducesResponseType(typeof(Parking), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<Parking>>> GetParking()
    {
        /*
         *
         * Parameters: { None }
         *
         * Return: List<Parking> Parking - array instances of parking
         *
         * Description: Gets all the parking in the table
         *
         * Authorization: None
         */
        
        return Ok(await _repository.GetParking());
    }

    [HttpGet("verified")]
    [ProducesResponseType(typeof(List<Parking>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<Parking>>> GetVerifiedParking()
    {
        return Ok(await _repository.GetParkingByVerified());
    }
    
    [HttpGet("distance")]
    [ProducesResponseType(typeof(List<Parking>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<ParkingDistanceDto>>> GetParkingByDistance(double longitude, double latitude, TimeSpan duration)
    {
        return Ok(await _repository.GetClosestParkingSpots(latitude, longitude, duration));
    }

    [HttpGet("humanized/{id}")]
    [ProducesResponseType(typeof(ParkingHumanizedDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ParkingHumanizedDto>> GetHumanizedParkingById(Guid id)
    {
        return Ok(await _repository.GetHumanizedParkingById(id));
    }
    
    [HttpGet("{id}", Name = "GetParking")]
    [ProducesResponseType(typeof(Parking), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Parking>> GetParking(Guid id)
    {
        /*
         *
         * Parameters: { id } - id of the parking
         *
         * Return: Review
         *
         * Description: Gets the parking with the corresponding id
         *
         * Authorization: None
         */
        
        var parking = await _repository.GetParking(id);

        if (parking == null) return NotFound();

        return parking;
    }

    [Authorize]
    [HttpGet("account/{id}", Name = "GetParkingForProvider")]
    [ProducesResponseType(typeof(List<Parking>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<Parking>>> GetParkingByProviderId(Guid id)
    {
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }

        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);

        if (role == null) return Forbid();

        if (nameId == null && role.Value is not ("admin" or "provider"))
        {
            return Forbid();
        }

        _logger.LogInformation($"Fetching parking spots linked to user account: {id}");

        var parkingList = await _repository.GetParkingByProviderId(id);
        return parkingList;
    }
    
    
    
    // ------------------------------ WRITE OPERATIONS [ POST, PUT, DELETE ]-----------------------------
    
    
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(Parking), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Parking>> CreateParking([FromForm] ParkingCreateDto dto)
    {
        /*
         *
         * Parameters: { [FromBody] Parking } - gets the details of parking 
         *
         * Return: Status OK
         *
         * Description: Creates the parking with and returns the parking with the corresponding id
         *
         * Authorization: Only a user with the role of "admin" or "provider" is allowed to create a parking spot.
         * 
         * 
         */
        
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);
        var email = identity.FindFirst(ClaimTypes.Email);

        if (role == null) return Forbid();

        // The parking object is in JSON field,
        // which means that we cant automatically assign the parking entity to a local variable.
        // Must use JSON to deserialize first.
        var parking = JsonConvert.DeserializeObject<Parking>(dto.Parking);

        if (parking == null) return BadRequest("The Parking object is missing");

        if (parking is { TimeLimited: true, DayLimited: true })
            return BadRequest("You cannot have both TimeLimit and DayLimit set to true");

        if (parking is { TimeLimited: false, DayLimited: false })
            return BadRequest("You cannot have both TimeLimit and DayLimit set to false");

        if (nameId != null && role.Value is "admin" or "provider")
        {
            // accountId gets automatically populated by the endpoint based on the BearerToken of the user.
            // This avoids situations where a user is authenticated, but they add parking on behalf of someone else.

            Guid accountId = new Guid(nameId.Value);
            parking.AccountId = accountId;
            parking.Email = email!.Value;
        }
        else
        {
            return Forbid();
        }

        ParkingImageDto parkingImageDto = new ParkingImageDto();

        if (dto.Files != null)
        {
            try
            {
                _logger.LogInformation($"Image data passed: {dto.Files.Count}");
                var parkingImageObj = await _s3UploadHelper.UploadToParklinkS3(dto.Files);
                parkingImageDto = parkingImageObj;
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogError("Error during upload: " + e.Message);
            }

            parking.SlotImages = JsonConvert.SerializeObject(parkingImageDto);
        }

        Guid parkingId = await _repository.CreateParking(parking);
        
        _logger.LogInformation($"Retrieved the parking Id: {parkingId}");
        parking.Id = parkingId;

        _logger.LogInformation($"Creating parking for user with Id: {nameId.Value}");
        
        // email the user with the details of the created parking spot
        _backgroundJobClient.Enqueue(() => _notificationService.SendNewParkingEmail(parking, identity.FindFirst(ClaimTypes.Email)!.Value));

        return CreatedAtRoute("GetParking", new { id = parkingId }, parking);
    }
    
    [Authorize]
    [HttpPut]
    [ProducesResponseType(typeof(Parking), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Parking>> UpdateParking([FromBody] ParkingUpdateDto parking)
    {
        /*
         *
         * Parameters: { Parking } - The body will contain the details of the parking
         *
         * Return: Status OK
         *
         * Description: Updates the parking with the corresponding Id
         * 
         * TODO: implement
         * Improvements: Figure out a way to move objects that only have a value to the existing parking spot.
         * E.g. use automapper or something ref => https://stackoverflow.com/questions/43947475/how-to-ignore-null-values-for-all-source-members-during-mapping-in-automapper-6
         *
         */

        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);
        if (nameId == null || role == null) return Forbid();
        
        var id = parking.Id;
        var existingParking = await _repository.GetParking(id);

        if (existingParking == null) return NotFound();
        
        var accountId = new Guid(nameId.Value);
        
        // only administrators can update a parking spot after it has been rejected
        if (existingParking.ParkingRejected && role.Value != "admin") return Forbid();
        
        // only administrators and users who own the parking spot can update the parking spot
        if (existingParking.AccountId != accountId && role.Value != "admin") return Forbid();

        if (parking.ParkingRejected)
        {
            // sends an email that the parking spot has been rejected
            _backgroundJobClient.Enqueue(() => _notificationService.SendParkingRejected(existingParking.Email));
        }

        if (existingParking.VerificationStatus == false && parking.VerificationStatus)
        {
            // sends an email that the parking spot has been verified
            _backgroundJobClient.Enqueue(() => _notificationService.SendParkingVerified(existingParking.Email));
        }

        // only administrators can edit the verification status
        // this quickly checks if the verification status has been tampered with by 
        // a user that doesn't have administrator privileges
        if (existingParking.VerificationStatus != parking.VerificationStatus && role.Value != "admin") return Forbid();

        return Ok(await _repository.UpdateParking(parking));
    }

    [Authorize]
    [HttpPost("images")]
    [ProducesResponseType(typeof(Parking), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Parking>> UpdateParkingImage([FromForm] ParkingImageUpdateDto imageUpdateDto)
    {
        /*
         *
         * Parameters: { ParkingImageUpdateDto } - DirectoryPrefix, ImageList, DeleteList
         *
         * Return: Status OK { Parking }
         *
         * Description: this contains all the details required
         * to submit the new images to the correct directory and make further adjustments
         *
         */
        
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);

        if (nameId == null || role == null) return Forbid();

        var accountId = new Guid(nameId.Value);

        var existingParking = await _repository.GetParking(imageUpdateDto.ParkingId);

        if (existingParking == null) return NotFound();

        if (existingParking.AccountId != accountId && role.Value != "admin")
            return Forbid();
        
        _logger.LogInformation("Updating image for parking: " + existingParking.Id);
        
        var parking = await _s3UploadHelper.UpdateParkingImageS3(imageUpdateDto, existingParking);
        
        return parking;
    }
    
    [Authorize]
    [HttpDelete("{id}", Name = "DeleteParking")]
    [ProducesResponseType(typeof(Parking), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Parking>> DeleteParking(Guid id)
    {
        /*
         *
         * Parameters: { id } - id of the parking
         *
         * Return: Status OK
         *
         * Description: Deletes the parking with the corresponding id
         *
         */
        
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);

        if (nameId == null || role == null) return Forbid();

        var accountId = new Guid(nameId.Value);

        var existingParking = await _repository.GetParking(id);

        if (existingParking == null)
        {
            _logger.LogInformation($"The requested resource was not found");
            return NotFound();
        }
        
        // Make sure that the admin can delete roles not created by him later...
        // Just noticed the bug in the code.
        if (existingParking.AccountId != accountId && role.Value != "admin")
        {
            _logger.LogInformation($"Request forbidden by user id: {accountId} for parking id: {existingParking.Id}");
            return Forbid();
        }

        var result = await _repository.DeleteParking(id);

        if (result)
        {
            _logger.LogInformation($"Sending parking spot delete event to service bus queue. Parking Id: {id}");
            await _messageService.ParkingDeleted(existingParking);
            return Ok();
        }

        return NotFound();
    }
}