using Microsoft.EntityFrameworkCore;
using TP1.DataLayer;
using TP1.DataLayer.Repositories;
using TP1.Models;
using Xunit;

namespace TP1.Tests.Repositories
{
    public class ProductRepositoryTests : IDisposable
    {
        private readonly DBContext _context;
        private readonly ProductRepository _repository;

        public ProductRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<DBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DBContext(options);
            _repository = new ProductRepository(_context);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllProducts()
        {
            // Arrange
            await SeedProducts();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ExistingId_ShouldReturnProduct()
        {
            // Arrange
            await SeedProducts();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Product 1", result.Title);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ShouldReturnNull()
        {
            // Arrange
            await SeedProducts();

            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SearchAsync_WithMatchingTitle_ShouldReturnMatchingProducts()
        {
            // Arrange
            await SeedProducts();

            // Act
            var result = await _repository.SearchAsync("Product 1");

            // Assert
            Assert.Single(result);
            Assert.Contains(result, p => p.Title == "Product 1");
        }

        [Fact]
        public async Task SearchAsync_WithMatchingDescription_ShouldReturnMatchingProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product A", Description = "Laptop computer", Price = 1000, Quantity = 5, AddedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Product { Id = 2, Title = "Product B", Description = "Desktop computer", Price = 800, Quantity = 3, AddedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            };
            _context.Product.AddRange(products);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.SearchAsync("computer");

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task SearchAsync_EmptySearchTerm_ShouldReturnAllProducts()
        {
            // Arrange
            await SeedProducts();

            // Act
            var result = await _repository.SearchAsync("");

            // Assert
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetAvailableProductsAsync_ShouldReturnOnlyInStockProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Price = 100, Quantity = 5, AddedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Product { Id = 2, Title = "Product 2", Price = 200, Quantity = 0, AddedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Product { Id = 3, Title = "Product 3", Price = 300, Quantity = 10, AddedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            };
            _context.Product.AddRange(products);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAvailableProductsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.True(p.Quantity > 0));
        }

        [Fact]
        public async Task GetRecommendedProductsAsync_ShouldReturnLimitedProducts()
        {
            // Arrange
            var products = new List<Product>();
            for (int i = 1; i <= 10; i++)
            {
                products.Add(new Product
                {
                    Id = i,
                    Title = $"Product {i}",
                    Price = 100 * i,
                    Quantity = 5,
                    AddedAt = DateTime.Now.AddDays(-i),
                    UpdatedAt = DateTime.Now
                });
            }
            _context.Product.AddRange(products);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRecommendedProductsAsync(5);

            // Assert
            Assert.Equal(5, result.Count());
            // Should be ordered by AddedAt descending (most recent first)
            var dates = result.Select(p => p.AddedAt).ToList();
            Assert.True(dates[0] >= dates[1]);
        }

        [Fact]
        public async Task AddAsync_ShouldAddProduct()
        {
            // Arrange
            var product = new Product
            {
                Title = "New Product",
                Price = 150,
                Quantity = 10,
                AddedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // Act
            await _repository.AddAsync(product);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Product.FindAsync(product.Id);
            Assert.NotNull(result);
            Assert.Equal("New Product", result.Title);
        }

        [Fact]
        public async Task Update_ShouldUpdateProduct()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Title = "Original",
                Price = 100,
                Quantity = 5,
                AddedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Product.Add(product);
            await _context.SaveChangesAsync();

            // Act
            product.Title = "Updated";
            product.Price = 200;
            _repository.Update(product);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Product.FindAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Updated", result.Title);
            Assert.Equal(200, result.Price);
        }

        [Fact]
        public async Task Remove_ShouldRemoveProduct()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Title = "Product to Remove",
                Price = 100,
                Quantity = 5,
                AddedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Product.Add(product);
            await _context.SaveChangesAsync();

            // Act
            _repository.Remove(product);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Product.FindAsync(1);
            Assert.Null(result);
        }

        private async Task SeedProducts()
        {
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Price = 100, Quantity = 5, AddedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Product { Id = 2, Title = "Product 2", Price = 200, Quantity = 3, AddedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Product { Id = 3, Title = "Product 3", Price = 300, Quantity = 0, AddedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            };

            _context.Product.AddRange(products);
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

