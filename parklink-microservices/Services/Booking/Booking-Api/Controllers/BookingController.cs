using System.Net;
using System.Security.Claims;
using AutoMapper;
using Booking_Domain.Data;
using Booking_Domain.Entities;
using Booking_Infrastructure.GrpcServices;
using Booking_Infrastructure.Persistence.Repositories;
using Booking_Infrastructure.S3;
using Booking_Infrastructure.Services;
using Grpc.Core;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Booking_Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BookingController : ControllerBase
{
    private readonly IBookingRepository _repository;
    private readonly ILogger<Booking> _logger;
    private readonly IParkingGrpcServices _parkingGrpcServices;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly INotificationService _notificationServices;
    private readonly IS3UploadHelper _s3UploadHelper;
    private readonly IMapper _mapper;

    public BookingController(IBookingRepository repository, ILogger<Booking> logger, IParkingGrpcServices parkingGrpcServices, 
        IBackgroundJobClient backgroundJobClient, INotificationService notificationServices, IS3UploadHelper s3UploadHelper, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _parkingGrpcServices = parkingGrpcServices ?? throw new ArgumentNullException(nameof(parkingGrpcServices));
        _backgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
        _notificationServices = notificationServices ?? throw new ArgumentNullException(nameof(notificationServices));
        _s3UploadHelper = s3UploadHelper ?? throw new ArgumentNullException(nameof(s3UploadHelper));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(Booking), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<Booking>>> GetBooking()
    {
        /*
         *
         * Parameters: { None }
         *
         * Return: List<Booking> - an array of booking
         *
         * Description: Returns all the bookings in the table
         * 
         */

        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }

        // dunno if this is the best way to do this, but it works lol :)
        var role = identity.FindFirst(ClaimTypes.Role);

        if (role?.Value != "admin")
        {
            _logger.LogWarning($"Access was forbidden to user: {identity.Name} ");
            return Forbid();
        }

        _logger.LogInformation($"Access was granted to user: {identity.Name}");

        var bookings = await _repository.GetBooking();

        return bookings;
    }
    
    [Authorize]
    [HttpGet("{id}", Name = "GetBooking")]
    [ProducesResponseType(typeof(Booking), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Booking>> GetBooking(Guid id)
    {
        /*
         *
         * Parameters: { Guid: id } - id of the booking
         *
         * Return: Booking - an instance of booking with all the details
         *
         * Description: overloads the GetBooking with the id and returns the booking with the correct id
         *
         */

        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }

        // Getting resources
        var booking = await _repository.GetBooking(id);

        if (booking == null)
        {
            return NotFound();
        }

        // Getting identity claims to see if user is authenticated to see the info
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);

        if (nameId == null || role == null) return Forbid();

        // get the user id of the person who owns the booking
        if (booking.AccountId.ToString() == nameId.Value || role.Value == "admin" || role.Value == "provider")
        {
            _logger.LogInformation($"Resource access requested by user id: {nameId.Value}, Role: {role.Value}");
            return booking;
        }

        return Forbid();
    }

    [Authorize]
    [HttpGet("humanized/{id}")]
    [ProducesResponseType(typeof(BookingHumanizedDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<BookingHumanizedDto>> GetHumanizedBookingById(Guid id)
    {
        /*
         * Params { id } = BookingId that endpoint will be humanizing
         * 
         * Description: this endpoint is used to humanize data for the view booking page.
         * This endpoint is used to provide more human-readable formats for the booking that
         * is being retrieved.
         * 
         * Return: BookingHumanizedDto
         */
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }

        // Getting resources
        var booking = await _repository.GetBooking(id);

        if (booking == null)
        {
            return NotFound();
        }

        // Getting identity claims to see if user is authenticated to see the info
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);

        if (nameId == null || role == null) return Forbid();

        // get the user id of the person who owns the booking
        if (booking.AccountId.ToString() != nameId.Value && role.Value is not "admin" && role.Value is not "provider")
        {
            _logger.LogInformation($"Unauthorized resource access requested by user id: {nameId.Value}, Role: {role.Value}");
            return Forbid();
        }

        var humanizedBooking = await _repository.GetHumanizedBooking(booking);

        return humanizedBooking;
    }

    [Authorize]
    [HttpGet("account/{id}", Name = "GetAllBookingById")]
    [ProducesResponseType(typeof(Booking), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<Booking>>> GetAllBookingById(Guid id)
    {
        /*
         * 
         * Parameters: { Guid: id }
         *
         * Return: List<Booking> - an array of booking
         *
         * Description: Returns all the bookings in the table related to the account
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

        if (id != accountId && role.Value != "admin") return Forbid();
        // admin should be able to access bookings for a specific user

        var bookings = await _repository.GetBookingForAccountId(accountId);

        return bookings;
    }

    [Authorize]
    [HttpGet("provider/{providerId}")]
    public async Task<ActionResult<List<Booking>>> GetBookingForProvider(Guid providerId)
    {
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);
        
        if (nameId == null || role == null) return Forbid();

        if (role.Value != "provider" && role.Value != "admin") return Forbid();

        var booking = await _repository.GetBookingForProvider(providerId);
        return booking;
    }

    [Authorize]
    [HttpPost("gaps/{parkingId}")]
    public async Task<ActionResult<List<BookingGapDto>>> GetBookingGapsForParkingId
        (Guid parkingId, [FromBody] BookingGapsRequestDto requestDto)
    {
        /*
         *
         * Parameters: { Guid: id }
         *
         * Return: BookingGapsDto - an entity that contains gaps in an existing parking spot
         *
         * Description: Returns all the bookings in the table related to the account
         * this is mainly used in the card that are displayed for the user
         *
         */
        
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);
        
        if (nameId == null || role == null) return Forbid();

        ParkingModel parkingModel;
        
        try
        {
            // quick rpc call to make sure that the parking exists, also need to fetch the capacity of the parking spot
            parkingModel = await _parkingGrpcServices.GetParking(parkingId.ToString());
        }
        catch (RpcException)
        {
            var error = $"Parking Spot with Id={parkingId} NOT FOUND";
            _logger.LogWarning(error);
            return NotFound(error);
        }

        var duration = requestDto.BookingExit - requestDto.BookingDate;

        var bookingGaps = await _repository
            .FindGapsForParking(parkingId, requestDto.BookingDate, requestDto.BookingExit, parkingModel.SlotCapacity);

        return bookingGaps;
    }
    
    // this works the same way as the function (GetAllBookingById) except it has some humanizer elements that 
    // are going to be used in the cards to avoid parsing and formatting them on the client side
    [Authorize]
    [HttpGet("account/card/{id}", Name = "GetAllBookingForCardById")]
    public async Task<ActionResult<List<BookingHumanizedDto>>> GetAllBookingForCardById(Guid id)
    {
        /*
         *
         * Parameters: { Guid: id }
         *
         * Return: List<HumanizedBooking> - an array of humanized bookings
         *
         * Description: Returns all the bookings in the table related to the account
         * this is mainly used in the card that are displayed for the user
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

        if (id != accountId && role.Value != "admin") return Forbid();
        // admin should be able to access bookings for a specific user

        var bookings = await _repository.GetHumanizedBookingsByAccountId(accountId);

        return bookings;
    }

    [Authorize]
    [HttpGet("transaction/{id}", Name = "GetBookingRecord")]
    [ProducesResponseType(typeof(BookingRecord), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<BookingRecord>> GetBookingRecord(Guid id)
    {
        /*
         *
         * Parameters: { Guid: id } - id of the booking record
         *
         * Return: BookingRecord - an instance of booking record with all the details
         *
         * Description: returns the transaction details
         *
         */
        
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);

        if (nameId == null || role == null) return Forbid();

        var bookingRecord = await _repository.GetBookingRecord(id);

        if (bookingRecord == null) return NotFound();
        
        _logger.LogInformation($"Resource access requested by user id: {nameId.Value}, Role: {role.Value}");

        if (bookingRecord.AccountId.ToString() != nameId.Value && role.Value != "admin") return Forbid();
        
        return bookingRecord;
    }

    [Authorize]
    [HttpPost("price")]
    [ProducesResponseType(typeof(BookingPricingInfoDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<BookingPricingInfoDto>> GetBookingPricingInformation([FromBody] BookingPriceDto bookingPriceDto)
    {
        /*
         * Body = { BookingPriceDto }
         * 
         * Description: This is the endpoint used to quickly calculate the price of the slot that is being booked
         * in the booking checkout page.
         */
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);

        if (nameId == null) return Forbid();

        decimal price;
        
        try
        {
            var parkingModel = await _parkingGrpcServices.GetParking(bookingPriceDto.ParkingId.ToString());
            price = decimal.Parse(parkingModel.Price);
        }
        catch (RpcException)
        {
            var error = $"Parking Spot with Id={bookingPriceDto.ParkingId} NOT FOUND";
            _logger.LogWarning(error);
            return NotFound(error);
        }

        TimeSpan duration = bookingPriceDto.EndDate - bookingPriceDto.StartDate;

        _logger.LogInformation("Computing Duration: " + duration);

        var bookingPricing = await _repository
            .GetPricingForPeriod(bookingPriceDto.StartDate, duration, price);

        var pricingInfo = new BookingPricingInfoDto();
        
        _mapper.Map(bookingPricing, pricingInfo);
        pricingInfo.ParkingId = bookingPriceDto.ParkingId;
        
        pricingInfo.HumanizedTotal = bookingPricing
            .Total.ToString("C", new System.Globalization.CultureInfo("en-GB"));
        
        pricingInfo.HumanizedFees = bookingPricing
            .Fees.ToString("C", new System.Globalization.CultureInfo("en-GB"));
        
        pricingInfo.HumanizedSubTotal = bookingPricing
            .SubTotal.ToString("C", new System.Globalization.CultureInfo("en-GB"));

        return pricingInfo;
    }

    [Authorize]
    [HttpGet("analytics/provider/{id}")]
    [ProducesResponseType(typeof(BookingAnalyticsDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<BookingAnalyticsDto>> GetAnalyticsForProviderById(Guid id)
    {
        // id = providerId
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);
        
        if (nameId == null || role == null) return Forbid();

        if (role.Value != "provider" && role.Value != "admin") return Forbid();
        var providerAnalytics = await _repository.GetAnalyticsForProviderById(id);
        return providerAnalytics;
    }

    [Authorize]
    [HttpGet("analytics/admin")]
    [ProducesResponseType(typeof(BookingAnalyticsDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<BookingAnalyticsDto>> GetAnalyticsForAdmin()
    {
        // id = providerId
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);
        
        if (nameId == null || role == null) return Forbid();

        if (role.Value != "admin") return Forbid();

        var bookingAnalyticsDto = await _repository.GetAnalyticsForAdmin();

        return bookingAnalyticsDto;
    }

    [Authorize]
    [HttpGet("refund/{id}")]
    [ProducesResponseType(typeof(BookingRefundDto), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<BookingRefundDto>> GetBookingRefund(Guid id)
    {
        // id = bookingId
        
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);
        
        if (nameId == null || role == null) return Forbid();
        
        var booking = await _repository.GetBooking(id);
        
        if (booking == null) return NotFound("Booking does not exist");
        
        var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var currentTime = DateTime.UtcNow;
        var isDayLightSaving = info.IsDaylightSavingTime(currentTime);
        var currentDateTime = isDayLightSaving ? DateTime.UtcNow.AddHours(1) : DateTime.UtcNow;
        
        // Users can still cancel a booking if it's within the timeframe.
        // However, the refund of the booking depends on refund policies
        if (booking.EndDate < currentDateTime)
            return BadRequest();

        if (booking.AccountId != new Guid(nameId.Value) && role.Value != "admin") return Forbid();

        var refundDto = await _repository.ComputeRefundForBooking(booking);

        return refundDto;
    }
    
    // ------------------------------ WRITE OPERATIONS [ POST, PUT, DELETE ]-----------------------------

    [Authorize]
    [HttpPost("transaction")]
    [ProducesResponseType(typeof(BookingRecord), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<BookingRecord>> CreateBookingRecord([FromBody] BookingRecordDto bookingRecordDto)
    {
        /*
         *
         * Parameters: { BookingRecordRequest } - request object for the BookingRecord
         *
         * Return: BookingRecord - an instance of booking with all the details
         *
         * Description: returns the transaction details
         *
         */
        
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        
        var bookingRecord = new BookingRecord();
        
        _mapper.Map(bookingRecordDto, bookingRecord);
        
        if (nameId != null)
        {
            Guid accountId = new Guid(nameId.Value);
            bookingRecord.AccountId = accountId;
        }
        
        var unsuccessfulCard = "4000000000000000"; // this card number will mock unsuccessful card transactions
        
        if (unsuccessfulCard.Equals(bookingRecord.CardNumber))
        {
            return BadRequest("The payment was unsuccessful");
        }
        
        var bookingRecordObject = await _repository.CreateBookingRecord(bookingRecord);
        
        return CreatedAtRoute("GetBookingRecord", new { id = bookingRecordObject.Id }, bookingRecordObject);
    }
    
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(Booking), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Booking>> CreateBooking([FromBody] BookingDto bookingDto)
    {
        /*
         * 
         * Parameters: { FromBody: Booking } - The body will contain the details of the booking
         *
         * Return: Reroutes to GetBooking - returns an instance of Booking
         *
         * Description: creates the booking and routes back to GetBooking alongside the new id
         *
         * Transaction Relation: If the booking is not created. The transaction will not be updated to the verified
         * state which means that the transaction is invalid.
         * 
         */


        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }

        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var email = identity.FindFirst(ClaimTypes.Email);

        _logger.LogInformation($"Creating booking for user with Id: {nameId?.Value}");

        var booking = new Booking();

        _mapper.Map(bookingDto, booking);

        if (nameId != null && email != null)
        {
            // accountId gets automatically populated by the endpoint based on the BearerToken of the user.
            // This avoids situations where a user is authenticated, but they add a booking on behalf of someone else.
            Guid accountId = new Guid(nameId.Value);
            booking.AccountId = accountId;
            booking.Email = email.Value;
        }
        
        
        // transaction exists check (get unverified now or nah innit)
        // the transaction should be unverified to make sure that its recent and 
        // hasn't been applied to another booking.
        var transaction = await _repository.GetUnverifiedRecord(booking.RecordId);

        if (transaction == null) return NotFound("Transaction does not exist");
        
        // Bug: Not a bug but if a TransactionId is passed, that's already been used (ripple in the universe stuff).
        // It will throw an exception and IDK what happens then.
        // In theory, this should not happen because a new transaction is generated
        // every time the BookNow function is called (from the client)
        // and that transaction is updated to verify when the booking is being created.
        // So it shouldn't ever happen.
        // If it does... too bad!
        if (transaction.Verified) return Forbid();

        ParkingModel parkingModel;

        try
        {
            // check if the parking spot exists
            parkingModel = await _parkingGrpcServices.GetParking(booking.ParkingId.ToString());
            _logger.LogInformation($"Parking spot with Id={parkingModel.Id} FOUND");
            _logger.LogInformation(parkingModel.ToString());
        }
        catch (RpcException)
        {
            var error = $"Parking Spot with Id={booking.ParkingId} NOT FOUND";
            _logger.LogWarning(error);
            return NotFound(error);
        }
       
        // check if the parking spot verification status is true (the parking spot is valid and not pending a check).
        // check if the parking spot availability status is true (that the owner of the parking spot has made it temporarily unavailable for bookings)
        var parkingVerificationStatus = parkingModel.VerificationStatus;
        var parkingAvailabilityStatus = parkingModel.AvailabilityStatus;
        var slotCapacity = parkingModel.SlotCapacity;
        booking.ProviderId = new Guid(parkingModel.AccountId);

        if (!parkingVerificationStatus || !parkingAvailabilityStatus)
        {
            return NotFound("Parking space is unavailable at this time");
        }

        // check if the duration of the booking is valid for the parking spot MaxTimeLimit
        var bookingDuration = booking.EndDate - booking.StartDate;

        if (parkingModel.TimeLimited)
        {
            TimeSpan duration = TimeSpan.Parse(parkingModel.TimeLimit);

            if (bookingDuration > duration)
            {
                return BadRequest();
            }
        }

        if (parkingModel.DayLimited)
        {
            TimeSpan duration = TimeSpan.FromDays(parkingModel.DayLimit);
            if (bookingDuration > duration)
            {
                return BadRequest();
            }
        }

        var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var currentTime = DateTime.UtcNow;
        
        var isDayLightSaving = info.IsDaylightSavingTime(currentTime);

        if (isDayLightSaving)
        {
            currentTime = currentTime.AddHours(1);
        }

        // Check if the user is attempting to book a date that's in the past.
        // It will work if it's still in the booking timeframe.
        // EXAMPLE: BookingDate -> 00:00:00 Duration -> 00:30:00 Exit -> 00:30:00 CurrentTime -> 00:20:00 = will be a valid booking
        if (booking.StartDate.Add(bookingDuration) < currentTime)
        {
            _logger.LogInformation($"{booking.StartDate} : {currentTime}");
            return BadRequest("Booking period has elapsed");

        // checks if there are conflicts with other bookings
        }
        
        if (!await _repository.CheckIfSpotAvailable(booking, slotCapacity))
        {
            // TODO: Potentially return the next available parking spot when the booking is conflicted
            return Conflict();
        }

        // convert the price returned from the gRPC call to a decimal from string (gRPC does not natively support decimal types)
        decimal parkingSpotPrice = Convert.ToDecimal(parkingModel.Price);

        var bookingObject = await _repository.CreateBooking(booking, parkingSpotPrice);

        _logger.LogInformation($"Retrieved the booking Id: {bookingObject.Id}");
        
        // invoke jobs - uses continuation to make sure that an email is sent only after the qr code is uploaded.
        var jobId  = _backgroundJobClient.Enqueue(() => _s3UploadHelper.UploadQrCode(bookingObject.Id, parkingModel.Address));

        // continuing the job using the jobId obtained from the backgroundJobClient
        // HACK, don't change!!
        // More info on this in the BookingUpdate function in the booking repo...
        var updateDto = new BookingUpdateDto();
        _mapper.Map(booking, updateDto);
        BatchJob.ContinueJobWith(jobId,
            batch => batch.Enqueue(() => _notificationServices.SendNewBookingEmail(parkingModel.Address, updateDto, booking.Email)));
        
        return CreatedAtRoute("GetBooking", new { id = bookingObject.Id }, bookingObject);
    }

    [Authorize]
    [HttpPut]
    [ProducesResponseType(typeof(Booking), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<bool>> UpdateBooking([FromBody] BookingUpdateDto bookingDto)
    {
        /*
         *
         * Parameters: { Booking } - The body will contain the details of the booking
         *
         * Return: Status OK
         *
         * Description: updates the booking entity in the table
         *
         */

        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }

        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);

        if (nameId == null || role == null) return Forbid();

        var id = bookingDto.Id;
        var booking = await _repository.GetBooking(id);

        if (booking == null) return NotFound();
        
        var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var currentTime = DateTime.UtcNow;
        
        var isDayLightSaving = info.IsDaylightSavingTime(currentTime);

        if (isDayLightSaving)
        {
            currentTime = currentTime.AddHours(1);
        }

        // checks to make sure the user is not updating the booking after the booking ends
        if (currentTime >= booking.EndDate && booking.FineStatus == false)
            return BadRequest("Cannot update after booking ends");
        
        // check if the user is cancelling the booking
        if (booking.BookingConfirmation && bookingDto.BookingConfirmation == false)
        {
            await _repository.SubmitRefund(bookingDto);
        }

        _mapper.Map(bookingDto, booking);
        
        ParkingModel parkingModel;

        try
        {
            // check if the parking spot exists
            parkingModel = await _parkingGrpcServices.GetParking(booking.ParkingId.ToString());
            _logger.LogInformation($"Parking spot with Id={parkingModel.Id} FOUND");
            _logger.LogInformation(parkingModel.ToString());
        }
        catch (RpcException)
        {
            var error = $"Parking Spot with Id={booking.ParkingId} NOT FOUND";
            _logger.LogWarning(error);
            return NotFound(error);
        }

        // checks if there are conflicts with other bookings
        var checkResult = await _repository.CheckIfSpotAvailable(booking, parkingModel.SlotCapacity);

        if (!checkResult)
        {
            return Conflict();
        }
        
        var guid = new Guid(nameId.Value);
        if (booking.AccountId == guid || role.Value == "admin")
        {
            var result = await _repository.UpdateBooking(bookingDto);
            return result;
        }

        return Forbid();
    }

    [Authorize]
    [HttpDelete("{id}", Name = "DeleteBooking")]
    [ProducesResponseType(typeof(Booking), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> DeleteBooking(Guid id)
    {
        /*
         *
         * Parameters: { id } - id of the booking
         *
         * Return: Status OK
         *
         * Description: Deletes the booking with the corresponding id
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

        var booking = await _repository.GetBooking(id);
        if (booking == null) return NotFound();

        _logger.LogInformation($"Delete requested by id: {nameId.Value} for booking id: {id}");

        if (booking.AccountId != accountId && role.Value != "admin")
        {
            _logger.LogInformation($"Request forbidden, user id: {accountId} for booking id: {booking.Id}");
            return Forbid();
        }

        var result = await _repository.DeleteBooking(id);

        return result ? Ok() : NotFound();
    }
}