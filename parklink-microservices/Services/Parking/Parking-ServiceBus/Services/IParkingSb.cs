using Parking_Domain.Entities;

namespace Parking_ServiceBus.Services
{
    public interface IParkingSb
    {
        public Task<bool> ParkingDeleted(Parking obj);
    }
}
