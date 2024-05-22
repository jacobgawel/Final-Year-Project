using System.Net;
using System.Security.Claims;
using AutoMapper;
using Fines_Domain.Data;
using Fines_Domain.Entities;
using Fines_Infrastructure.GrpcServices;
using Fines_Infrastructure.Persistence.Repositories;
using Fines_Infrastructure.S3;
using Fines_Infrastructure.Services;
using Grpc.Core;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fines_Api.Controllers;

[Route("api/v1/[controller]")]
public class FineController : ControllerBase
{
    private readonly IFineRepository _repository;
    private readonly BookingGrpcServices _grpcServices;
    private readonly ILogger<FineController> _logger;
    private readonly IMapper _mapper;
    private readonly IS3UploadHelper _s3UploadHelper;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly INotificationServices _notificationServices;

    public FineController(IFineRepository repository, BookingGrpcServices grpcServices, 
        ILogger<FineController> logger, IMapper mapper, IS3UploadHelper s3UploadHelper, 
        IBackgroundJobClient backgroundJobClient, INotificationServices notificationServices)
    {
        _grpcServices = grpcServices;
        _logger = logger;
        _mapper = mapper;
        _s3UploadHelper = s3UploadHelper;
        _backgroundJobClient = backgroundJobClient;
        _notificationServices = notificationServices;
        _repository = repository;
    }
    
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(Fine), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<Fine>>> GetFine()
    {
        /*
         *
         * Parameters: { } - None
         *
         * Return: Status OK
         *
         * Description: Returns all the fines in the db
         *
         */
        
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);
        
        if (nameId == null || role == null) return Forbid();
        
        if (role.Value != "admin") return Forbid();

        return Ok(await _repository.GetFine());
    }
    
    [Authorize]
    [HttpGet("{id}", Name = "GetFineById")]
    [ProducesResponseType(typeof(Fine), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Fine>> GetFineById(string id)
    {
        /*
         *
         * Parameters: { id } - Get fine by id
         *
         * Return: Status OK
         *
         * Description: Returns the fine with the specific id
         *
         */
        
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);
        
        if (nameId == null || role == null) return Forbid();
        
        // check fine exists
        var fine = await _repository.GetFine(new Guid(id));
        if (fine == null) return NotFound();
        
        // only the admins and providers who's parking spot the space belong to can access the fine
        // and of-course the user who the fine belongs to
        if (
            role.Value != "admin" &&            // The user is not an admin
            new Guid(nameId.Value) != fine.FineIssuerId &&  // The user's ID is different from the ticket's Provider ID
            new Guid(nameId.Value) != fine.AccountId      // The user's ID is different from the ticket's account ID
        ) {
            return Forbid();  // If all conditions are met, forbid access
        }

        return fine;
    }
    
    [Authorize]
    [HttpGet("user/{id}", Name = "GetFinesForUser")]
    [ProducesResponseType(typeof(Fine), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<Fine>>> GetFinesForUser(string id)
    {
        /*
         *
         * Parameters: { id } - Get fines for user
         *
         * Return: Status OK List<Fine>
         *
         * Description: Returns a list of fines associated with the account
         *
         */
        
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);
        
        if (nameId == null || role == null) return Forbid();

        if (new Guid(nameId.Value) != new Guid(id) && role.Value != "admin") return Forbid();

        return Ok(await _repository.GetFineByAccountId(new Guid(nameId.Value)));
    }
    
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(Fine), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Fine>> CreateFine([FromForm] FineCreateDto fineCreateDto)
    {
        /*
         *
         * Parameters: { Fine } - Details of the fine are passed through the body
         *
         * Return: Fines
         *
         * Description: The approved user simply has to pass through the parameter
         * the details of the fine and the essential details e.g. description, total and booking id
         * the ticket issuer details are populated automatically
         *
         */
        
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }

        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);
        
        if (nameId == null || role == null) return Forbid();
        
        if (role.Value != "admin" && role.Value != "provider") return Forbid();
        
        // Check that the booking exists via calling gRPC service.
        // If the booking does not exist, then a fine is not allowed to be added.
        
        // quickly map all the objects from the request to the fine
        var fine = new Fine();
        _mapper.Map(fineCreateDto, fine);

        try
        {
            var booking = await _grpcServices.GetBooking(fineCreateDto.BookingId.ToString());

            if (booking.Id == null) return NotFound();
            
            var accountId = booking.AccountId;
            var bookingId = booking.Id;
            var parkingId = booking.ParkingId;
            _logger.LogInformation($"Date of booking: {booking.StartDate}");
            var bookingDate = DateTime.Parse(booking.StartDate);
            
            // There is a 2-day threshold for submitting bookings.
            // Bookings that are a day after the fact should not be allowed to be created.
            if (bookingDate.Add(TimeSpan.FromDays(1)) < DateTime.UtcNow)
            {
                return BadRequest("You cannot submit a fine 2 days after the booking ends");
            }

            // add the accountId linked to the fine to the fineId
            fine.AccountId = new Guid(accountId);

            _logger.LogInformation($"The booking exists: {accountId}, {bookingId}, {parkingId}");
        }
        catch (RpcException)
        {
            var error = $"Booking with Id={fine.BookingId} does not exist";
            _logger.LogWarning(error);
            return NotFound(error);
        }

        var providerId = new Guid(nameId.Value);
        fine.FineIssuerId = providerId;
        
        // check if a fine already exists for this specific booking before creation
        if (await _repository.CheckIfBookingHasFine(fine.BookingId))
        {
            // if the booking already has fines, then it will return a bad request
            return Conflict("The booking already has a fine");
        }

        var s3UploadDto = await _s3UploadHelper
            .UploadToParklinkS3(fineCreateDto.File, fine.BookingId.ToString());

        fine.ImageUri = s3UploadDto.ImageUri;

        var fineId = await _repository.CreateFine(fine);
        fine.Id = fineId;
        
        return CreatedAtRoute("GetFineById", new { id = fineId }, fine);
    }

    [Authorize]
    [HttpPut]
    [ProducesResponseType(typeof(Fine), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Fine>> UpdateFine([FromBody] FineUpdateDto fine)
    {
        /*
         *
         * Parameters: { Fine } - The body will contain the details of the fine
         * 
         * Return: Status OK
         *
         * Description: updates the fine entity in the table
         *
         */
        
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);
        
        if (nameId == null || role == null) return Forbid();
        
        if (role.Value != "admin" && role.Value != "provider") return Forbid();
        
        var existingFine = await _repository.GetFine(fine.Id);
        
        if (existingFine == null) return NotFound();

        if (existingFine.FineIssuerId != new Guid(nameId.Value))
            return Forbid();
        
        await _repository.UpdateFine(fine);
        
        return Ok();
    }

    [Authorize]
    [HttpDelete("{id}", Name = "DeleteFine")]
    [ProducesResponseType(typeof(Fine), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Fine>> DeleteFine(Guid id)
    {
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);

        if (nameId == null || role == null) return Forbid();

        if (role.Value != "admin" && role.Value != "provider") return Forbid();

        var fine = await _repository.GetFine(id);
        if (fine == null) return NotFound();

        var result = await _repository.DeleteFine(id);

        try
        {
            // fetch the existing booking to get the email and update the fine status inside the booking database
            var bookingModel = await _grpcServices.GetBooking(fine.BookingId.ToString());
            await _grpcServices.FineDeleted(fine.BookingId.ToString());
            await _notificationServices.SendFineDeletedEmail(fine.BookingId.ToString(), bookingModel.Email);
        }
        catch (RpcException)
        {
            _logger.LogCritical($"Could not update the fine status of {id} to false! The booking doesn't exist. " +
                                $"Check the Grpc function BookingService.cs.");
        }
        
        return result? Ok() : NotFound();
    }

    [Authorize]
    [HttpGet("provider/analytics/{id}")]
    [ProducesResponseType(typeof(FineAnalyticsDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<FineAnalyticsDto>> FineAnalyticsForProvider(Guid id)
    {
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);

        if (nameId == null || role == null) return Forbid();
        
        if (role.Value != "admin" && role.Value != "provider") return Forbid();

        if (role.Value != "admin" && new Guid(nameId.Value) != id) return Forbid();
        
        var analyticsDto = await _repository.GetFinesAnalyticsForProvider(id);

        return analyticsDto;
    }
    
    [Authorize]
    [HttpGet("provider/{id}")]
    [ProducesResponseType(typeof(FineAnalyticsDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<Fine>>> GetFinesForProvider(Guid id)
    {
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);

        if (nameId == null || role == null) return Forbid();
        
        if (role.Value != "admin" && role.Value != "provider") return Forbid();
        
        if (role.Value != "admin" && new Guid(nameId.Value) != id) return Forbid();
        
        var finesForProvider = await _repository.GetFinesForProvider(id);
        
        return finesForProvider;
    }
    
    [Authorize]
    [HttpGet("admin/analytics")]
    [ProducesResponseType(typeof(FineAnalyticsDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<FineAnalyticsDto>> FineAnalyticsForAdmin()
    {
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);

        if (nameId == null || role == null) return Forbid();
        
        if (role.Value != "admin") return Forbid();

        var analyticsDto = await _repository.GetFinesAnalyticsForAdmin();

        return analyticsDto;
    }

    [Authorize]
    [HttpGet("verify/{id}", Name = "AdminVerifyFine")]
    [ProducesResponseType(typeof(Fine), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Fine>> AdminVerifyFine(Guid id)
    {
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);

        if (nameId == null || role == null) return Forbid();

        if (role.Value != "admin")
            return Forbid();

        var fine = await _repository.GetFine(id);

        if (fine == null) return NotFound();
        fine.FineStatus = true;
        
        var fineUpdateDto = new FineUpdateDto();
        _mapper.Map(fine, fineUpdateDto);

        var result = await _repository.UpdateFine(fineUpdateDto);

        if (result == false)
            return NotFound("The booking could not be found in the UpdateFine repository");

        try
        {
            var verifyResult = await _grpcServices.VerifyFine(fine.BookingId.ToString(), true);
            if (verifyResult.Result == false)
            {
                return BadRequest("The booking could not be updated has messed up, check the BookingGrpcService" +
                                  "and the update method in Booking-Repository");
            }
            
            _backgroundJobClient.Enqueue(() =>
                _notificationServices.SendFineSubmittedEmail(fine.BookingId.ToString(), verifyResult.UserEmail));
        }
        catch (RpcException)
        {
            _logger.LogCritical("The fine could not be verified check the grpc request... BookingServices");
        }
        
        return fine;
    }

    [Authorize]
    [HttpGet("pay/{id}", Name = "PayFine")]
    [ProducesResponseType(typeof(Fine), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Fine>> UserPayFine(Guid id)
    {
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }

        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);

        if (nameId == null || role == null) return Forbid();
        
        var fine = await _repository.GetFine(id);

        if (fine == null) return NotFound();
        fine.FinePaid = true;
        fine.FineStatus = true;
        var fineUpdateDto = new FineUpdateDto();
        _mapper.Map(fine, fineUpdateDto);
        
        var result = await _repository.UpdateFine(fineUpdateDto);

        if (result == false)
            return NotFound("The booking could not be found in the UpdateFine repository");
        
        try
        {
            var finePaidResult = await _grpcServices.FinePaid(fine.BookingId.ToString());
            if (finePaidResult.Result == false)
            {
                return BadRequest("The booking could not be updated has messed up, check the BookingGrpcService" +
                                  "and the update method in Booking-Repository");
            }
            
            _backgroundJobClient.Enqueue(() =>
                _notificationServices.SendFineDeletedEmail(fine.BookingId.ToString(), finePaidResult.UserEmail));
        }
        catch (RpcException)
        {
            _logger.LogCritical("The fine could not be verified check the grpc request... BookingServices");
        }
        
        return fine;
    }
}