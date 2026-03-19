using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HotelBilling.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // Get All
        Task<IEnumerable<T>> GetAllAsync();

        // Get By Id
        Task<T?> GetByIdAsync(int id);

        // Find with condition
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        // Add
        Task AddAsync(T entity);

        // Add Range
        Task AddRangeAsync(IEnumerable<T> entities);

        // Update
        void Update(T entity);

        // Delete
        void Delete(T entity);

        // Delete by Id
        Task DeleteByIdAsync(int id);

        // Any check (exists)
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        // Count
        Task<int> CountAsync();
    }
}
