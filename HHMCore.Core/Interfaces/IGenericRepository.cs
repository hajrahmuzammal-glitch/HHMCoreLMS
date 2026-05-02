using System.Linq.Expressions;
using HHMCore.Core.Entities;

namespace HHMCore.Core.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    // ── Single lookups ────────────────────────────────────────────────────

    Task<T?> GetByIdAsync(Guid id);

    Task<T?> FindOneAsync(
        Expression<Func<T, bool>> predicate);

    Task<T?> GetByIdWithIncludesAsync(
        Guid id,
        params Expression<Func<T, object>>[] includes);

    Task<T?> GetByIdWithDetailsAsync(
        Guid id,
        Func<IQueryable<T>, IQueryable<T>> includeChain);

    Task<T?> GetByIdWithPathIncludesAsync(
        Guid id,
        params string[] paths);

    Task<T?> FindOneWithIncludesAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes);

    Task<T?> FindOneWithPathIncludesAsync(
        Expression<Func<T, bool>> predicate,
        params string[] paths);

    // ── Collection lookups ────────────────────────────────────────────────

    Task<IReadOnlyList<T>> GetAllAsync();

    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate);

    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate);

    Task<IReadOnlyList<T>> GetAllWithIncludesAsync(
        params Expression<Func<T, object>>[] includes);

    Task<IReadOnlyList<T>> GetAllWithDetailsAsync(
        Func<IQueryable<T>, IQueryable<T>> includeChain);

    Task<IReadOnlyList<T>> FindWithDetailsAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>> includeChain);

    Task<IReadOnlyList<T>> FindWithIncludesAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes);

    Task<IReadOnlyList<T>> GetAllWithPathIncludesAsync(
        params string[] paths);

    Task<IReadOnlyList<T>> FindWithPathIncludesAsync(
        Expression<Func<T, bool>> predicate,
        params string[] paths);

    // ── Write operations ──────────────────────────────────────────────────

    Task AddAsync(T entity);

    void Update(T entity);

    void Delete(T entity);
}