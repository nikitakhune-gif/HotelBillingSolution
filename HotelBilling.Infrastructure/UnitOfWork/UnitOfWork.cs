using HotelBilling.Domain.Interfaces;
using HotelBilling.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        // Dispose
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}