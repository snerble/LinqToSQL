using System.Data.Common;
using System.Linq.Expressions;

namespace LinqToSQL.Querying;

/// <summary>
/// Provides functionality to build SQL queries.
/// </summary>
public interface IQueryBuilder
{
    /// <summary>
    /// Gets a dictionary of properties used to share data while building a query.
    /// </summary>
    IDictionary<object, object> Properties { get; }

    /// <summary>
    /// Returns a <see cref="DbCommand"/> constructed from the current
    /// SQL string.
    /// </summary>
    DbCommand Build();

    /// <summary>
    /// Inserts a specified string <paramref name="literal"/>.
    /// </summary>
    /// <param name="literal">The text to insert.</param>
    /// <returns>This <see cref="IQueryBuilder"/> instance for chaining calls.</returns>
    IQueryBuilder Literal(string literal);
    /// <summary>
    /// Inserts a common SQL statement.
    /// </summary>
    /// <param name="statement">The type of SQL statement to insert.</param>
    /// <param name="operands">Optional operands for the statement.</param>
    /// <returns>This <see cref="IQueryBuilder"/> instance for chaining calls.</returns>
    IQueryBuilder Statement(SqlStatement statement, params object[] operands);
    /// <summary>
    /// Inserts a <see cref="System.Linq.Expressions.Expression"/>.
    /// </summary>
    /// <param name="expression">The expression to add directly to the query.</param>
    /// <returns>This <see cref="IQueryBuilder"/> instance for chaining calls.</returns>
    IQueryBuilder Expression(Expression expression);
    /// <summary>
    /// Inserts a reference to a specified table or column.
    /// </summary>
    /// <param name="tableName">The name of the table to reference.</param>
    /// <param name="columnReference">The name of the column to reference.</param>
    /// <returns>This <see cref="IQueryBuilder"/> instance for chaining calls.</returns>
    IQueryBuilder Reference(string tableName, string? columnReference = null);
    /// <summary>
    /// Inserts an SQL parameter with the specified value.
    /// </summary>
    /// <param name="value">The value of the parameter.</param>
    /// <param name="paramName">An name for the parameter.</param>
    /// <returns>This <see cref="IQueryBuilder"/> instance for chaining calls.</returns>
    IQueryBuilder Parameter(object? value, string? paramName = null);
}

/// <summary>
/// Represents a common SQL statement.
/// </summary>
public enum SqlStatement
{
    /// <summary>
    /// SELECT
    /// </summary>
    Select,
    /// <summary>
    /// Wildcard (*)
    /// </summary>
    Wildcard,
    /// <summary>
    /// FROM
    /// </summary>
    From,
    /// <summary>
    /// WHERE
    /// </summary>
    Where,
    /// <summary>
    /// Alias (AS)
    /// </summary>
    As,
    /// <summary>
    /// ORDER BY
    /// </summary>
    OrderBy,
    /// <summary>
    /// Ascending order (ASC)
    /// </summary>
    Ascending,
    /// <summary>
    /// Descending order (DESC)
    /// </summary>
    Descending,
    /// <summary>
    /// GROUP BY
    /// </summary>
    GroupBy
}
