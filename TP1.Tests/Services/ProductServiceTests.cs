using AutoMapper;
using Moq;
using TP1.DataLayer.Interfaces;
using TP1.DTO;
using TP1.Models;
using TP1.Services;
using Xunit;

namespace TP1.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockCacheService = new Mock<ICacheService>();
            
            _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);
            
            // Par défaut, le cache retourne null (cache miss)
            _mockCacheService.Setup(c => c.GetAsync<List<ProductDTO>>(It.IsAny<string>()))
                .ReturnsAsync((List<ProductDTO>?)null);
            _mockCacheService.Setup(c => c.GetAsync<ProductDTO>(It.IsAny<string>()))
                .ReturnsAsync((ProductDTO?)null);
            
            // Setup new Redis methods with default return values
            _mockCacheService.Setup(c => c.RemoveByPatternAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockCacheService.Setup(c => c.KeyExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _mockCacheService.Setup(c => c.GetKeysCountAsync(It.IsAny<string>()))
                .ReturnsAsync(0);
            _mockCacheService.Setup(c => c.GetTtlAsync(It.IsAny<string>()))
                .ReturnsAsync((TimeSpan?)null);
            
            _productService = new ProductService(_mockUnitOfWork.Object, _mockMapper.Object, _mockCacheService.Object);
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Price = 100 },
                new Product { Id = 2, Title = "Product 2", Price = 200 }
            };

            var productDTOs = new List<ProductDTO>
            {
                new ProductDTO { Id = 1, Title = "Product 1", Price = 100 },
                new ProductDTO { Id = 2, Title = "Product 2", Price = 200 }
            };

            _mockProductRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(products);
            _mockMapper.Setup(m => m.Map<IEnumerable<ProductDTO>>(products))
                .Returns(productDTOs);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockProductRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<IEnumerable<ProductDTO>>(products), Times.Once);
            
            // Verify cache was set after cache miss
            _mockCacheService.Verify(c => c.SetAsync("products:all", It.IsAny<List<ProductDTO>>(), It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        public async Task GetAllProductsAsync_CacheHit_ShouldReturnCachedProducts()
        {
            // Arrange
            var cachedProducts = new List<ProductDTO>
            {
                new ProductDTO { Id = 1, Title = "Cached Product 1", Price = 100 },
                new ProductDTO { Id = 2, Title = "Cached Product 2", Price = 200 }
            };

            _mockCacheService.Setup(c => c.GetAsync<List<ProductDTO>>("products:all"))
                .ReturnsAsync(cachedProducts);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Cached Product 1", result.First().Title);
            
            // Verify cache was used (no database call)
            _mockProductRepository.Verify(r => r.GetAllAsync(), Times.Never);
            _mockCacheService.Verify(c => c.GetAsync<List<ProductDTO>>("products:all"), Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_ExistingId_ShouldReturnProduct()
        {
            // Arrange
            var productId = 1;
            var product = new Product { Id = productId, Title = "Product 1", Price = 100 };
            var productDTO = new ProductDTO { Id = productId, Title = "Product 1", Price = 100 };

            _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _mockMapper.Setup(m => m.Map<ProductDTO>(product))
                .Returns(productDTO);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            _mockProductRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
            
            // Verify cache was set after cache miss
            _mockCacheService.Verify(c => c.SetAsync($"product:{productId}", It.IsAny<ProductDTO>(), It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_NonExistingId_ShouldReturnNull()
        {
            // Arrange
            var productId = 999;
            _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.Null(result);
            _mockProductRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
            _mockMapper.Verify(m => m.Map<ProductDTO>(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task CreateProductAsync_ShouldCreateAndReturnProductDTO()
        {
            // Arrange
            var createDto = new CreateProductDTO
            {
                Title = "New Product",
                Price = 150,
                Quantity = 10
            };

            var product = new Product
            {
                Id = 1,
                Title = createDto.Title,
                Price = createDto.Price,
                Quantity = createDto.Quantity,
                AddedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var productDTO = new ProductDTO
            {
                Id = 1,
                Title = createDto.Title,
                Price = createDto.Price,
                InStock = createDto.Quantity > 0
            };

            _mockMapper.Setup(m => m.Map<Product>(createDto))
                .Returns(product);
            _mockProductRepository.Setup(r => r.AddAsync(product))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<ProductDTO>(product))
                .Returns(productDTO);

            // Act
            var result = await _productService.CreateProductAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createDto.Title, result.Title);
            _mockProductRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            
            // Verify cache invalidation was called
            _mockCacheService.Verify(c => c.RemoveAsync("products:all"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveByPatternAsync("products:recommended:*"), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_ExistingProduct_ShouldUpdateAndReturnProductDTO()
        {
            // Arrange
            var updateDto = new UpdateProductDTO
            {
                Id = 1,
                Title = "Updated Product",
                Price = 200
            };

            var existingProduct = new Product { Id = 1, Title = "Original", Price = 100 };
            var updatedProductDTO = new ProductDTO { Id = 1, Title = "Updated Product", Price = 200 };

            _mockProductRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingProduct);
            _mockMapper.Setup(m => m.Map(updateDto, existingProduct))
                .Callback<UpdateProductDTO, Product>((dto, prod) =>
                {
                    prod.Title = dto.Title;
                    prod.Price = dto.Price;
                });
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<ProductDTO>(existingProduct))
                .Returns(updatedProductDTO);

            // Act
            var result = await _productService.UpdateProductAsync(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Product", result.Title);
            _mockProductRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
            _mockProductRepository.Verify(r => r.Update(existingProduct), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            
            // Verify cache invalidation was called
            _mockCacheService.Verify(c => c.RemoveAsync("products:all"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync("product:1"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveByPatternAsync("products:recommended:*"), Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_ExistingProduct_ShouldReturnTrue()
        {
            // Arrange
            var productId = 1;
            var product = new Product { Id = productId, Title = "Product" };

            _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _productService.DeleteProductAsync(productId);

            // Assert
            Assert.True(result);
            _mockProductRepository.Verify(r => r.Remove(product), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            
            // Verify cache invalidation was called
            _mockCacheService.Verify(c => c.RemoveAsync("products:all"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveAsync("product:1"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveByPatternAsync("products:recommended:*"), Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_NonExistingProduct_ShouldReturnFalse()
        {
            // Arrange
            var productId = 999;
            _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.DeleteProductAsync(productId);

            // Assert
            Assert.False(result);
            _mockProductRepository.Verify(r => r.Remove(It.IsAny<Product>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task SearchProductsAsync_ShouldReturnMatchingProducts()
        {
            // Arrange
            var searchTerm = "laptop";
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Laptop 1", Price = 1000 }
            };

            var productDTOs = new List<ProductDTO>
            {
                new ProductDTO { Id = 1, Title = "Laptop 1", Price = 1000 }
            };

            _mockProductRepository.Setup(r => r.SearchAsync(searchTerm))
                .ReturnsAsync(products);
            _mockMapper.Setup(m => m.Map<IEnumerable<ProductDTO>>(products))
                .Returns(productDTOs);

            // Act
            var result = await _productService.SearchProductsAsync(searchTerm);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _mockProductRepository.Verify(r => r.SearchAsync(searchTerm), Times.Once);
        }

        [Fact]
        public async Task GetRecommendedProductsAsync_ShouldReturnLimitedProducts()
        {
            // Arrange
            var count = 5;
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Price = 100, Quantity = 5 },
                new Product { Id = 2, Title = "Product 2", Price = 200, Quantity = 3 }
            };

            var productDTOs = new List<ProductDTO>
            {
                new ProductDTO { Id = 1, Title = "Product 1", Price = 100, InStock = true },
                new ProductDTO { Id = 2, Title = "Product 2", Price = 200, InStock = true }
            };

            _mockProductRepository.Setup(r => r.GetRecommendedProductsAsync(count))
                .ReturnsAsync(products);
            _mockMapper.Setup(m => m.Map<IEnumerable<ProductDTO>>(products))
                .Returns(productDTOs);

            // Act
            var result = await _productService.GetRecommendedProductsAsync(count);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockProductRepository.Verify(r => r.GetRecommendedProductsAsync(count), Times.Once);
            
            // Verify cache was set with shorter expiration (30 minutes)
            _mockCacheService.Verify(c => c.SetAsync($"products:recommended:{count}", It.IsAny<List<ProductDTO>>(), TimeSpan.FromMinutes(30)), Times.Once);
        }
    }
}

