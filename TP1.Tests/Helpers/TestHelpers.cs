using AutoMapper;
using TP1.Helpers;
using TP1.Models;
using TP1.DTO;

namespace TP1.Tests.Helpers
{
    public static class TestHelpers
    {
        public static IMapper CreateMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });
            return configuration.CreateMapper();
        }

        public static Product CreateTestProduct(int id = 1, string title = "Test Product")
        {
            return new Product
            {
                Id = id,
                Title = title,
                Description = "Test Description",
                Price = 100,
                Quantity = 10,
                MainImagePath = "/images/test.jpg",
                AddedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        public static ProductDTO CreateTestProductDTO(int id = 1, string title = "Test Product", bool inStock = true)
        {
            return new ProductDTO
            {
                Id = id,
                Title = title,
                Description = "Test Description",
                Price = 100,
                InStock = inStock,
                MainImagePath = "/images/test.jpg",
                AddedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }
    }
}

