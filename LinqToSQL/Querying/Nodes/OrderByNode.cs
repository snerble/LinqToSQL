using LinqToSQL.Utilities;
using System.Linq.Expressions;

namespace LinqToSQL.Querying.Nodes;

/// <summary>
/// Defines an ORDER BY statement in an SQL query.
/// </summary>
internal class OrderByNode : IQueryNode
{
    private readonly IQueryNode _orderValueNode;
    private readonly OrderDirection _direction;

    /// <summary>
    /// Initializes a new instance of <see cref="OrderByNode"/> using the specified
    /// expression and direction.
    /// </summary>
    /// <param name="expression">The expression that selects the member to order by.</param>
    /// <param name="direction">The direction to order by.</param>
    public OrderByNode(IQueryNode parent, LambdaExpression expression, OrderDirection direction)
    {
        Parent = parent;
        _direction = direction;

        _orderValueNode = expression.Body switch
        {
            MemberExpression { NodeType: ExpressionType.MemberAccess } me => MemberAccessNode.FromMemberExpression(this, me),
            ParameterExpression { NodeType: ExpressionType.Parameter } pe => new ExpressionNode(this, pe),
            _ => throw new ArgumentException($"Invalid expression '{expression.GetDebugView()}'")
        };
    }

    public IQueryNode Parent { get; }
    public IReadOnlyList<IQueryNode> Children => new[] { _orderValueNode };

    public void Build(IQueryBuilder query)
    {
        query.Statement(SqlStatement.OrderBy);

        foreach (var child in Children)
            child.Build(query);

        query.Statement(_direction == OrderDirection.Ascending
            ? SqlStatement.Ascending
            : SqlStatement.Descending);
    }
}
