using System;
using System.Threading.Tasks;

namespace TP1.DataLayer.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        // IUserRepository Users { get; }
        
        Task<int> SaveChangesAsync();
        int SaveChanges();
    }
}

