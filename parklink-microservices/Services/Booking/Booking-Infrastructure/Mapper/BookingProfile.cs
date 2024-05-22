using AutoMapper;
using Booking_Domain.Data;
using Booking_Domain.Entities;

namespace Booking_Infrastructure.Mapper;

public class BookingProfile : Profile
{
    public BookingProfile()
    {
        // Booking Entity Mappings
        CreateMap<Booking, BookingHumanizedDto>().ReverseMap();
        CreateMap<Booking, Booking>()
            .ForMember(dest => dest.Id, 
                opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, 
                opt => opt.Ignore())
            .ForMember(dest => dest.RecordId, 
                opt => opt.Ignore())
            .ReverseMap();
        CreateMap<BookingPricing, Booking>().ReverseMap();
        CreateMap<BookingDto, Booking>().ReverseMap();
        CreateMap<BookingUpdateDto, Booking>().ReverseMap();
        
        // Booking Record Entity Mappings
        CreateMap<BookingRecordDto, BookingRecord>().ReverseMap();
        
        // Booking Pricing Entity Mappings
        CreateMap<BookingPricing, BookingPricingInfoDto>().ReverseMap();
        
        // BookingHumanized to BookingRefund humanized fields
        CreateMap<BookingHumanizedDto, BookingRefundDto>() 
            // the refund amount must be ignored otherwise it will be assigned
            // to 0.00 since HumanizedDto inherits UpdateDto, which isn't set
            .ForMember(dest => dest.RefundAmount,
                opt => opt.Ignore())
            .ReverseMap();
    }
}