using BitByBit.DataAccess.Repository.Interfaces;
using BitByBit.Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BitByBit.DataAccess.Context;

namespace BitByBit.DataAccess.Repository.Implementations
{
    public class Repository<T> : IRepository<T> where T : BaseEntity, new()
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public DbSet<T> Table => _dbSet;

        #region Basic CRUD Operations

        // Create
        public async Task<T> AddAsync(T entity)
        {
            entity.CreatedDate = DateTime.Now;
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entity.CreatedDate = DateTime.Now;
            }
            await _dbSet.AddRangeAsync(entities);
            return entities;
        }

        // Read - Single
        public async Task<T?> GetByIdAsync(int id, bool isTracking = true)
        {
            return await GetByIdAsync(id, isTracking);
        }

        public async Task<T?> GetByIdAsync(int id, bool isTracking = true, params string[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
        }

        public async Task<T?> GetSingleAsync(Expression<Func<T, bool>> condition, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted).Where(condition).SingleOrDefaultAsync();
        }

        public async Task<T?> GetSingleAsync(Expression<Func<T, bool>> condition, bool isTracking = true, params string[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted).Where(condition).SingleOrDefaultAsync();
        }

        public async Task<T?> GetFirstAsync(Expression<Func<T, bool>> condition, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted).Where(condition).FirstOrDefaultAsync();
        }

        public async Task<T?> GetFirstAsync(Expression<Func<T, bool>> condition, bool isTracking = true, params string[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted).Where(condition).FirstOrDefaultAsync();
        }

        public async Task<T?> GetLastAsync(Expression<Func<T, bool>> condition, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted).Where(condition).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
        }

        public async Task<T?> GetLastAsync(Expression<Func<T, bool>> condition, bool isTracking = true, params string[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted).Where(condition).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
        }

        // Read - Multiple
        public async Task<IEnumerable<T>> GetAllAsync(bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(bool isTracking = true, params string[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(int limit, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted).Take(limit).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(int limit, bool isTracking = true, params string[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted).Take(limit).ToListAsync();
        }

        public IQueryable<T> GetAll(bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return query.Where(x => !x.IsDeleted);
        }

        public IQueryable<T> GetAll(bool isTracking = true, params string[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (!isTracking)
                query = query.AsNoTracking();

            return query.Where(x => !x.IsDeleted);
        }

        // Update
        public void Update(T entity)
        {
            entity.UpdatedDate = DateTime.Now;
            _dbSet.Update(entity);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entity.UpdatedDate = DateTime.Now;
            }
            _dbSet.UpdateRange(entities);
        }

        public async Task UpdateAsync(T entity)
        {
            entity.UpdatedDate = DateTime.Now;
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entity.UpdatedDate = DateTime.Now;
            }
            _dbSet.UpdateRange(entities);
            await Task.CompletedTask;
        }

        // Delete
        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public async Task<bool> DeleteByIdAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                Delete(entity);
                return true;
            }
            return false;
        }

        public async Task<int> DeleteByConditionAsync(Expression<Func<T, bool>> condition)
        {
            var entities = await _dbSet.Where(x => !x.IsDeleted).Where(condition).ToListAsync();
            DeleteRange(entities);
            return entities.Count;
        }

        public void SoftDelete(T entity)
        {
            entity.IsDeleted = true;
            entity.UpdatedDate = DateTime.Now;
            Update(entity);
        }

        public void SoftDeleteRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
                entity.UpdatedDate = DateTime.Now;
            }
            UpdateRange(entities);
        }

        public async Task<bool> SoftDeleteByIdAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                SoftDelete(entity);
                return true;
            }
            return false;
        }

        #endregion

        #region Condition-based Operations

        public IQueryable<T> Where(Expression<Func<T, bool>> condition, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return query.Where(x => !x.IsDeleted).Where(condition);
        }

        public IQueryable<T> Where(Expression<Func<T, bool>> condition, bool isTracking = true, params string[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (!isTracking)
                query = query.AsNoTracking();

            return query.Where(x => !x.IsDeleted).Where(condition);
        }

        public async Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> condition, bool isTracking = true)
        {
            return await Where(condition, isTracking).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> condition, bool isTracking = true, params string[] includes)
        {
            return await Where(condition, isTracking, includes).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> condition, int limit, bool isTracking = true)
        {
            return await Where(condition, isTracking).Take(limit).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> condition, int limit, bool isTracking = true, params string[] includes)
        {
            return await Where(condition, isTracking, includes).Take(limit).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetByMultipleConditionsAsync(List<Expression<Func<T, bool>>> conditions, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            query = query.Where(x => !x.IsDeleted);

            foreach (var condition in conditions)
            {
                query = query.Where(condition);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetByMultipleConditionsAsync(List<Expression<Func<T, bool>>> conditions, bool isTracking = true, params string[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (!isTracking)
                query = query.AsNoTracking();

            query = query.Where(x => !x.IsDeleted);

            foreach (var condition in conditions)
            {
                query = query.Where(condition);
            }

            return await query.ToListAsync();
        }

        #endregion

        #region Search Operations

        public async Task<IEnumerable<T>> SearchAsync(Expression<Func<T, string>> selector, string searchTerm, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted)
                            .Where(x => EF.Functions.Like(selector.Compile()(x), $"%{searchTerm}%"))
                            .ToListAsync();
        }

        public async Task<IEnumerable<T>> SearchAsync(Expression<Func<T, string>> selector, string searchTerm, int limit, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted)
                            .Where(x => EF.Functions.Like(selector.Compile()(x), $"%{searchTerm}%"))
                            .Take(limit)
                            .ToListAsync();
        }

        public async Task<IEnumerable<T>> SearchInMultipleFieldsAsync(List<Expression<Func<T, string>>> selectors, string searchTerm, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            query = query.Where(x => !x.IsDeleted);

            Expression<Func<T, bool>> combinedCondition = null;

            foreach (var selector in selectors)
            {
                var condition = CreateLikeExpression(selector, searchTerm);
                combinedCondition = combinedCondition == null ? condition : CombineOr(combinedCondition, condition);
            }

            if (combinedCondition != null)
                query = query.Where(combinedCondition);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetByContainsAsync(Expression<Func<T, string>> selector, string value, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted)
                            .Where(x => selector.Compile()(x).Contains(value))
                            .ToListAsync();
        }

        public async Task<IEnumerable<T>> GetByStartsWithAsync(Expression<Func<T, string>> selector, string value, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted)
                            .Where(x => selector.Compile()(x).StartsWith(value))
                            .ToListAsync();
        }

        public async Task<IEnumerable<T>> GetByEndsWithAsync(Expression<Func<T, string>> selector, string value, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted)
                            .Where(x => selector.Compile()(x).EndsWith(value))
                            .ToListAsync();
        }

        public async Task<IEnumerable<T>> GetByRangeAsync<TProperty>(Expression<Func<T, TProperty>> selector, TProperty min, TProperty max, bool isTracking = true) where TProperty : IComparable<TProperty>
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted)
                            .Where(x => selector.Compile()(x).CompareTo(min) >= 0 && selector.Compile()(x).CompareTo(max) <= 0)
                            .ToListAsync();
        }

        public async Task<IEnumerable<T>> GetByDateRangeAsync(Expression<Func<T, DateTime>> selector, DateTime startDate, DateTime endDate, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted)
                            .Where(x => selector.Compile()(x) >= startDate && selector.Compile()(x) <= endDate)
                            .ToListAsync();
        }

        public async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<int> ids, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted && ids.Contains(x.Id)).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<int> ids, bool isTracking = true, params string[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted && ids.Contains(x.Id)).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetByValueInListAsync<TProperty>(Expression<Func<T, TProperty>> selector, IEnumerable<TProperty> values, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted)
                            .Where(x => values.Contains(selector.Compile()(x)))
                            .ToListAsync();
        }

        #endregion

        #region Sorting Operations

        public IQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return query.Where(x => !x.IsDeleted).OrderBy(keySelector);
        }

        public IQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return query.Where(x => !x.IsDeleted).OrderByDescending(keySelector);
        }

        public async Task<IEnumerable<T>> GetAllOrderedAsync<TKey>(Expression<Func<T, TKey>> keySelector, bool ascending = true, bool isTracking = true)
        {
            if (ascending)
                return await OrderBy(keySelector, isTracking).ToListAsync();
            else
                return await OrderByDescending(keySelector, isTracking).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllOrderedAsync<TKey>(Expression<Func<T, TKey>> keySelector, bool ascending = true, bool isTracking = true, params string[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (!isTracking)
                query = query.AsNoTracking();

            query = query.Where(x => !x.IsDeleted);

            if (ascending)
                query = query.OrderBy(keySelector);
            else
                query = query.OrderByDescending(keySelector);

            return await query.ToListAsync();
        }

        public IQueryable<T> OrderByMultiple(List<(Expression<Func<T, object>> keySelector, bool ascending)> sorts, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            query = query.Where(x => !x.IsDeleted);

            if (sorts != null && sorts.Any())
            {
                var firstSort = sorts.First();
                IOrderedQueryable<T> orderedQuery;

                if (firstSort.ascending)
                    orderedQuery = query.OrderBy(firstSort.keySelector);
                else
                    orderedQuery = query.OrderByDescending(firstSort.keySelector);

                foreach (var sort in sorts.Skip(1))
                {
                    if (sort.ascending)
                        orderedQuery = orderedQuery.ThenBy(sort.keySelector);
                    else
                        orderedQuery = orderedQuery.ThenByDescending(sort.keySelector);
                }

                return orderedQuery;
            }

            return query;
        }

        #endregion

        #region Pagination Operations

        public async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
        }

        public async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, bool isTracking = true, params string[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (!isTracking)
                query = query.AsNoTracking();

            return await query.Where(x => !x.IsDeleted)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
        }

        public async Task<IEnumerable<T>> GetPagedAsync(Expression<Func<T, bool>> condition, int page, int pageSize, bool isTracking = true)
        {
            return await Where(condition, isTracking)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
        }

        public async Task<IEnumerable<T>> GetPagedAsync(Expression<Func<T, bool>> condition, int page, int pageSize, bool isTracking = true, params string[] includes)
        {
            return await Where(condition, isTracking, includes)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
        }

        public async Task<IEnumerable<T>> GetPagedOrderedAsync<TKey>(Expression<Func<T, TKey>> keySelector, int page, int pageSize, bool ascending = true, bool isTracking = true)
        {
            if (ascending)
                return await OrderBy(keySelector, isTracking)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
            else
                return await OrderByDescending(keySelector, isTracking)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
        }

        public async Task<IEnumerable<T>> GetPagedOrderedAsync<TKey>(Expression<Func<T, bool>> condition, Expression<Func<T, TKey>> keySelector, int page, int pageSize, bool ascending = true, bool isTracking = true)
        {
            IQueryable<T> query = Where(condition, isTracking);

            if (ascending)
                query = query.OrderBy(keySelector);
            else
                query = query.OrderByDescending(keySelector);

            return await query.Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
        }

        public async Task<(IEnumerable<T> Data, int TotalCount)> GetPagedWithCountAsync(int page, int pageSize, bool isTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
                query = query.AsNoTracking();

            query = query.Where(x => !x.IsDeleted);

            var totalCount = await query.CountAsync();
            var data = await query.Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<(IEnumerable<T> Data, int TotalCount)> GetPagedWithCountAsync(Expression<Func<T, bool>> condition, int page, int pageSize, bool isTracking = true)
        {
            var query = Where(condition, isTracking);

            var totalCount = await query.CountAsync();
            var data = await query.Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<(IEnumerable<T> Data, int TotalCount)> GetPagedWithCountAsync<TKey>(Expression<Func<T, bool>> condition, Expression<Func<T, TKey>> keySelector, int page, int pageSize, bool ascending = true, bool isTracking = true)
        {
            IQueryable<T> query = Where(condition, isTracking);

            if (ascending)
                query = query.OrderBy(keySelector);
            else
                query = query.OrderByDescending(keySelector);

            var totalCount = await query.CountAsync();
            var data = await query.Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            return (data, totalCount);
        }

        #endregion

        #region Aggregation Operations

        public async Task<int> CountAsync()
        {
            return await _dbSet.Where(x => !x.IsDeleted).CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> condition)
        {
            return await _dbSet.Where(x => !x.IsDeleted).Where(condition).CountAsync();
        }

        public async Task<long> LongCountAsync()
        {
            return await _dbSet.Where(x => !x.IsDeleted).LongCountAsync();
        }

        public async Task<long> LongCountAsync(Expression<Func<T, bool>> condition)
        {
            return await _dbSet.Where(x => !x.IsDeleted).Where(condition).LongCountAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> condition)
        {
            return await _dbSet.Where(x => !x.IsDeleted).AnyAsync(condition);
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            return await _dbSet.Where(x => !x.IsDeleted).AnyAsync(x => x.Id == id);
        }

        public async Task<bool> AnyAsync()
        {
            return await _dbSet.Where(x => !x.IsDeleted).AnyAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> condition)
        {
            return await _dbSet.Where(x => !x.IsDeleted).AnyAsync(condition);
        }

        public async Task<bool> AllAsync(Expression<Func<T, bool>> condition)
        {
            return await _dbSet.Where(x => !x.IsDeleted).AllAsync(condition);
        }

        public async Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            return await _dbSet.Where(x => !x.IsDeleted).MinAsync(selector);
        }

        public async Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            return await _dbSet.Where(x => !x.IsDeleted).MaxAsync(selector);
        }

        public async Task<TResult?> MinAsync<TResult>(Expression<Func<T, bool>> condition, Expression<Func<T, TResult>> selector)
        {
            var query = _dbSet.Where(x => !x.IsDeleted).Where(condition);
            if (await query.AnyAsync())
                return await query.MinAsync(selector);
            return default(TResult);
        }

        public async Task<TResult?> MaxAsync<TResult>(Expression<Func<T, bool>> condition, Expression<Func<T, TResult>> selector)
        {
            var query = _dbSet.Where(x => !x.IsDeleted).Where(condition);
            if (await query.AnyAsync())
                return await query.MaxAsync(selector);
            return default(TResult);
        }

        public async Task<decimal> SumAsync(Expression<Func<T, decimal>> selector)
        {
            return await _dbSet.Where(x => !x.IsDeleted).SumAsync(selector);
        }

        public async Task<decimal?> SumAsync(Expression<Func<T, decimal?>> selector)
        {
            return await _dbSet.Where(x => !x.IsDeleted).SumAsync(selector);
        }

        public async Task<double> AverageAsync(Expression<Func<T, decimal>> selector)
        {
            return (double)await _dbSet.Where(x => !x.IsDeleted).AverageAsync(selector);
        }

        public async Task<double?> AverageAsync(Expression<Func<T, decimal?>> selector)
        {
            return (double?)await _dbSet.Where(x => !x.IsDeleted).AverageAsync(selector);
        }

        public async Task<double> AverageAsync(Expression<Func<T, bool>> condition, Expression<Func<T, decimal>> selector)
        {
            return (double)await _dbSet.Where(x => !x.IsDeleted).Where(condition).AverageAsync(selector);
        }
        public async Task<decimal> SumAsync(Expression<Func<T, bool>> condition, Expression<Func<T, decimal>> selector)
        {
            return await _dbSet.Where(x => !x.IsDeleted).Where(condition).SumAsync(selector);
        }

     

        #endregion

        #region Bulk Operations

        public async Task BulkInsertAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entity.CreatedDate = DateTime.Now;
            }
            await _dbSet.AddRangeAsync(entities);
        }

        public async Task BulkInsertAsync(IEnumerable<T> entities, int batchSize)
        {
            var entityList = entities.ToList();
            for (int i = 0; i < entityList.Count; i += batchSize)
            {
                var batch = entityList.Skip(i).Take(batchSize);
                await BulkInsertAsync(batch);
                await _context.SaveChangesAsync();
            }
        }

        public async Task BulkUpdateAsync(Expression<Func<T, bool>> condition, Expression<Func<T, T>> updateExpression)
        {
            var entities = await _dbSet.Where(x => !x.IsDeleted).Where(condition).ToListAsync();
            foreach (var entity in entities)
            {
                entity.UpdatedDate = DateTime.Now;
                var updatedEntity = updateExpression.Compile()(entity);
                _context.Entry(entity).CurrentValues.SetValues(updatedEntity);
            }
        }

        public async Task BulkUpdateAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entity.UpdatedDate = DateTime.Now;
            }
            _dbSet.UpdateRange(entities);
            await Task.CompletedTask;
        }

        public async Task BulkDeleteAsync(Expression<Func<T, bool>> condition)
        {
            var entities = await _dbSet.Where(x => !x.IsDeleted).Where(condition).ToListAsync();
            _dbSet.RemoveRange(entities);
        }

        public async Task BulkDeleteAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }

        #endregion

        #region Transaction Operations

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region Special Operations

        public IQueryable<T> Distinct()
        {
            return _dbSet.Where(x => !x.IsDeleted).Distinct();
        }

        public async Task<IEnumerable<TResult>> GetDistinctAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            return await _dbSet.Where(x => !x.IsDeleted).Select(selector).Distinct().ToListAsync();
        }

        public async Task<IEnumerable<TResult>> GetDistinctAsync<TResult>(Expression<Func<T, bool>> condition, Expression<Func<T, TResult>> selector)
        {
            return await _dbSet.Where(x => !x.IsDeleted).Where(condition).Select(selector).Distinct().ToListAsync();
        }

        public async Task<IEnumerable<IGrouping<TKey, T>>> GroupByAsync<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return await _dbSet.Where(x => !x.IsDeleted).GroupBy(keySelector).ToListAsync();
        }

        public async Task<IEnumerable<TResult>> GroupByAsync<TKey, TResult>(Expression<Func<T, TKey>> keySelector, Expression<Func<IGrouping<TKey, T>, TResult>> resultSelector)
        {
            return await _dbSet.Where(x => !x.IsDeleted).GroupBy(keySelector).Select(resultSelector).ToListAsync();
        }

        public async Task<T?> GetRandomAsync()
        {
            var count = await CountAsync();
            if (count == 0) return null;

            var random = new Random();
            var skip = random.Next(0, count);

            return await _dbSet.Where(x => !x.IsDeleted).Skip(skip).FirstOrDefaultAsync();
        }

        public async Task<T?> GetRandomAsync(Expression<Func<T, bool>> condition)
        {
            var count = await CountAsync(condition);
            if (count == 0) return null;

            var random = new Random();
            var skip = random.Next(0, count);

            return await _dbSet.Where(x => !x.IsDeleted).Where(condition).Skip(skip).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetRandomAsync(int count)
        {
            var totalCount = await CountAsync();
            if (totalCount == 0) return new List<T>();

            var random = new Random();
            var skip = random.Next(0, Math.Max(1, totalCount - count));

            return await _dbSet.Where(x => !x.IsDeleted).Skip(skip).Take(count).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetRandomAsync(Expression<Func<T, bool>> condition, int count)
        {
            var totalCount = await CountAsync(condition);
            if (totalCount == 0) return new List<T>();

            var random = new Random();
            var skip = random.Next(0, Math.Max(1, totalCount - count));

            return await _dbSet.Where(x => !x.IsDeleted).Where(condition).Skip(skip).Take(count).ToListAsync();
        }

        public async Task<T?> GetLatestAsync()
        {
            return await _dbSet.Where(x => !x.IsDeleted).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
        }

        public async Task<T?> GetLatestAsync(Expression<Func<T, bool>> condition)
        {
            return await _dbSet.Where(x => !x.IsDeleted).Where(condition).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
        }

        public async Task<T?> GetOldestAsync()
        {
            return await _dbSet.Where(x => !x.IsDeleted).OrderBy(x => x.CreatedDate).FirstOrDefaultAsync();
        }

        public async Task<T?> GetOldestAsync(Expression<Func<T, bool>> condition)
        {
            return await _dbSet.Where(x => !x.IsDeleted).Where(condition).OrderBy(x => x.CreatedDate).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetLatestAsync(int count)
        {
            return await _dbSet.Where(x => !x.IsDeleted).OrderByDescending(x => x.CreatedDate).Take(count).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetOldestAsync(int count)
        {
            return await _dbSet.Where(x => !x.IsDeleted).OrderBy(x => x.CreatedDate).Take(count).ToListAsync();
        }

        #endregion

        #region Raw SQL Operations

        public async Task<IEnumerable<T>> FromSqlRawAsync(string sql, params object[] parameters)
        {
            return await _dbSet.FromSqlRaw(sql, parameters).Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<TResult>> ExecuteQueryAsync<TResult>(string sql, params object[] parameters)
        {
            return await _context.Database.SqlQueryRaw<TResult>(sql, parameters).ToListAsync();
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
        {
            return await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        #endregion

        #region Helper Methods

        private Expression<Func<T, bool>> CreateLikeExpression(Expression<Func<T, string>> selector, string searchTerm)
        {
            var parameter = selector.Parameters[0];
            var member = selector.Body;
            var constant = Expression.Constant($"%{searchTerm}%");
            var likeMethod = typeof(DbFunctionsExtensions).GetMethod("Like", new[] { typeof(DbFunctions), typeof(string), typeof(string) });
            var dbFunctions = Expression.Property(null, typeof(EF), nameof(EF.Functions));
            var call = Expression.Call(likeMethod, dbFunctions, member, constant);
            return Expression.Lambda<Func<T, bool>>(call, parameter);
        }

        private Expression<Func<T, bool>> CombineOr(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var parameter = left.Parameters[0];
            var leftBody = left.Body;
            var rightBody = Expression.Invoke(right, parameter);
            var orElse = Expression.OrElse(leftBody, rightBody);
            return Expression.Lambda<Func<T, bool>>(orElse, parameter);
        }

        #endregion
    }
}
