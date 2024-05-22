using AutoFixture;
using AutoMapper;
using Booking_Api.Controllers;
using Booking_Domain.Data;
using Booking_Domain.Entities;
using Booking_Infrastructure.GrpcServices;
using Booking_Infrastructure.Persistence.Repositories;
using Booking_Infrastructure.S3;
using Booking_Infrastructure.Services;
using Booking_Tests.Utils;
using Grpc.Core;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Booking_Tests
{
    public class BookingControllerAdmin
    {
        private readonly Mock<IBookingRepository> _bookingRepo;
        private readonly Fixture _fixture;
        private readonly BookingController _controller;
        private readonly Mock<ILogger<Booking>> _logger;
        private readonly Mock<IParkingGrpcServices> _grpcServices;
        private readonly Mock<INotificationService> _notificationServices;
        private readonly Mock<IBackgroundJobClient> _backgroundJobClient;
        private readonly Mock<IS3UploadHelper> _s3UploadHelper;
        private readonly Mock<IBatchJobClient> _batchJobClient;
        private readonly Mock<IMapper> _mapper;

        public BookingControllerAdmin()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _bookingRepo = new Mock<IBookingRepository>();
            _logger = new Mock<ILogger<Booking>>();
            _grpcServices = new Mock<IParkingGrpcServices>();
            _notificationServices = new Mock<INotificationService>();
            _backgroundJobClient = new Mock<IBackgroundJobClient>();
            _s3UploadHelper = new Mock<IS3UploadHelper>();
            _batchJobClient = new Mock<IBatchJobClient>();
            _mapper = new Mock<IMapper>();

            _controller = new BookingController(_bookingRepo.Object, _logger.Object, _grpcServices.Object,
                _backgroundJobClient.Object, _notificationServices.Object, _s3UploadHelper.Object, _mapper.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = Helpers.GetClaimsPrincipal() }
                }
            };
        }

        [Fact]
        public async Task GetBooking_WithNoParams_Returns10Bookings()
        {
            /*
             * test mocks the repository returning 10 potential bookings
             */

            var bookingMock = _fixture.CreateMany<Booking>(10).ToList();
            _bookingRepo.Setup(s => s.GetBooking()).ReturnsAsync(bookingMock);

            var result = await _controller.GetBooking();

            Assert.Equal(10, actual: result.Value.Count);
            Assert.IsType<ActionResult<List<Booking>>>(result);
        }

        [Fact]
        public async Task GetBooking_WithId_ReturnsBooking()
        {
            /*
             * Test mocks the return of a booking with any id
             */

            var bookingMock = _fixture.Create<Booking>();
            _bookingRepo.Setup(s => s.GetBooking(It.IsAny<Guid>())).ReturnsAsync(bookingMock);

            var result = await _controller.GetBooking(bookingMock.Id);
            Assert.Equal(bookingMock, result.Value);
            Assert.IsType<ActionResult<Booking>>(result);
        }

        [Fact]
        public async Task GetBooking_WithInvalidId_ReturnsNotFound()
        {
            _bookingRepo.Setup(s => s.GetBooking(It.IsAny<Guid>())).ReturnsAsync(value: null);

            var result = await _controller.GetBooking(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateBooking_ValidRequest_ReturnsCreatedResult()
        {
            /*
             * There's additional comments in this controller to showcase how the moq tests framework works 
             * this is for personal info and understanding etc...
             */
            var parkingModelMock = _fixture.Create<ParkingModel>();
            parkingModelMock.Price = "10.50";
            parkingModelMock.VerificationStatus = true;
            parkingModelMock.SlotCapacity = 2;

            // mock gRPC communication within the controller
            _grpcServices.Setup(s => s.GetParking(It.IsAny<string>())).ReturnsAsync(parkingModelMock);

            // mock the function return in the CheckIfSpotAvailable function in the repo. Basically just returns true
            // since we are mocking a valid booking.
            _bookingRepo.Setup(r => r.CheckIfSpotAvailable(It.IsAny<Booking>(), parkingModelMock.SlotCapacity)).ReturnsAsync(true);

            var bookingModelMock = _fixture.Create<Booking>();

            // Simulate returning a new booking ID
            _bookingRepo.Setup(r => r.CreateBooking(It.IsAny<Booking>(), It.IsAny<decimal>()))
                .ReturnsAsync(bookingModelMock);

            // Arrange
            var bookingMock = _fixture.Create<BookingDto>();
            bookingMock.StartDate = DateTime.UtcNow.AddHours(1); // set future booking time
            parkingModelMock.TimeLimit = "02:00:00";

            // Mock setups (as shown above)

            // Act
            var result = await _controller.CreateBooking(bookingMock);

            // Assert
            Assert.IsType<CreatedAtRouteResult>(result.Result);
        }

        [Fact]
        public async Task CreateBooking_OverParkingMaxTimeLimit_ReturnsBadRequest()
        {
            /*
             * There's additional comments in this controller to showcase how the moq tests framework works 
             * this is for personal info and understanding etc...
             */
            var parkingModelMock = _fixture.Create<ParkingModel>();
            parkingModelMock.Price = "10.50";
            parkingModelMock.TimeLimit = "02:00:00";
            parkingModelMock.VerificationStatus = true;
            parkingModelMock.SlotCapacity = 2;

            // mock gRPC communication within the controller
            _grpcServices.Setup(s => s.GetParking(It.IsAny<string>())).ReturnsAsync(parkingModelMock);

            var bookingModel = _fixture.Create<Booking>();

            // Simulate returning a new booking ID
            _bookingRepo.Setup(r => r.CreateBooking(It.IsAny<Booking>(), It.IsAny<decimal>()))
                .ReturnsAsync(bookingModel);

            // Arrange
            var bookingMockRequest = _fixture.Create<BookingDto>();
            bookingMockRequest.StartDate = DateTime.UtcNow.AddHours(1); // set future booking time

            var bookingMock = _fixture.Create<Booking>();

            // mock the function return in the CheckIfSpotAvailable function in the repo. Basically just returns true
            // since we are mocking a valid booking.
            _bookingRepo.Setup(r => r.CheckIfSpotAvailable(bookingMock, parkingModelMock.SlotCapacity)).ReturnsAsync(true);

            // Mock setups (as shown above)

            // Act
            var result = await _controller.CreateBooking(bookingMockRequest);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task CreateBooking_ParkingNotFound_ReturnsNotFound()
        {
            _grpcServices.Setup(s => s.GetParking(It.IsAny<string>()))
                .ThrowsAsync(new RpcException(new Status(StatusCode.NotFound, "Not Found")));

            var bookingMock = _fixture.Create<BookingDto>();

            var result = await _controller.CreateBooking(bookingMock);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateBooking_BookingPeriodElapsed_ReturnsBadRequest()
        {

            /*
             * the booking period here represents an elapsed time frame 
             * e.g. the booking period is 40 mins before the current time 
             * and the booking duration is 30 minutes which is not within the timeframe
             */

            var bookingMock = _fixture.Create<BookingDto>();
            bookingMock.StartDate = DateTime.UtcNow - TimeSpan.FromMinutes(40);

            var parkingModelMock = _fixture.Create<ParkingModel>();
            parkingModelMock.Price = "10.50";
            parkingModelMock.TimeLimit = "02:00:00";
            parkingModelMock.VerificationStatus = true;
            parkingModelMock.AvailabilityStatus = true;
            parkingModelMock.SlotCapacity = 2;

            // gRPC communication should return a succesfull result
            _grpcServices.Setup(s => s.GetParking(It.IsAny<string>())).ReturnsAsync(parkingModelMock);

            var result = await _controller.CreateBooking(bookingMock);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateBooking_ParkingSpotIsNotVerifiedByAdmin_ReturnsBadRequest()
        {

            /*
             * the booking period here represents an elapsed time frame 
             * e.g. the booking period is 40 mins before the current time 
             * and the booking duration is 30 minutes which is not within the timeframe
             */

            var bookingMock = _fixture.Create<BookingDto>();
            bookingMock.StartDate = DateTime.UtcNow;

            var parkingModelMock = _fixture.Create<ParkingModel>();
            parkingModelMock.Price = "10.50";
            parkingModelMock.TimeLimit = "02:00:00";
            parkingModelMock.VerificationStatus = false;

            // gRPC communication should return a succesfull result
            _grpcServices.Setup(s => s.GetParking(It.IsAny<string>())).ReturnsAsync(parkingModelMock);

            var result = await _controller.CreateBooking(bookingMock);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateBooking_JustInTimeValidBooking_ReturnsBooking()
        {
            /*
             * this test mocks JIT booking that are booked close to the booking time but within 
             * a valid timeframe of the booking
             */

            var bookingMock = _fixture.Create<BookingDto>();
            bookingMock.StartDate = DateTime.UtcNow - TimeSpan.FromMinutes(20);

            var parkingModelMock = _fixture.Create<ParkingModel>();
            parkingModelMock.Price = "10.50";
            parkingModelMock.TimeLimit = "02:00:00";
            parkingModelMock.VerificationStatus = true;
            parkingModelMock.SlotCapacity = 2;

            var bookingMockSpot = _fixture.Create<Booking>();

            // gRPC communication should return a succesfull result
            _grpcServices.Setup(s => s.GetParking(It.IsAny<string>())).ReturnsAsync(parkingModelMock);

            // return a valid parking spot since we are mocking a successful JIT booking
            _bookingRepo.Setup(r => r.CheckIfSpotAvailable(bookingMockSpot, parkingModelMock.SlotCapacity)).ReturnsAsync(true);

            var bookingModel = _fixture.Create<Booking>();

            // booking repo for a succesfull booking
            _bookingRepo.Setup(r => r.CreateBooking(It.IsAny<Booking>(), It.IsAny<decimal>()))
                .ReturnsAsync(bookingModel);

            var result = await _controller.CreateBooking(bookingMock);

            Assert.IsType<CreatedAtRouteResult>(result.Result);
        }

        [Fact]
        public async Task CreateBooking_ParkingSpotUnavailable_ReturnsConflictResult()
        {
            /*
             * this test mocks a potential conflict in the parking spot check
             */

            var bookingMock = _fixture.Create<BookingDto>();
            bookingMock.StartDate = DateTime.UtcNow.AddHours(1); // set future booking time

            var parkingModelMock = _fixture.Create<ParkingModel>();
            parkingModelMock.Price = "10.50";
            parkingModelMock.TimeLimit = "02:00:00";
            parkingModelMock.VerificationStatus = true;
            parkingModelMock.SlotCapacity = 2;

            var bookingMockSpot = _fixture.Create<Booking>();


            // gRPC communication should return a succesfull result
            _grpcServices.Setup(s => s.GetParking(It.IsAny<string>())).ReturnsAsync(parkingModelMock);

            // return a valid parking spot since we are mocking a successful JIT booking
            _bookingRepo.Setup(r => r.CheckIfSpotAvailable(bookingMockSpot, parkingModelMock.SlotCapacity)).ReturnsAsync(false);

            var result = await _controller.CreateBooking(bookingMock);

            Assert.IsType<ConflictResult>(result.Result);
        }

        [Fact]
        public async Task GetAllBookingById_WithValidAccountId()
        {
            var accountId = new Guid("025afb7c-5483-4abe-b17e-0b3dde9eb75b"); // must match the one in the claims
            // the controller fetches the id dynamically which is linked to the JWT token that is passed in the request.

            var bookingsMock = _fixture.CreateMany<Booking>(5).ToList();

            foreach (var booking in bookingsMock)
            {
                booking.AccountId = accountId;
            }

            _bookingRepo.Setup(s => s.GetBookingForAccountId(accountId)).ReturnsAsync(bookingsMock);

            var result = await _controller.GetAllBookingById(accountId);

            Assert.Equal(5, result.Value.Count);
            Assert.IsType<ActionResult<List<Booking>>>(result);
        }

        [Fact]
        public async Task UpdateBooking_WithValidBooking_ReturnTrue()
        {
            // variables to use to update the booking
            var accountId = new Guid("025afb7c-5483-4abe-b17e-0b3dde9eb75b"); // must be the same for explained above

            var existingBookingMock = _fixture.Create<Booking>();
            existingBookingMock.AccountId = accountId;
            existingBookingMock.Id = existingBookingMock.Id;
            existingBookingMock.StartDate = DateTime.UtcNow + TimeSpan.FromDays(1); // add a day to the booking to make sure
            // its in the future

            // return the existing mock using the accountId generated
            _bookingRepo.Setup(s => s.GetBooking(existingBookingMock.Id)).ReturnsAsync(existingBookingMock);

            var newBookingMock = _fixture.Create<Booking>();
            newBookingMock.Id = existingBookingMock.Id;
            newBookingMock.AccountId = accountId;

            var updateRequest = _fixture.Create<BookingUpdateDto>();

            // return a valid parking spot since we are mocking a successful update
            _bookingRepo.Setup(r => r.CheckIfSpotAvailable(newBookingMock, 2)).ReturnsAsync(true);

            _bookingRepo.Setup(s => s.UpdateBooking(updateRequest)).ReturnsAsync(true);

            var result = await _controller.UpdateBooking(updateRequest);

            Assert.True(result.Value);
        }

        [Fact]
        public async Task UpdateBooking_WithInvalidBooking_ReturnFalse()
        {
            // variables to use to update the booking
            var accountId = new Guid("025afb7c-5483-4abe-b17e-0b3dde9eb75b"); // must be the same for explained above

            var existingBookingMock = _fixture.Create<Booking>();
            existingBookingMock.AccountId = accountId;
            existingBookingMock.StartDate = DateTime.UtcNow - TimeSpan.FromDays(1); // subtract a day from the booking to make it in the past

            // return the existing mock using the accountId generated
            _bookingRepo.Setup(s => s.GetBooking(existingBookingMock.Id)).ReturnsAsync(existingBookingMock);

            // return a valid parking spot since we are mocking a successful update
            _bookingRepo.Setup(r => r.CheckIfSpotAvailable(existingBookingMock, 2)).ReturnsAsync(true);

            var newBookingMock = _fixture.Create<Booking>();
            newBookingMock.Id = existingBookingMock.Id;
            newBookingMock.AccountId = accountId;
            newBookingMock.StartDate = DateTime.UtcNow; // add the current time to the new booking to make it invalid
            // since the booking with the id ended the day prior
            
            var updateRequest = _fixture.Create<BookingUpdateDto>();

            _bookingRepo.Setup(s => s.UpdateBooking(updateRequest)).ReturnsAsync(true);

            var result = await _controller.UpdateBooking(updateRequest);

            Assert.False(result.Value);
        }

        [Fact]
        public async Task DeleteBooking_WithValidId_ReturnOk()
        {
            var bookingMock = _fixture.Create<Booking>();
            _bookingRepo.Setup(s => s.GetBooking(It.IsAny<Guid>())).ReturnsAsync(bookingMock);
            _bookingRepo.Setup(s => s.DeleteBooking(bookingMock.Id)).ReturnsAsync(true);
            var result = await _controller.DeleteBooking(bookingMock.Id);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteBooking_WithInvalid_ReturnNotFound()
        {
            var bookingMock = _fixture.Create<Booking>();
            _bookingRepo.Setup(s => s.GetBooking(It.IsAny<Guid>())).ReturnsAsync(bookingMock);
            var result = await _controller.DeleteBooking(It.IsAny<Guid>());
            Assert.IsType<NotFoundResult>(result);
        }
    }
}