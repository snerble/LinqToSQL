using LinqToSQL.Utilities;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqToSQL.Querying.Nodes;

/// <summary>
/// Defines a member access expression in an SQL query.
/// </summary>
internal sealed class MemberAccessNode : IQueryNode
{
    private MemberAccessNode(IQueryNode parent, MemberInfo srcMember, MemberInfo? dstMember)
    {
        Parent = parent;
        SrcMember = srcMember;
        DstMember = dstMember;
    }

    /// <summary>
    /// Gets the member that is accessed.
    /// </summary>
    public MemberInfo SrcMember { get; }
    /// <summary>
    /// Gets the member to which the <see cref="SrcMember"/> is aliased to,
    /// or <see langword="null"/> if there is no alias.
    /// </summary>
    public MemberInfo? DstMember { get; }

    public IQueryNode Parent { get; }
    public IReadOnlyList<IQueryNode> Children => throw new NotImplementedException();

    /// <summary>
    /// Returns a new instance of <see cref="MemberAccessNode"/> from the specified
    /// <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The member expression to create a node from.</param>
    /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The node type is not <see cref="ExpressionType.MemberAccess"/></exception>
    public static MemberAccessNode FromMemberExpression(IQueryNode parent, MemberExpression expression)
    {
        if (expression is null)
            throw new ArgumentNullException(nameof(expression));

        if (expression.NodeType != ExpressionType.MemberAccess)
            throw new ArgumentException($"NodeType must be {ExpressionType.MemberAccess}");

        return new(parent, expression.Member, null);
    }

    /// <summary>
    /// Returns a collection of new instances of <see cref="MemberAccessNode"/> from the
    /// specified <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The expression that constructs a new object.</param>
    /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// The <paramref name="expression"/> contains expressions that are not
    /// <see cref="MemberExpression"/>s.
    /// </exception>
    public static IEnumerable<MemberAccessNode> FromNewExpression(IQueryNode parent, NewExpression expression)
    {
        if (expression is null)
            throw new ArgumentNullException(nameof(expression));

        if (expression.Members is null)
            throw new ArgumentException("Members of NewExpression are null");

        foreach (var (member, arg) in expression.Members.Zip(expression.Arguments))
        {
            yield return arg switch
            {
                MemberExpression me => new(parent, FromMemberExpression(parent, me).SrcMember, member),
                _ => throw new ArgumentException("Invalid expression"),
            };
        }
    }

    public void Build(IQueryBuilder query)
    {
        // Get parameter remapping data
        var stack = (QueryParameterScopeStack)query.Properties[typeof(QueryParameterScopeStack)];

        // Remap the src member or fall back to the src member
        if (!stack.MemberMapping.TryGetValue(SrcMember, out var member))
            member = SrcMember;
        query.Reference(member);

        if (DstMember != null && DstMember.Name != SrcMember.Name)
            query.Statement(SqlStatement.As, EntityUtilities.GetColumnName(DstMember));
    }
}