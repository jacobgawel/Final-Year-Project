using MassTransit;
using Parking_Domain.Entities;
using Parking_Domain.Topics;

namespace Parking_ServiceBus.Services
{
    public class ParkingSb : IParkingSb
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public ParkingSb(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }
        

        public async Task<bool> ParkingDeleted(Parking parking)
        {
            // Creates the deleted event object

            var parkingDeletedEvent = new ParkingDeleted
            {
                Parking = parking,
                Event = "Deleted"
            };

            await _publishEndpoint.Publish(parkingDeletedEvent);

            return true;
        }
    }
}
