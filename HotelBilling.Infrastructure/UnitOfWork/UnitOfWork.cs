using HotelBilling.Domain.Interfaces;
using HotelBilling.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HotelBilling.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        // Save changes (Commit without explicit transaction)
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Begin Transaction
        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
                _transaction = await _context.Database.BeginTransactionAsync();
        }

        // Commit Transaction
        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();

                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }

        // Rollback Transaction
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        // ---------------------------------------------------------------
        // NEW: ExecuteInTransactionAsync
        //
        // WHY THIS WAS ADDED: BookingService.CreateBookingAsync (and any
        // other service using BeginTransactionAsync/CommitTransactionAsync
        // manually) was throwing:
        //   "The configured execution strategy 'SqlServerRetryingExecutionStrategy'
        //    does not support user-initiated transactions."
        // That's because AppDbContext is configured with
        // EnableRetryOnFailure(...) (a retrying execution strategy), and EF
        // Core does not allow a transaction to be started manually
        // (Database.BeginTransactionAsync) outside of that strategy's own
        // retry loop — if a transient fault happens mid-transaction, EF
        // needs to be able to retry the WHOLE unit of work, which it can't
        // do if the transaction was opened outside its control.
        //
        // FIX: CreateExecutionStrategy().ExecuteAsync(...) runs the given
        // delegate — including opening/committing/rolling back its own
        // transaction inside that delegate — as a single retriable unit.
        // Callers that need "begin work, save several related entities,
        // commit as one unit" should use THIS method instead of manually
        // calling BeginTransactionAsync/CommitTransactionAsync/
        // RollbackTransactionAsync.
        // ---------------------------------------------------------------
        public async Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<Task<TResult>> operation,
            CancellationToken cancellationToken = default)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var result = await operation();
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        // Dispose
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}