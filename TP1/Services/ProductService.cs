using AutoMapper;
using TP1.DataLayer.Interfaces;
using TP1.DTO;
using TP1.Models;

namespace TP1.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
        Task<ProductDTO?> GetProductByIdAsync(int id);
        Task<IEnumerable<ProductDTO>> SearchProductsAsync(string searchTerm);
        Task<ProductDTO> CreateProductAsync(CreateProductDTO createDto);
        Task<ProductDTO?> UpdateProductAsync(UpdateProductDTO updateDto);
        Task<bool> DeleteProductAsync(int id);
        Task<IEnumerable<ProductDTO>> GetRecommendedProductsAsync(int count = 5);
    }

    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private const string CacheKeyPrefix = "product:";
        private const string AllProductsCacheKey = "products:all";
        private const string RecommendedProductsCacheKey = "products:recommended";
        private static readonly TimeSpan DefaultCacheExpiration = TimeSpan.FromHours(1);

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            // Essayer de récupérer depuis le cache
            var cachedProducts = await _cacheService.GetAsync<List<ProductDTO>>(AllProductsCacheKey);
            if (cachedProducts != null)
            {
                return cachedProducts;
            }

            // Cache miss : récupérer depuis la base de données
            var products = await _unitOfWork.Products.GetAllAsync();
            var productDTOs = _mapper.Map<IEnumerable<ProductDTO>>(products).ToList();

            // Mettre en cache
            await _cacheService.SetAsync(AllProductsCacheKey, productDTOs, DefaultCacheExpiration);

            return productDTOs;
        }

        public async Task<ProductDTO?> GetProductByIdAsync(int id)
        {
            var cacheKey = $"{CacheKeyPrefix}{id}";

            // Essayer de récupérer depuis le cache
            var cachedProduct = await _cacheService.GetAsync<ProductDTO>(cacheKey);
            if (cachedProduct != null)
            {
                return cachedProduct;
            }

            // Cache miss : récupérer depuis la base de données
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return null;
            }

            var productDTO = _mapper.Map<ProductDTO>(product);

            // Mettre en cache
            await _cacheService.SetAsync(cacheKey, productDTO, DefaultCacheExpiration);

            return productDTO;
        }

        public async Task<IEnumerable<ProductDTO>> SearchProductsAsync(string searchTerm)
        {
            // Les recherches ne sont généralement pas mises en cache car elles sont très variées
            var products = await _unitOfWork.Products.SearchAsync(searchTerm);
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public async Task<ProductDTO> CreateProductAsync(CreateProductDTO createDto)
        {
            var product = _mapper.Map<Product>(createDto);
            product.AddedAt = DateTime.Now;
            product.UpdatedAt = DateTime.Now;

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var productDTO = _mapper.Map<ProductDTO>(product);

            // Invalider les caches liés
            await InvalidateProductCachesAsync();

            return productDTO;
        }

        public async Task<ProductDTO?> UpdateProductAsync(UpdateProductDTO updateDto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(updateDto.Id);
            if (product == null)
            {
                return null;
            }

            _mapper.Map(updateDto, product);
            product.UpdatedAt = DateTime.Now;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();

            var productDTO = _mapper.Map<ProductDTO>(product);

            // Invalider les caches
            await InvalidateProductCachesAsync(product.Id);

            return productDTO;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return false;
            }

            _unitOfWork.Products.Remove(product);
            await _unitOfWork.SaveChangesAsync();

            // Invalider les caches
            await InvalidateProductCachesAsync(id);

            return true;
        }

        public async Task<IEnumerable<ProductDTO>> GetRecommendedProductsAsync(int count = 5)
        {
            var cacheKey = $"{RecommendedProductsCacheKey}:{count}";

            // Essayer de récupérer depuis le cache
            var cachedProducts = await _cacheService.GetAsync<List<ProductDTO>>(cacheKey);
            if (cachedProducts != null)
            {
                return cachedProducts;
            }

            // Cache miss : récupérer depuis la base de données
            var products = await _unitOfWork.Products.GetRecommendedProductsAsync(count);
            var productDTOs = _mapper.Map<IEnumerable<ProductDTO>>(products).ToList();

            // Mettre en cache avec une expiration plus courte (30 minutes)
            await _cacheService.SetAsync(cacheKey, productDTOs, TimeSpan.FromMinutes(30));

            return productDTOs;
        }

        private async Task InvalidateProductCachesAsync(int? productId = null)
        {
            // Invalider le cache de tous les produits
            await _cacheService.RemoveAsync(AllProductsCacheKey);

            // Invalider le cache du produit spécifique
            if (productId.HasValue)
            {
                await _cacheService.RemoveAsync($"{CacheKeyPrefix}{productId.Value}");
            }

            // Invalider les caches des produits recommandés
            await _cacheService.RemoveByPatternAsync($"{RecommendedProductsCacheKey}:*");
        }
    }
}

