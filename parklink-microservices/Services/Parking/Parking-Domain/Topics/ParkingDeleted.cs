using Parking_Domain.Entities;

namespace Parking_Domain.Topics
{
    public class ParkingDeleted
    {
        public Parking Parking { get; set; }
        public string Event { get; set; }
    }
}
