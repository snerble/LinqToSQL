using System.Data.Common;
using System.Linq.Expressions;

namespace LinqToSQL.Querying;

/// <summary>
/// Provides mechanisms for querying elements using the <see cref="Expression"/>
/// syntax.
/// </summary>
/// <typeparam name="TEntity">The type of the entities to query.</typeparam>
public interface IQueryable<TEntity>
{
    /// <summary>
    /// Executes the query and asynchronously enumerates it's results.
    /// </summary>
    /// <param name="transaction">The transaction to perform the query in.</param>
    IAsyncEnumerable<TEntity> QueryAsync(DbTransaction? transaction = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Selects specified values from the current query.
    /// </summary>
    /// <typeparam name="TResult">The type of the resulting elements.</typeparam>
    /// <param name="expression">An expression that selects values from <typeparamref name="TEntity"/>.</param>
    /// <returns>
    /// An instance of <see cref="IQueryable{TEntity}"/> that represents the resulting query.
    /// </returns>
    IQueryable<TResult> Select<TResult>(Expression<Func<TEntity, TResult>> expression);

    /// <summary>
    /// Filters the current query using the specified predicate.
    /// </summary>
    /// <param name="expression">An expression that defines a condition to filter the query with.</param>
    /// <returns>
    /// An instance of <see cref="IQueryable{TEntity}"/> that represents the resulting query.
    /// </returns>
    IQueryable<TEntity> Where(Expression<Predicate<TEntity>> expression);

    /// <summary>
    /// Orders the elements of the current query using the specified value.
    /// </summary>
    /// <param name="expression">An expression that selects a value to order by.</param>
    /// <param name="direction">The direction to order the elements by.</param>
    /// <returns>
    /// An instance of <see cref="IQueryable{TEntity}"/> that represents the resulting query.
    /// </returns>
    IQueryable<TEntity> OrderBy<TOrder>(Expression<Func<TEntity, TOrder>> expression, OrderDirection direction = default);

    /// <summary>
    /// Groups the elements of the query by a specified value.
    /// </summary>
    /// <typeparam name="TGroup">The type of the value to group on.</typeparam>
    /// <param name="expression">An expression that selects a value on which the query should be grouped.</param>
    /// <returns>
    /// An instance of <see cref="IQueryable{TEntity}"/> that represents the resulting query.
    /// </returns>
    IQueryable<TEntity> GroupBy<TGroup>(Expression<Func<TEntity, TGroup>> expression);
}

/// <summary>
/// Specifies the direction to sort elements.
/// </summary>
public enum OrderDirection
{
    /// <summary>
    /// Smallest values first.
    /// </summary>
    Ascending,
    /// <summary>
    /// Largest values first.
    /// </summary>
    Descending,
}
