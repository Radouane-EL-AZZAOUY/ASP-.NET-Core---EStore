using AutoMapper;
using TP1.Models;
using TP1.DTO;

namespace TP1.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Map Product to ProductDTO: Convert Quantity to InStock (hides sensitive data)
            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.InStock, opt => opt.MapFrom(src => src.Quantity > 0));
            
            // Reverse mapping: ProductDTO -> Product (Quantity defaults to 0, should not be used)
            CreateMap<ProductDTO, Product>()
                .ForMember(dest => dest.Quantity, opt => opt.Ignore());
            
            CreateMap<Product, CreateProductDTO>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AddedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            
            CreateMap<Product, UpdateProductDTO>()
                .ReverseMap()
                .ForMember(dest => dest.AddedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}
