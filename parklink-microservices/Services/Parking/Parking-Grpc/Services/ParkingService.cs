using AutoMapper;
using Grpc.Core;
using Parking_Infrastructure.Repositories;

namespace Parking_Grpc.Services
{
    public class ParkingService : ParkingProtoService.ParkingProtoServiceBase
    {
        private readonly ILogger<ParkingService> _logger;
        private readonly IMapper _mapper;
        private readonly IParkingRepository _parkingRepository;

        public ParkingService(IParkingRepository parkingRepository, ILogger<ParkingService> logger, IMapper mapper)
        {
            _parkingRepository = parkingRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<ParkingModel> GetParking(GetParkingRequest request, ServerCallContext context)
        {
            Guid parkingId = new Guid(request.ParkingId);
            var parking = await _parkingRepository.GetParking(parkingId);
            
            if (parking == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Parking with Id={request.ParkingId} - NOT FOUND"));
            }
            
            _logger.LogInformation($"Parking with Id=: {request.ParkingId} - FOUND");

            var parkingModel = _mapper.Map<ParkingModel>(parking);
            return parkingModel;
        }
    }
}
