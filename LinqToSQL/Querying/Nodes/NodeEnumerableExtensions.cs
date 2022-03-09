namespace LinqToSQL.Querying.Nodes;

/// <summary>
/// Provides extensions for collections of <see cref="IQueryNode"/>.
/// </summary>
internal static class NodeEnumerableExtensions
{
    /// <summary>
    /// Builds the collection of <paramref name="nodes"/>.
    /// </summary>
    /// <param name="nodes">The nodes to build.</param>
    /// <param name="query">The query builder to use.</param>
    public static void Build(this IEnumerable<IQueryNode> nodes, IQueryBuilder query)
    {
        foreach (var node in nodes)
            node.Build(query);
    }
}
