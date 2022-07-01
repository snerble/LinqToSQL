using LinqToSQL.Utilities;
using System.Linq.Expressions;

namespace LinqToSQL.Querying.Nodes;

internal class GroupByNode : IQueryNode
{
    private readonly IQueryNode[] _groupByValueNodes;

    public GroupByNode(IQueryNode parent, LambdaExpression expression)
	{
		Parent = parent;

        _groupByValueNodes = expression.Body switch
        {
            NewExpression ne => MemberAccessNode.FromNewExpression(this, ne).ToArray(),
            MemberExpression { NodeType: ExpressionType.MemberAccess } me => new[] { MemberAccessNode.FromMemberExpression(this, me) },
            _ => throw new ArgumentException($"Invalid expression '{expression.GetDebugView()}'")
        };
    }

	public IQueryNode Parent { get; }
	public IReadOnlyList<IQueryNode> Children => _groupByValueNodes;

    public void Build(IQueryBuilder query)
	{
        query.Statement(SqlStatement.GroupBy);

        // Finish the group by query
        for (int i = 0; i < Children.Count; i++)
        {
            if (i > 0)
                query.Literal(",");
            _groupByValueNodes[i].Build(query);
        }
    }
}