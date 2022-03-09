using System.Data.Common;
using System.Linq.Expressions;

namespace LinqToSQL.Querying;

/// <summary>
/// Provides extensions for <see cref="IQueryable{TEntity}"/> instances.
/// </summary>
public static class QueryableExtensions
{
    /// <inheritdoc cref="IQueryable{TEntity}.OrderBy(Expression{Predicate{TEntity}}, OrderDirection)"/>
    public static IQueryable<TEntity> OrderByDescending<TEntity, TOrder>(
        this IQueryable<TEntity> @this,
        Expression<Func<TEntity, TOrder>> expression)
    {
        return @this.OrderBy(expression, OrderDirection.Descending);
    }

    /// <inheritdoc cref="IQueryable{TEntity}.QueryAsync(DbConnection, DbTransaction?, CancellationToken)"/>
    public static IAsyncEnumerable<TEntity> QueryAsync<TEntity>(
        this IQueryable<TEntity> queryable,
        CancellationToken cancellationToken = default)
    {
        return queryable.QueryAsync(null, cancellationToken);
    }
}