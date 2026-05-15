using System.Linq.Expressions;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using HHMCore.Data.Context;
using Microsoft.EntityFrameworkCore;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // ── Single lookups ────────────────────────────────────────────────────────

    // Fetch by primary key, no tracking
    public async Task<T?> GetByIdAsync(Guid id)
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

    // Fetch first match by predicate, no tracking
    public async Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);

    // Fetch by id with flat (non-nested) includes
    public async Task<T?> GetByIdWithIncludesAsync(Guid id, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    // Fetch by id with full nested include chain (supports ThenInclude)
    public async Task<T?> GetByIdWithDetailsAsync(Guid id, Func<IQueryable<T>, IQueryable<T>> includeChain)
    {
        var query = includeChain(_dbSet.AsNoTracking());
        return await query.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    // Fetch by id using string navigation paths
    public async Task<T?> GetByIdWithPathIncludesAsync(Guid id, params string[] paths)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();
        foreach (var path in paths)
        {
            query = query.Include(path);
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    // Fetch first match by predicate with flat includes
    public async Task<T?> FindOneWithIncludesAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(predicate);
    }

    // Fetch first match by predicate using string paths
    public async Task<T?> FindOneWithPathIncludesAsync(Expression<Func<T, bool>> predicate, params string[] paths)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();
        foreach (var path in paths)
        {
            query = query.Include(path);
        }

        return await query.FirstOrDefaultAsync(predicate);
    }

    // ── Collection lookups ────────────────────────────────────────────────────

    // All records, no tracking
    public async Task<IReadOnlyList<T>> GetAllAsync()
        => await _dbSet.AsNoTracking().ToListAsync();

    // All matching predicate, no tracking
    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AsNoTracking().Where(predicate).ToListAsync();

    // Returns true if any record matches predicate
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AsNoTracking().AnyAsync(predicate);

    // All records with flat includes
    public async Task<IReadOnlyList<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.ToListAsync();
    }

    // All non-deleted records with nested include chain (supports ThenInclude)
    public async Task<IReadOnlyList<T>> GetAllWithDetailsAsync(Func<IQueryable<T>, IQueryable<T>> includeChain)
    {
        var query = includeChain(_dbSet.AsNoTracking());
        return await query.Where(x => !x.IsDeleted).ToListAsync();
    }

    // Filtered non-deleted records with nested include chain (supports ThenInclude)
    public async Task<IReadOnlyList<T>> FindWithDetailsAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>> includeChain)
    {
        var query = includeChain(_dbSet.AsNoTracking());
        return await query.Where(predicate).Where(x => !x.IsDeleted).ToListAsync();
    }

    // Filtered records with flat includes
    public async Task<IReadOnlyList<T>> FindWithIncludesAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.Where(predicate).ToListAsync();
    }

    // All records using string navigation paths
    public async Task<IReadOnlyList<T>> GetAllWithPathIncludesAsync(params string[] paths)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();
        foreach (var path in paths)
        {
            query = query.Include(path);
        }

        return await query.ToListAsync();
    }

    // Filtered records using string navigation paths
    public async Task<IReadOnlyList<T>> FindWithPathIncludesAsync(Expression<Func<T, bool>> predicate, params string[] paths)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();
        foreach (var path in paths)
        {
            query = query.Include(path);
        }

        return await query.Where(predicate).ToListAsync();
    }

    // ── Write operations ──────────────────────────────────────────────────────

    // Insert new entity
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    // Mark entity as modified
    public void Update(T entity) => _dbSet.Entry(entity).State = EntityState.Modified;

    // Soft delete — sets IsDeleted flag
    public void Delete(T entity) { entity.IsDeleted = true; Update(entity); }
}