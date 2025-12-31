using AutoMapper;
using EcoRoute.Models;
using EcoRoute.Models.Entities;

namespace EcoRoute.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<OrderRequestDto, OrderDto>()
                .ForMember(dest => dest.SelectedRouteSummary, opt => opt.Ignore())
                .ForMember(dest => dest.OrderDistance, opt => opt.Ignore())
                .ForMember(dest => dest.TransportVehicle, opt => opt.Ignore())
                .ForMember(dest => dest.TransportMode, opt => opt.Ignore())
                .ForMember(dest => dest.OrderStatus, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.RouteDuration, opt => opt.Ignore());
        
            CreateMap<OrderDto, Order>()
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.OrderStatus, opt => opt.Ignore())
                .ForMember(dest => dest.OrderCO2Emission, opt => opt.Ignore());

            CreateMap<Order, OrderHistoryDto>()
                .ForMember(dest => dest.ShipmentCode, opt => opt.Ignore());

            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id));

            // CreateMap<
                
        }


    }
}