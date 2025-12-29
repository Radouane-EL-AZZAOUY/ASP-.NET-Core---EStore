using TP1.DataLayer;
using TP1.DataLayer.Interfaces;
using TP1.DataLayer.Repositories;

namespace TP1.DataLayer.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DBContext _context;
        private IProductRepository? _products;

        public UnitOfWork(DBContext context)
        {
            _context = context;
        }

        public IProductRepository Products
        {
            get
            {
                _products ??= new ProductRepository(_context);
                return _products;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
