using LinqToSQL.Querying;
using System.Data.Common;
using System.Linq.Expressions;

namespace LinqToSQL;

public class DbSet<TEntity> : Querying.IQueryable<TEntity>
{
    private readonly DbConnection _connection;

    public DbSet(DbConnection connection) => _connection = connection;

    /// <summary>
    /// Gets a new instance of <see cref="IQueryable{TEntity}"/>.
    /// </summary>
    private Querying.IQueryable<TEntity> Query => new Queryable<TEntity>(_connection);

    public Querying.IQueryable<TEntity> GroupBy<TGroup>(Expression<Func<TEntity, TGroup>> expression) => Query.GroupBy(expression);
    public Querying.IQueryable<TEntity> OrderBy<TOrder>(Expression<Func<TEntity, TOrder>> expression, OrderDirection direction = OrderDirection.Ascending) => Query.OrderBy(expression, direction);
    public IAsyncEnumerable<TEntity> QueryAsync(DbTransaction? transaction = null, CancellationToken cancellationToken = default) => Query.QueryAsync(transaction, cancellationToken);
    public Querying.IQueryable<TResult> Select<TResult>(Expression<Func<TEntity, TResult>> expression) => Query.Select(expression);
    public Querying.IQueryable<TEntity> Where(Expression<Predicate<TEntity>> expression) => Query.Where(expression);
}
