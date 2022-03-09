using LinqToSQL.Querying.Nodes;
using System.Collections;

namespace LinqToSQL.Querying;

/// <summary>
/// Represents the structure of an SQL query and acts as the root node for any
/// SQL query.
/// </summary>
internal sealed class AbstractSyntaxTree : IQueryNode
{
    private readonly IQueryNode[] _nodes;

    public IQueryNode Parent => null!;
    public IReadOnlyList<IQueryNode> Children => _nodes;

    /// <summary>
    /// Initializes a new instance of <see cref="AbstractSyntaxTree"/> that is empty.
    /// </summary>
    public AbstractSyntaxTree() => _nodes = Array.Empty<IQueryNode>();
    /// <summary>
    /// Initializes a new instance of <see cref="AbstractSyntaxTree"/> that contains elements
    /// copied from the specified collection.
    /// </summary>
    /// <param name="other">The collection of nodes to copy.</param>
    public AbstractSyntaxTree(IEnumerable<IQueryNode> other) => _nodes = other.ToArray();

    public void Build(IQueryBuilder query)
    {
        var stack = new QueryParameterScopeStack();
        query.Properties[typeof(QueryParameterScopeStack)] = stack;
        stack.Push(new());

        _nodes.Build(query);
    }

    /// <summary>
    /// Returns a new instance of <see cref="AbstractSyntaxTree"/> that contains
    /// the elements of this instance with the specified node added to it.
    /// </summary>
    /// <param name="node">The node to append.</param>
    public AbstractSyntaxTree Append(IQueryNode node) => new(_nodes.Append(node));
}
