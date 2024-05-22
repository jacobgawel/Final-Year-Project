using AutoMapper;
using Booking_Domain.Entities;

namespace Booking_Grpc.Mapper
{
    public class BookingProfileGrpc : Profile
    {
        public BookingProfileGrpc() 
        { 
            CreateMap<Booking, BookingModel>().ReverseMap();
        }
    }
}
