using AutoMapper;
using Booking_Domain.Data;
using Booking_Domain.Entities;
using Booking_Infrastructure.Persistence.Repositories;
using Grpc.Core;

namespace Booking_Grpc.Services
{
    public class BookingService : BookingProtoService.BookingProtoServiceBase
    {
        private readonly IBookingRepository _repository;
        private readonly ILogger<BookingService> _logger;
        private readonly IMapper _mapper;

        public BookingService(IBookingRepository repository, ILogger<BookingService> logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;

        }

        public override async Task<FinePaidResult> VerifyPaymentStatus(FinePaidRequest request, 
            ServerCallContext context)
        {
            Guid bookingId = new Guid(request.BookingId);
            var existingBooking = await _repository.GetBooking(bookingId);
            if (existingBooking == null)
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Booking with Id={bookingId.ToString()} - NOT FOUND"));

            existingBooking.FinePaid = true;
            existingBooking.FineStatus = false;
            
            BookingUpdateDto updateDto = new BookingUpdateDto();
            _mapper.Map(existingBooking, updateDto);
            
            var result = await _repository.UpdateBooking(updateDto);
            
            var finePaidResult = new FinePaidResult()
            {
                Result = result,
                BookingId = request.BookingId,
                UserEmail = existingBooking.Email
            };
            
            return finePaidResult;
        }
        
        public override async Task<VerificationResult> VerifyFineStatus(VerificationRequest request,
            ServerCallContext context)
        {
            Guid bookingId = new Guid(request.BookingId);
            var existingBooking = await _repository.GetBooking(bookingId);

            if (existingBooking == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Booking with Id={bookingId.ToString()} - NOT FOUND"));
            }

            existingBooking.FineStatus = request.FineStatus;

            BookingUpdateDto updateDto = new BookingUpdateDto();
            _mapper.Map(existingBooking, updateDto);

            var result = await _repository.UpdateBooking(updateDto);
            var verificationResult = new VerificationResult
            {
                Result = result,
                BookingId = request.BookingId,
                UserEmail = existingBooking.Email
            };

            return verificationResult;
        }

        public override async Task<AvailableReturn> CheckAvailable(GetAvailableRequest request,
            ServerCallContext context)
        {
            Guid parkingId = new Guid(request.ParkingId);

            DateTime dateTime = DateTime.Parse(request.BookingDate).ToUniversalTime();
            TimeSpan duration = TimeSpan.Parse(request.Duration);

            AvailableReturn availableReturn = new AvailableReturn();
            availableReturn.Result = await _repository.CheckSpotAvailableGrpc(parkingId, dateTime, duration, request.SlotCapacity);
            
            _logger.LogInformation($"Parking Id {parkingId}, {dateTime}, {duration} : result = {availableReturn.Result}");
            return availableReturn;
        }


        public override async Task<BookingModel> GetBooking(GetBookingRequest request, ServerCallContext context)
        {
            // this function is responsible for the GetBooking functionality in the fine controller
            
            Guid guid = new Guid(request.BookingId);
            var booking = await _repository.GetBooking(guid);

            if (booking == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Booking with Id={guid.ToString()} - NOT FOUND"));
            }

            _logger.LogInformation($"Booking with Id={request.BookingId}, Parking: {booking.ParkingId} - FOUND");

            var bookingModel = new BookingModel();
            _mapper.Map(booking, bookingModel);
            // when it retrieves the booking it also changes the booking fine status to true
            // booking.FineStatus = true;
            
            // map the object to the BookingRequest (with the updated FineStatus) and then call UpdateBooking method
            // var bookingRequest = new BookingUpdateDto();
            // _mapper.Map(booking, bookingRequest);
            
            // await _repository.UpdateBooking(bookingRequest);

            return bookingModel;
        }

        public override async Task<BookingUpdateStatus> FineDeleted(BookingIdRequest request, ServerCallContext context)
        {
            var guid = new Guid(request.BookingId);
            
            var bookingUpdateStatus = new BookingUpdateStatus
            {
                Status = false
            };

            var booking = await _repository.GetBooking(guid);
            
            // throw error if booking doesn't exist
            if (booking == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Booking with Id={guid.ToString()} - NOT FOUND"));
            }
            
            // changes the booking fine status to false when the fine is deleted
            booking.FineStatus = false;
            
            // the booking FineStatus alongside the other properties are then mapped 
            // to the BookingUpdateRequest object which then gets passed to the UpdateBooking method.
            var bookingRequest = new BookingUpdateDto();
            _mapper.Map(booking, bookingRequest);
            
            var result = await _repository.UpdateBooking(bookingRequest);

            if (result) bookingUpdateStatus.Status = true;

            return bookingUpdateStatus;
        }
    }
}
