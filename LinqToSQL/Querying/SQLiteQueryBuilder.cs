using Microsoft.Data.Sqlite;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace LinqToSQL.Querying;

/// <summary>
/// SQLite implementation of <see cref="IQueryBuilder"/>.
/// </summary>
internal sealed class SQLiteQueryBuilder : IQueryBuilder
{
    private readonly StringBuilder _sb = new();
    private readonly List<SqliteParameter> _params = new();

    public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

    public DbCommand Build()
    {
        var command = new SqliteCommand(_sb.ToString());
        command.Parameters.AddRange(_params);
        return command;
    }

    public IQueryBuilder Expression(Expression expression)
    {
        switch (expression)
        {
            case ConstantExpression { NodeType: ExpressionType.Constant } ce:
                return Parameter(ce.Value);

            case MemberExpression { NodeType: ExpressionType.MemberAccess } me:
                return ((IQueryBuilder)this).Reference(me.Member);
        }

        _sb.Append(expression.NodeType switch
        {
            ExpressionType.Add or
            ExpressionType.AddChecked => "+",
            ExpressionType.And => "&",
            ExpressionType.AndAlso => "AND",
            ExpressionType.Divide => "/",
            ExpressionType.Equal => "=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LeftShift => "<<",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.Modulo => "%",
            ExpressionType.Multiply or
            ExpressionType.MultiplyChecked => "*",
            ExpressionType.Negate or
            ExpressionType.NegateChecked => "* -1",
            ExpressionType.Not => "NOT",
            ExpressionType.NotEqual => "!=",
            ExpressionType.Or => "|",
            ExpressionType.OrElse => "OR",
            ExpressionType.RightShift => ">>",
            ExpressionType.Subtract or
            ExpressionType.SubtractChecked => "-",
            ExpressionType.OnesComplement => "~",
            ExpressionType.IsTrue => "IS 1",
            ExpressionType.IsFalse => "IS NOT 1",
            _ => throw new NotSupportedException(),
        });

        return this;
    }

    public IQueryBuilder Literal(string literal)
    {
        _sb.Append(literal);
        return this;
    }

    public IQueryBuilder Parameter(object? value, string? paramName = null)
    {
        paramName ??= _params.Count.ToString();
        var param = new SqliteParameter(paramName, value);
        _params.Add(param);

        _sb.AppendFormat("@{0}", paramName);
        Space();

        return this;
    }

    public IQueryBuilder Reference(string tableName, string? columnName = null)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException($"'{nameof(tableName)}' cannot be null or whitespace.", nameof(tableName));

        if (columnName != null && string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException($"'{nameof(columnName)}' cannot be empty or whitespace.", nameof(columnName));

        _sb.AppendFormat("`{0}`", tableName);

        if (columnName != null)
            _sb.AppendFormat(".`{0}`", columnName);

        return this;
    }

    public IQueryBuilder Statement(SqlStatement statement, params object[] operands)
    {
        if (statement == SqlStatement.As)
        {
            _sb.Append("AS");
            Space();
            _sb.AppendFormat("\"{0}\"", operands.Single());
            return this;
        }

        _sb.Append(statement switch
        {
            SqlStatement.Select => "SELECT",
            SqlStatement.From => "FROM",
            SqlStatement.Where => "WHERE",
            SqlStatement.As => throw new Exception(), // Should not happen
            SqlStatement.OrderBy => "ORDER BY",
            SqlStatement.Ascending => "ASC",
            SqlStatement.Descending => "DESC",
            SqlStatement.GroupBy => "GROUP BY",
            SqlStatement.Wildcard => " * ",

            _ when Enum.IsDefined(statement) => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(statement))
        });

        return this;
    }

    private void Space() => _sb.Append(' ');
}
