using AutoMapper;
using Parking_Domain.Entities;

namespace Parking_Grpc.Mapper;

public class ParkingProfileGrpc : Profile
{
    public ParkingProfileGrpc()
    {
        CreateMap<Parking, ParkingModel>().ReverseMap();
    }
}