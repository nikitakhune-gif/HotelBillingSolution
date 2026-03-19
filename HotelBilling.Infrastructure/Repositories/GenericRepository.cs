using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using HotelBilling.Domain.Interfaces;
using HotelBilling.Infrastructure.Data.DbContext;

namespace HotelBilling.Infrastructure.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        // Get All
        public async Task<IEnumerable<T>> GetAllAsync()
            => await _dbSet.ToListAsync();

        // Get By Id
        public async Task<T?> GetByIdAsync(int id)
            => await _dbSet.FindAsync(id);

        // Find (Where condition)
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        // Add
        public async Task AddAsync(T entity)
            => await _dbSet.AddAsync(entity);

        // Add Range
        public async Task AddRangeAsync(IEnumerable<T> entities)
            => await _dbSet.AddRangeAsync(entities);

        // Update
        public void Update(T entity)
            => _dbSet.Update(entity);

        // Delete
        public void Delete(T entity)
            => _dbSet.Remove(entity);

        // Delete by Id
        public async Task DeleteByIdAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
                Delete(entity);
        }

        // Exists
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.AnyAsync(predicate);

        // Count
        public async Task<int> CountAsync()
            => await _dbSet.CountAsync();
    }
}