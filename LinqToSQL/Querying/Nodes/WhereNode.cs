using LinqToSQL.Utilities;
using System.Linq.Expressions;

namespace LinqToSQL.Querying.Nodes;

/// <summary>
/// Defines a <c>WHERE</c> statement in an SQL query.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being tested.</typeparam>
internal class WhereNode : IQueryNode
{
    private readonly LambdaExpression _expression;
    private readonly List<IQueryNode> _conditionOperands = new();

    /// <summary>
    /// Initializes a new instance of <see cref="WhereNode{TEntity}"/> using the specified
    /// predicate <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The expression to use for this <see cref="WhereNode{TEntity}"/>.</param>
    public WhereNode(IQueryNode parent, LambdaExpression expression)
    {
        _expression = expression;
        GetOperands(expression.Body);
        Parent = parent;
    }

    public IQueryNode Parent { get; }
    public IReadOnlyList<IQueryNode> Children => _conditionOperands;

    /// <summary>
    /// Recursively populates the <see cref="_conditionOperands"/> list with all relevant data in the
    /// specified <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The expression to extract operands from.</param>
    /// <exception cref="ArgumentException">
    /// An expression was a <see cref="BinaryExpression"/>, <see cref="ConstantExpression"/>
    /// or a <see cref="MemberExpression"/> with a node type of <see cref="ExpressionType.MemberAccess"/>.
    /// </exception>
    private void GetOperands(Expression expression)
    {
        switch (expression)
        {
            // Logic expression
            case BinaryExpression be:
                GetOperands(be.Left);
                _conditionOperands.Add(new ExpressionNode(this, expression));
                GetOperands(be.Right);
                break;

            // Member access
            case MemberExpression { NodeType: ExpressionType.MemberAccess } me:
                _conditionOperands.Add(MemberAccessNode.FromMemberExpression(this, me));
                break;

            // Parameters
            case ParameterExpression { NodeType: ExpressionType.Parameter }:
            // Constants
            case ConstantExpression:
                _conditionOperands.Add(new ExpressionNode(this, expression));
                break;

            case MemberExpression { NodeType: ExpressionType.Call }:
                throw new ArgumentException($"Method calls are unsupported.\nExpression: {expression.GetDebugView()}");

            default:
                throw new ArgumentException($"Invalid expression '{expression.GetDebugView()}'");
        }
    }

    public void Build(IQueryBuilder query)
    {
        query.Statement(SqlStatement.Where);
        _conditionOperands.Build(query);
    }
}
