using System.Linq.Expressions;

namespace LinqToSQL.Querying.Nodes;

/// <summary>
/// Defines a generic expression in an SQL query.
/// </summary>
internal class ExpressionNode : IQueryNode
{
    private readonly Expression _expression;

    /// <summary>
    /// Initializes a new instance of <see cref="ExpressionNode"/> with the specified
    /// <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The expression to use in the query.</param>
    public ExpressionNode(IQueryNode parent, Expression expression)
    {
        _expression = expression;
        Parent = parent;
    }

    public IQueryNode Parent { get; }
    public IReadOnlyList<IQueryNode> Children => throw new NotImplementedException();

    public void Build(IQueryBuilder query)
    {
        if (_expression is ParameterExpression)
        {
            // Get parameter remapping data
            var stack = (QueryParameterScopeStack)query.Properties[typeof(QueryParameterScopeStack)];

            // Check if a parameter member is mapped (should not happen after a select node)
            if (stack.ParameterMember == null)
                throw new InvalidOperationException("Missing parameter member");

            query.Reference(stack.ParameterMember);
        }
        else
        {
            query.Expression(_expression);
        }
    }
}