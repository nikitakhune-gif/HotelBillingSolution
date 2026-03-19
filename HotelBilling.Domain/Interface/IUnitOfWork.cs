using System;
using System.Threading.Tasks;

namespace HotelBilling.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Save changes (commit)
        Task<int> SaveChangesAsync();

        // Optional: Begin Transaction (advanced use)
        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();
    }
}
