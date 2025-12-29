using Microsoft.EntityFrameworkCore;
using TP1.Models;

namespace TP1.DataLayer
{
    public static class DataSeeder
    {
        public static async Task SeedProductsAsync(DBContext context, IWebHostEnvironment environment)
        {
            // Check if products already exist
            if (await context.Product.AnyAsync())
            {
                return; // Database has been seeded
            }

            // Get available images from wwwroot/images folder
            var imagesPath = Path.Combine(environment.WebRootPath, "images");
            var availableImages = new List<string>();
            
            if (Directory.Exists(imagesPath))
            {
                var imageFiles = Directory.GetFiles(imagesPath, "*.webp", SearchOption.TopDirectoryOnly)
                    .Select(f => Path.GetFileName(f))
                    .ToList();
                
                availableImages.AddRange(imageFiles);
            }

            // If no images found, use default paths
            if (!availableImages.Any())
            {
                availableImages.AddRange(new[]
                {
                    "/images/airpod.webp",
                    "/images/airpod1.webp",
                    "/images/laptop.webp",
                    "/images/laptop2.webp",
                    "/images/phone1.webp",
                    "/images/phone3.webp"
                });
            }
            else
            {
                // Convert to relative paths
                availableImages = availableImages.Select(img => $"/images/{img}").ToList();
            }

            // Sample product data
            var now = DateTime.Now;
            var random = new Random();
            var products = new List<Product>
            {
                new Product
                {
                    Title = "Apple AirPods Pro",
                    Description = "Active Noise Cancellation, Transparency mode, Spatial Audio with dynamic head tracking, Adaptive EQ, and more. Up to 6 hours of listening time on one charge.",
                    Price = 249.99m,
                    Quantity = random.Next(10, 100),
                    MainImagePath = availableImages[0 % availableImages.Count],
                    AddedAt = now.AddDays(-random.Next(1, 30)),
                    UpdatedAt = now.AddDays(-random.Next(0, 7))
                },
                new Product
                {
                    Title = "Samsung Galaxy S24 Ultra",
                    Description = "6.8-inch Dynamic AMOLED 2X display, Snapdragon 8 Gen 3, 200MP camera, 12GB RAM, 256GB storage. Premium smartphone with S Pen support.",
                    Price = 1199.99m,
                    Quantity = random.Next(10, 100),
                    MainImagePath = availableImages[1 % availableImages.Count],
                    AddedAt = now.AddDays(-random.Next(1, 30)),
                    UpdatedAt = now.AddDays(-random.Next(0, 7))
                },
                new Product
                {
                    Title = "MacBook Pro 16-inch",
                    Description = "M3 Pro chip, 16-core GPU, 18-core CPU, 18GB unified memory, 512GB SSD. Stunning Liquid Retina XDR display. Professional-grade performance.",
                    Price = 2499.99m,
                    Quantity = random.Next(10, 100),
                    MainImagePath = availableImages[2 % availableImages.Count],
                    AddedAt = now.AddDays(-random.Next(1, 30)),
                    UpdatedAt = now.AddDays(-random.Next(0, 7))
                },
                new Product
                {
                    Title = "Dell XPS 15 Laptop",
                    Description = "15.6-inch OLED display, Intel Core i7, 16GB RAM, 512GB SSD, NVIDIA RTX 4050. Premium design with InfinityEdge display.",
                    Price = 1899.99m,
                    Quantity = random.Next(10, 100),
                    MainImagePath = availableImages[3 % availableImages.Count],
                    AddedAt = now.AddDays(-random.Next(1, 30)),
                    UpdatedAt = now.AddDays(-random.Next(0, 7))
                },
                new Product
                {
                    Title = "iPhone 15 Pro Max",
                    Description = "6.7-inch Super Retina XDR display, A17 Pro chip, Pro camera system with 5x Telephoto, Action button, Titanium design.",
                    Price = 1199.99m,
                    Quantity = random.Next(10, 100),
                    MainImagePath = availableImages[4 % availableImages.Count],
                    AddedAt = now.AddDays(-random.Next(1, 30)),
                    UpdatedAt = now.AddDays(-random.Next(0, 7))
                },
                new Product
                {
                    Title = "Sony WH-1000XM5 Headphones",
                    Description = "Industry-leading noise cancellation, 30-hour battery life, Quick Attention mode, Speak-to-Chat, Premium sound quality.",
                    Price = 399.99m,
                    Quantity = random.Next(10, 100),
                    MainImagePath = availableImages[5 % availableImages.Count],
                    AddedAt = now.AddDays(-random.Next(1, 30)),
                    UpdatedAt = now.AddDays(-random.Next(0, 7))
                },
                new Product
                {
                    Title = "iPad Pro 12.9-inch",
                    Description = "M2 chip, 12.9-inch Liquid Retina XDR display, 128GB storage, Wi-Fi, Face ID, Apple Pencil support. Ultimate iPad experience.",
                    Price = 1099.99m,
                    Quantity = random.Next(10, 100),
                    MainImagePath = availableImages[0 % availableImages.Count],
                    AddedAt = now.AddDays(-random.Next(1, 30)),
                    UpdatedAt = now.AddDays(-random.Next(0, 7))
                },
                new Product
                {
                    Title = "Google Pixel 8 Pro",
                    Description = "6.7-inch LTPO OLED display, Google Tensor G3, 50MP main camera, 12GB RAM, 128GB storage. Best-in-class computational photography.",
                    Price = 999.99m,
                    Quantity = random.Next(10, 100),
                    MainImagePath = availableImages[1 % availableImages.Count],
                    AddedAt = now.AddDays(-random.Next(1, 30)),
                    UpdatedAt = now.AddDays(-random.Next(0, 7))
                },
                new Product
                {
                    Title = "Microsoft Surface Laptop Studio 2",
                    Description = "14.4-inch PixelSense Flow display, Intel Core i7, 32GB RAM, 1TB SSD, NVIDIA RTX 4060. Versatile 2-in-1 design.",
                    Price = 2399.99m,
                    Quantity = random.Next(10, 100),
                    MainImagePath = availableImages[2 % availableImages.Count],
                    AddedAt = now.AddDays(-random.Next(1, 30)),
                    UpdatedAt = now.AddDays(-random.Next(0, 7))
                },
                new Product
                {
                    Title = "OnePlus 12",
                    Description = "6.82-inch LTPO AMOLED display, Snapdragon 8 Gen 3, 50MP triple camera, 16GB RAM, 512GB storage. Flagship performance.",
                    Price = 799.99m,
                    Quantity = random.Next(10, 100),
                    MainImagePath = availableImages[3 % availableImages.Count],
                    AddedAt = now.AddDays(-random.Next(1, 30)),
                    UpdatedAt = now.AddDays(-random.Next(0, 7))
                },
                new Product
                {
                    Title = "HP Spectre x360",
                    Description = "13.5-inch OLED touchscreen, Intel Core i7, 16GB RAM, 1TB SSD, Convertible 2-in-1 design. Premium build quality.",
                    Price = 1499.99m,
                    Quantity = random.Next(10, 100),
                    MainImagePath = availableImages[4 % availableImages.Count],
                    AddedAt = now.AddDays(-random.Next(1, 30)),
                    UpdatedAt = now.AddDays(-random.Next(0, 7))
                },
                new Product
                {
                    Title = "Bose QuietComfort Earbuds II",
                    Description = "Industry-leading noise cancellation, CustomTune technology, 6 hours battery, wireless charging case. Premium audio experience.",
                    Price = 279.99m,
                    Quantity = random.Next(10, 100),
                    MainImagePath = availableImages[5 % availableImages.Count],
                    AddedAt = now.AddDays(-random.Next(1, 30)),
                    UpdatedAt = now.AddDays(-random.Next(0, 7))
                }
            };

            // Add products to context
            await context.Product.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}

