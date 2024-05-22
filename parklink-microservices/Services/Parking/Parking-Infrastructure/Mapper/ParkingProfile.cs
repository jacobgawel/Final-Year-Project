using AutoMapper;
using Parking_Domain.Data;
using Parking_Domain.Entities;

namespace Parking_Infrastructure.Mapper;

public class ParkingProfile : Profile
{
    public ParkingProfile()
    {
        CreateMap<Parking, Parking>().ReverseMap();
        
        CreateMap<ParkingUpdateDto, Parking>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.VerificationDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastEditDate, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<Parking, ParkingHumanizedDto>().ReverseMap();
    }
}