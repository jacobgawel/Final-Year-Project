using Parking_Domain.Data;
using Parking_Domain.Entities;

namespace Parking_Infrastructure.Repositories;

public interface IParkingRepository
{
    Task<List<Parking>> GetParking();
    Task<Parking?> GetParking(Guid id);
    Task<List<ParkingDistanceDto>> GetClosestParkingSpots(double lat, double lon, TimeSpan duration);
    Task<List<Parking>> GetParkingByProviderId(Guid userId);
    Task<List<Parking>> GetParkingByVerified();
    Task<List<Parking>> GetParkingByCity(string city);
    Task<Guid> CreateParking(Parking parking);
    Task<bool> UpdateParking(ParkingUpdateDto parking);
    Task<bool> DeleteParking(Guid id);
    Task<ParkingHumanizedDto?> GetHumanizedParkingById(Guid id);
}