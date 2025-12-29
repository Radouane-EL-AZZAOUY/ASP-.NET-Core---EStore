using Microsoft.EntityFrameworkCore;
using TP1.DataLayer.Interfaces;
using TP1.Models;

namespace TP1.DataLayer.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(DBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllAsync();
            }

            var term = searchTerm.Trim();
            return await _dbSet
                .Where(p =>
                    (p.Title != null && EF.Functions.Like(p.Title, $"%{term}%")) ||
                    (p.Description != null && EF.Functions.Like(p.Description, $"%{term}%"))
                )
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAvailableProductsAsync()
        {
            return await _dbSet
                .Where(p => p.Quantity > 0)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetRecommendedProductsAsync(int count = 5)
        {
            return await _dbSet
                .Where(p => p.Quantity > 0)
                .OrderByDescending(p => p.AddedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}

