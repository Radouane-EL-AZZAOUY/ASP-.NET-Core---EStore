using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using TP1.DTO;
using TP1.Pages.Products;
using TP1.Services;
using Xunit;

namespace TP1.Tests.Pages.Products
{
    public class IndexModelTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly CartService _cartService;
        private readonly IndexModel _indexModel;

        public IndexModelTests()
        {
            _mockProductService = new Mock<IProductService>();
            
            // Setup HttpContextAccessor for CartService
            var httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            
            _cartService = new CartService(_mockHttpContextAccessor.Object);
            _indexModel = new IndexModel(_mockProductService.Object, _cartService);
            
            // Setup TempData
            var tempDataProvider = new Mock<ITempDataProvider>();
            var tempDataDictionary = new TempDataDictionary(
                httpContext,
                tempDataProvider.Object);
            _indexModel.TempData = tempDataDictionary;
        }

        [Fact]
        public async Task OnGetAsync_WithoutSearch_ShouldLoadAllProducts()
        {
            // Arrange
            var products = new List<ProductDTO>
            {
                new ProductDTO { Id = 1, Title = "Product 1", Price = 100 },
                new ProductDTO { Id = 2, Title = "Product 2", Price = 200 }
            };

            _mockProductService.Setup(s => s.GetAllProductsAsync())
                .ReturnsAsync(products);

            // Act
            await _indexModel.OnGetAsync(null);

            // Assert
            Assert.NotNull(_indexModel.Product);
            Assert.Equal(2, _indexModel.Product.Count);
            Assert.Equal(2, _indexModel.TotalResults);
            _mockProductService.Verify(s => s.GetAllProductsAsync(), Times.Once);
        }

        [Fact]
        public async Task OnGetAsync_WithSearch_ShouldSearchProducts()
        {
            // Arrange
            var searchTerm = "laptop";
            var products = new List<ProductDTO>
            {
                new ProductDTO { Id = 1, Title = "Laptop 1", Price = 1000 }
            };

            _mockProductService.Setup(s => s.SearchProductsAsync(searchTerm))
                .ReturnsAsync(products);

            // Act
            await _indexModel.OnGetAsync(searchTerm);

            // Assert
            Assert.Equal(searchTerm, _indexModel.SearchQuery);
            Assert.Single(_indexModel.Product);
            _mockProductService.Verify(s => s.SearchProductsAsync(searchTerm), Times.Once);
        }

        [Fact]
        public async Task OnPostAddToCartAsync_ExistingProduct_ShouldAddToCart()
        {
            // Arrange
            var productId = 1;
            var searchTerm = "laptop";
            var product = new ProductDTO
            {
                Id = productId,
                Title = "Laptop",
                Price = 1000,
                MainImagePath = "/images/laptop.jpg"
            };

            _mockProductService.Setup(s => s.GetProductByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _indexModel.OnPostAddToCart(productId, searchTerm);

            // Assert
            Assert.IsType<RedirectToPageResult>(result);
            var redirectResult = result as RedirectToPageResult;
            Assert.NotNull(redirectResult);
            // RedirectToPage() without page name redirects to the same page (Index), which sets PageName to null or empty
            Assert.True(string.IsNullOrEmpty(redirectResult.PageName) || redirectResult.PageName == "Index");
            
            // Verify product service was called
            _mockProductService.Verify(s => s.GetProductByIdAsync(productId), Times.Once);
            
            // Verify TempData message was set
            Assert.NotNull(_indexModel.TempData["CartMessage"]);
            Assert.Contains("Laptop", _indexModel.TempData["CartMessage"]!.ToString()!);
        }

        [Fact]
        public async Task OnPostAddToCartAsync_WithSearch_ShouldPreserveSearchQuery()
        {
            // Arrange
            var productId = 1;
            var searchTerm = "test";
            var product = new ProductDTO
            {
                Id = productId,
                Title = "Test Product",
                Price = 100,
                MainImagePath = "/images/test.jpg"
            };

            _mockProductService.Setup(s => s.GetProductByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _indexModel.OnPostAddToCart(productId, searchTerm);

            // Assert
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            var routeValues = redirectResult.RouteValues;
            Assert.NotNull(routeValues);
            Assert.Equal(searchTerm, routeValues["search"]);
        }

        [Fact]
        public async Task OnPostAddToCartAsync_NonExistingProduct_ShouldRedirectWithoutError()
        {
            // Arrange
            var productId = 999;
            _mockProductService.Setup(s => s.GetProductByIdAsync(productId))
                .ReturnsAsync((ProductDTO?)null);

            // Act
            var result = await _indexModel.OnPostAddToCart(productId, null);

            // Assert
            Assert.IsType<RedirectToPageResult>(result);
            _mockProductService.Verify(s => s.GetProductByIdAsync(productId), Times.Once);
        }
    }
}

