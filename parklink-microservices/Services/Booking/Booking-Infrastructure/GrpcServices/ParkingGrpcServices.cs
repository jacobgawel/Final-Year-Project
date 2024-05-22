namespace Booking_Infrastructure.GrpcServices;

public class ParkingGrpcServices : IParkingGrpcServices
{
    private readonly ParkingProtoService.ParkingProtoServiceClient _parkingProtoService;

    public ParkingGrpcServices(ParkingProtoService.ParkingProtoServiceClient parkingProtoService)
    {
        _parkingProtoService = parkingProtoService;
    }

    public async Task<ParkingModel> GetParking(string parkingId)
    {
        var parkingRequest = new GetParkingRequest { ParkingId = parkingId };
        var result = await _parkingProtoService.GetParkingAsync(parkingRequest);
        return result;
    }
}