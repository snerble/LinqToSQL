namespace LinqToSQL.Querying.Nodes;

/// <summary>
/// Provides mechanisms for generating SQL.
/// </summary>
internal interface IQueryNode
{
    /// <summary>
    /// Gets the parent of this node.
    /// </summary>
    IQueryNode Parent { get; }
    /// <summary>
    /// Gets the children of this node.
    /// </summary>
    IReadOnlyList<IQueryNode> Children { get; }

    /// <summary>
    /// Builds the SQL for this node to the specified <paramref name="query"/>.
    /// </summary>
    /// <param name="query">The query builder to use.</param>
    void Build(IQueryBuilder query);
}
