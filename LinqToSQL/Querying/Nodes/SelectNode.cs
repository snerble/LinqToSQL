using System.Linq.Expressions;

namespace LinqToSQL.Querying.Nodes;

/// <summary>
/// Defines a <c>SELECT</c> statement in an SQL query.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being selected.</typeparam>
/// <typeparam name="TResult">The type of the resulting element.</typeparam>
internal class SelectNode : IQueryNode
{
    private readonly LambdaExpression _expression;
    private readonly List<MemberAccessNode> _memberAccessors = new();

    /// <summary>
    /// The NewExpression used to obtain the <see cref="_memberAccessors"/> (if any).
    /// </summary>
    private NewExpression? _newExpression;

    /// <summary>
    /// Initializes a new instance of <see cref="SelectNode{TEntity, TResult}"/> using
    /// the specified <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The expression to use for this <see cref="SelectNode{TEntity, TResult}"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <see langword="null"/>.</exception>
    public SelectNode(IQueryNode parent, LambdaExpression expression)
    {
        Parent = parent ?? throw new ArgumentNullException(nameof(parent));
        _expression = expression ?? throw new ArgumentNullException(nameof(expression));
        GetMemberAccessors();
    }

    public IQueryNode Parent { get; }
    public IReadOnlyList<IQueryNode> Children => _memberAccessors;

    /// <summary>
    /// Populates the <see cref="_memberAccessors"/> list from expressions in the
    /// <see cref="_expression"/>.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    private void GetMemberAccessors()
    {
        switch (_expression.Body)
        {
            // Simple member access
            case MemberExpression { NodeType: ExpressionType.MemberAccess } me:
                _memberAccessors.Add(MemberAccessNode.FromMemberExpression(this, me));
                break;

            // Anonymous type object with member access expressions inside
            case NewExpression ne:
                _memberAccessors.AddRange(MemberAccessNode.FromNewExpression(this, ne));
                _newExpression = ne;
                break;

            case ParameterExpression { NodeType: ExpressionType.Parameter } pe:
                break;

            default:
                throw new ArgumentException("Invalid expression");
        }
    }

    public void Build(IQueryBuilder query)
    {
        var stack = (QueryParameterScopeStack)query.Properties[typeof(QueryParameterScopeStack)];

        // Add the NewExpression to the list in the query properties
        if (_newExpression != null)
        {
            if (!query.Properties.TryGetValue(typeof(List<NewExpression>), out var newExpressionsObj))
                newExpressionsObj = query.Properties[typeof(List<NewExpression>)] = new List<NewExpression>();

            var newExpressions = (List<NewExpression>)newExpressionsObj;
            newExpressions.Add(_newExpression);
        }

        // Begin the query
        query.Statement(SqlStatement.Select);
        
        // Add a wildcard if no members were accessed
        if (_memberAccessors.Count == 0)
        {
            query.Statement(SqlStatement.Wildcard);
        }
        else if (_memberAccessors.Count == 1)
        {
            // Add remapping data to the current query parameter scope
            stack.Last().ParameterMember = _memberAccessors.Single().SrcMember;
        }
        else
        {
            // Add remapping data to the current query parameter scope
            var mapping = stack.Last().MemberMapping;
            foreach (var m in _memberAccessors)
            {
                // Ensure member alias is present (should not happen)
                if (m.DstMember == null)
                    throw new InvalidOperationException("Missing member alias");

                mapping[m.DstMember] = m.SrcMember;
            }
        }

        // Finish the select query
        for (int i = 0; i < _memberAccessors.Count; i++)
        {
            if (i > 0)
                query.Literal(",");
            _memberAccessors[i].Build(query);
        }

        query
            .Statement(SqlStatement.From)
            .Reference(_expression.Parameters.Single().Type);
    }
}