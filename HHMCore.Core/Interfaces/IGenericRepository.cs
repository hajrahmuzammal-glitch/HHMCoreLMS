using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HHMCore.Core.Entities;
using System.Linq.Expressions;

namespace HHMCore.Core.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
 
        Task<T?> GetByIdWithIncludesAsync(Guid id, params Expression<Func<T, object>>[] includes);
        Task<IReadOnlyList<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes);
        Task<IReadOnlyList<T>> FindWithIncludesAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<T?> FindOneWithIncludesAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);

        Task<T?> GetByIdWithPathIncludesAsync(Guid id, params string[] paths);
        Task<IReadOnlyList<T>> GetAllWithPathIncludesAsync(params string[] paths);
        Task<IReadOnlyList<T>> FindWithPathIncludesAsync(Expression<Func<T, bool>> predicate, params string[] paths);
        Task<T?> FindOneWithPathIncludesAsync(Expression<Func<T, bool>> predicate, params string[] paths);


        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
//Note : It is handling the CRUD