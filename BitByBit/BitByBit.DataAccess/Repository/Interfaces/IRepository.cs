using BitByBit.Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.DataAccess.Repository.Interfaces
{
    public interface IRepository<T> where T : BaseEntity, new()
    {
        DbSet<T> Table { get; }

        #region Basic CRUD Operations

        // Create
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

        // Read - Single
        Task<T?> GetByIdAsync(int id, bool isTracking = true);
        Task<T?> GetByIdAsync(int id, bool isTracking = true, params string[] includes);
        Task<T?> GetSingleAsync(Expression<Func<T, bool>> condition, bool isTracking = true);
        Task<T?> GetSingleAsync(Expression<Func<T, bool>> condition, bool isTracking = true, params string[] includes);
        Task<T?> GetFirstAsync(Expression<Func<T, bool>> condition, bool isTracking = true);
        Task<T?> GetFirstAsync(Expression<Func<T, bool>> condition, bool isTracking = true, params string[] includes);
        Task<T?> GetLastAsync(Expression<Func<T, bool>> condition, bool isTracking = true);
        Task<T?> GetLastAsync(Expression<Func<T, bool>> condition, bool isTracking = true, params string[] includes);

        // Read - Multiple
        Task<IEnumerable<T>> GetAllAsync(bool isTracking = true);
        Task<IEnumerable<T>> GetAllAsync(bool isTracking = true, params string[] includes);
        Task<IEnumerable<T>> GetAllAsync(int limit, bool isTracking = true);
        Task<IEnumerable<T>> GetAllAsync(int limit, bool isTracking = true, params string[] includes);
        IQueryable<T> GetAll(bool isTracking = true);
        IQueryable<T> GetAll(bool isTracking = true, params string[] includes);

        // Update
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task UpdateRangeAsync(IEnumerable<T> entities);

        // Delete
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
        Task<bool> DeleteByIdAsync(int id);
        Task<int> DeleteByConditionAsync(Expression<Func<T, bool>> condition);
        void SoftDelete(T entity);
        void SoftDeleteRange(IEnumerable<T> entities);
        Task<bool> SoftDeleteByIdAsync(int id);

        #endregion

        #region Condition-based Operations

        // Where conditions
        IQueryable<T> Where(Expression<Func<T, bool>> condition, bool isTracking = true);
        IQueryable<T> Where(Expression<Func<T, bool>> condition, bool isTracking = true, params string[] includes);
        Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> condition, bool isTracking = true);
        Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> condition, bool isTracking = true, params string[] includes);
        Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> condition, int limit, bool isTracking = true);
        Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> condition, int limit, bool isTracking = true, params string[] includes);

        // Multiple conditions
        Task<IEnumerable<T>> GetByMultipleConditionsAsync(List<Expression<Func<T, bool>>> conditions, bool isTracking = true);
        Task<IEnumerable<T>> GetByMultipleConditionsAsync(List<Expression<Func<T, bool>>> conditions, bool isTracking = true, params string[] includes);

        #endregion

        #region Search Operations

        // Text search
        Task<IEnumerable<T>> SearchAsync(Expression<Func<T, string>> selector, string searchTerm, bool isTracking = true);
        Task<IEnumerable<T>> SearchAsync(Expression<Func<T, string>> selector, string searchTerm, int limit, bool isTracking = true);
        Task<IEnumerable<T>> SearchInMultipleFieldsAsync(List<Expression<Func<T, string>>> selectors, string searchTerm, bool isTracking = true);

        // Contains search
        Task<IEnumerable<T>> GetByContainsAsync(Expression<Func<T, string>> selector, string value, bool isTracking = true);
        Task<IEnumerable<T>> GetByStartsWithAsync(Expression<Func<T, string>> selector, string value, bool isTracking = true);
        Task<IEnumerable<T>> GetByEndsWithAsync(Expression<Func<T, string>> selector, string value, bool isTracking = true);

        // Range search
        Task<IEnumerable<T>> GetByRangeAsync<TProperty>(Expression<Func<T, TProperty>> selector, TProperty min, TProperty max, bool isTracking = true) where TProperty : IComparable<TProperty>;
        Task<IEnumerable<T>> GetByDateRangeAsync(Expression<Func<T, DateTime>> selector, DateTime startDate, DateTime endDate, bool isTracking = true);

        // List/Array search
        Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<int> ids, bool isTracking = true);
        Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<int> ids, bool isTracking = true, params string[] includes);
        Task<IEnumerable<T>> GetByValueInListAsync<TProperty>(Expression<Func<T, TProperty>> selector, IEnumerable<TProperty> values, bool isTracking = true);

        #endregion

        #region Sorting Operations

        // Single sort
        IQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector, bool isTracking = true);
        IQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector, bool isTracking = true);
        Task<IEnumerable<T>> GetAllOrderedAsync<TKey>(Expression<Func<T, TKey>> keySelector, bool ascending = true, bool isTracking = true);
        Task<IEnumerable<T>> GetAllOrderedAsync<TKey>(Expression<Func<T, TKey>> keySelector, bool ascending = true, bool isTracking = true, params string[] includes);

        // Multiple sort
        IQueryable<T> OrderByMultiple(List<(Expression<Func<T, object>> keySelector, bool ascending)> sorts, bool isTracking = true);

        #endregion

        #region Pagination Operations

        // Basic pagination
        Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, bool isTracking = true);
        Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, bool isTracking = true, params string[] includes);
        Task<IEnumerable<T>> GetPagedAsync(Expression<Func<T, bool>> condition, int page, int pageSize, bool isTracking = true);
        Task<IEnumerable<T>> GetPagedAsync(Expression<Func<T, bool>> condition, int page, int pageSize, bool isTracking = true, params string[] includes);

        // Pagination with sorting
        Task<IEnumerable<T>> GetPagedOrderedAsync<TKey>(Expression<Func<T, TKey>> keySelector, int page, int pageSize, bool ascending = true, bool isTracking = true);
        Task<IEnumerable<T>> GetPagedOrderedAsync<TKey>(Expression<Func<T, bool>> condition, Expression<Func<T, TKey>> keySelector, int page, int pageSize, bool ascending = true, bool isTracking = true);

        // Pagination result with total count
        Task<(IEnumerable<T> Data, int TotalCount)> GetPagedWithCountAsync(int page, int pageSize, bool isTracking = true);
        Task<(IEnumerable<T> Data, int TotalCount)> GetPagedWithCountAsync(Expression<Func<T, bool>> condition, int page, int pageSize, bool isTracking = true);
        Task<(IEnumerable<T> Data, int TotalCount)> GetPagedWithCountAsync<TKey>(Expression<Func<T, bool>> condition, Expression<Func<T, TKey>> keySelector, int page, int pageSize, bool ascending = true, bool isTracking = true);

        #endregion

        #region Aggregation Operations

        // Count
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> condition);
        Task<long> LongCountAsync();
        Task<long> LongCountAsync(Expression<Func<T, bool>> condition);

        // Exists
        Task<bool> ExistsAsync(Expression<Func<T, bool>> condition);
        Task<bool> ExistsByIdAsync(int id);

        // Any/All
        Task<bool> AnyAsync();
        Task<bool> AnyAsync(Expression<Func<T, bool>> condition);
        Task<bool> AllAsync(Expression<Func<T, bool>> condition);

        // Min/Max
        Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector);
        Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector);
        Task<TResult?> MinAsync<TResult>(Expression<Func<T, bool>> condition, Expression<Func<T, TResult>> selector);
        Task<TResult?> MaxAsync<TResult>(Expression<Func<T, bool>> condition, Expression<Func<T, TResult>> selector);

        // Sum/Average (for numeric types)
        Task<decimal> SumAsync(Expression<Func<T, decimal>> selector);
        Task<decimal?> SumAsync(Expression<Func<T, decimal?>> selector);
        Task<double> AverageAsync(Expression<Func<T, decimal>> selector);
        Task<double?> AverageAsync(Expression<Func<T, decimal?>> selector);
        Task<decimal> SumAsync(Expression<Func<T, bool>> condition, Expression<Func<T, decimal>> selector);
        Task<double> AverageAsync(Expression<Func<T, bool>> condition, Expression<Func<T, decimal>> selector);

        #endregion

        #region Bulk Operations

        // Bulk insert
        Task BulkInsertAsync(IEnumerable<T> entities);
        Task BulkInsertAsync(IEnumerable<T> entities, int batchSize);

        // Bulk update
        Task BulkUpdateAsync(Expression<Func<T, bool>> condition, Expression<Func<T, T>> updateExpression);
        Task BulkUpdateAsync(IEnumerable<T> entities);

        // Bulk delete
        Task BulkDeleteAsync(Expression<Func<T, bool>> condition);
        Task BulkDeleteAsync(IEnumerable<T> entities);

        #endregion

        #region Transaction Operations

        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        #endregion

        #region Special Operations

        // Distinct
        IQueryable<T> Distinct();
        Task<IEnumerable<TResult>> GetDistinctAsync<TResult>(Expression<Func<T, TResult>> selector);
        Task<IEnumerable<TResult>> GetDistinctAsync<TResult>(Expression<Func<T, bool>> condition, Expression<Func<T, TResult>> selector);

        // Group By
        Task<IEnumerable<IGrouping<TKey, T>>> GroupByAsync<TKey>(Expression<Func<T, TKey>> keySelector);
        Task<IEnumerable<TResult>> GroupByAsync<TKey, TResult>(Expression<Func<T, TKey>> keySelector, Expression<Func<IGrouping<TKey, T>, TResult>> resultSelector);

        // Random
        Task<T?> GetRandomAsync();
        Task<T?> GetRandomAsync(Expression<Func<T, bool>> condition);
        Task<IEnumerable<T>> GetRandomAsync(int count);
        Task<IEnumerable<T>> GetRandomAsync(Expression<Func<T, bool>> condition, int count);

        // Latest/Oldest (by CreatedDate)
        Task<T?> GetLatestAsync();
        Task<T?> GetLatestAsync(Expression<Func<T, bool>> condition);
        Task<T?> GetOldestAsync();
        Task<T?> GetOldestAsync(Expression<Func<T, bool>> condition);
        Task<IEnumerable<T>> GetLatestAsync(int count);
        Task<IEnumerable<T>> GetOldestAsync(int count);

        #endregion

        #region Raw SQL Operations

        // Raw SQL
        Task<IEnumerable<T>> FromSqlRawAsync(string sql, params object[] parameters);
        Task<IEnumerable<TResult>> ExecuteQueryAsync<TResult>(string sql, params object[] parameters);
        Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters);

        #endregion
    }
}
