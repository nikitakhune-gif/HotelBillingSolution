using System;
using System.Threading;
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

        // Execute an operation inside an execution-strategy-aware transaction.
        // This is required when the DbContext is configured with a retrying
        // execution strategy (e.g. EnableRetryOnFailure). Use this method
        // instead of manually beginning a transaction so the whole unit of
        // work can be retried on transient failures.
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken = default);
    }
}
