using System.Linq.Expressions;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using HHMCore.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace HHMCore.Data.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        // ── Single Lookups ────────────────────────────────────────────────

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate)
        {
            IQueryable<T> query = _dbSet;
            return await query.FirstOrDefaultAsync(predicate);
        }

        // ── Collection Lookups ────────────────────────────────────────────

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            IQueryable<T> query = _dbSet;
            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            IQueryable<T> query = _dbSet;
            return await query.Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            IQueryable<T> query = _dbSet;
            return await query.AnyAsync(predicate);
        }

        // ── With Typed Includes ───────────────────────────────────────────

        public async Task<T?> GetByIdWithIncludesAsync(Guid id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IReadOnlyList<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<T>> FindWithIncludesAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.Where(predicate).ToListAsync();
        }

        public async Task<T?> FindOneWithIncludesAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.FirstOrDefaultAsync(predicate);
        }

        // ── With String Path Includes ─────────────────────────────────────

        public async Task<T?> GetByIdWithPathIncludesAsync(Guid id, params string[] paths)
        {
            IQueryable<T> query = _dbSet;
            foreach (var path in paths)
                query = query.Include(path);
            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IReadOnlyList<T>> GetAllWithPathIncludesAsync(params string[] paths)
        {
            IQueryable<T> query = _dbSet;
            foreach (var path in paths)
                query = query.Include(path);
            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<T>> FindWithPathIncludesAsync(Expression<Func<T, bool>> predicate, params string[] paths)
        {
            IQueryable<T> query = _dbSet;
            foreach (var path in paths)
                query = query.Include(path);
            return await query.Where(predicate).ToListAsync();
        }

        public async Task<T?> FindOneWithPathIncludesAsync(Expression<Func<T, bool>> predicate, params string[] paths)
        {
            IQueryable<T> query = _dbSet;
            foreach (var path in paths)
                query = query.Include(path);
            return await query.FirstOrDefaultAsync(predicate);
        }

        // ── Write Operations ──────────────────────────────────────────────

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(T entity)
        {
            entity.IsDeleted = true;
            Update(entity);
        }
    }
}