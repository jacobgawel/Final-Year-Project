using AutoMapper;
using Fines_Domain.Data;
using Fines_Domain.Entities;

namespace Fines_Infrastructure.Mapper;

public class FinesProfile : Profile
{
    public FinesProfile()
    {
        CreateMap<FineCreateDto, Fine>().ReverseMap();
        // Ignore everything but the description and the imageUri
        CreateMap<FineUpdateDto, Fine>()
            .ForMember(dest => dest.CreatedAt,
                opt => opt.Ignore())
            .ForMember(dest => dest.FineIssuerId,
                opt => opt.Ignore())
            .ForMember(dest => dest.Total,
                opt => opt.Ignore())
            .ForMember(dest => dest.AccountId,
                opt => opt.Ignore())
            .ForMember(dest => dest.BookingId,
                opt => opt.Ignore())
            .ReverseMap();
    }
}