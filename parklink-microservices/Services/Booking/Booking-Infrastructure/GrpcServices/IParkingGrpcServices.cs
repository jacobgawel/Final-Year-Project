using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booking_Infrastructure.GrpcServices
{
    public interface IParkingGrpcServices
    {
        Task<ParkingModel> GetParking(string parkingId);
    }
}
