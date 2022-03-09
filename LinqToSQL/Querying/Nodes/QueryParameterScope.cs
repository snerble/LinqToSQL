using System.Collections;
using System.Reflection;

namespace LinqToSQL.Querying.Nodes;

/// <summary>
/// Contains information about the parameters currently available in the query.
/// </summary>
internal class QueryParameterScope
{
    /// <summary>
    /// Gets or sets the member from which the current expression parameter is derived.
    /// </summary>
    public virtual MemberInfo? ParameterMember { get; set; }
    /// <summary>
    /// Gets a mapping between members of the current entity members and the members
    /// from which they were originally derived.
    /// </summary>
    public virtual IDictionary<MemberInfo, MemberInfo> MemberMapping { get; } = new Dictionary<MemberInfo, MemberInfo>();
}

/// <summary>
/// Manages the stack of <see cref="QueryParameterScope"/> instances.
/// </summary>
internal class QueryParameterScopeStack : QueryParameterScope, IReadOnlyCollection<QueryParameterScope>
{
    private readonly Stack<QueryParameterScope> _stack = new();

    public override MemberInfo? ParameterMember => _stack
        .Reverse()
        .FirstOrDefault(x => x.ParameterMember != null)
        ?.ParameterMember;

    public override IDictionary<MemberInfo, MemberInfo> MemberMapping => new Dictionary<MemberInfo, MemberInfo>(_stack
        .Where(x => x.MemberMapping != null)
        .SelectMany(x => x.MemberMapping!));

    public int Count => ((IReadOnlyCollection<QueryParameterScope>)_stack).Count;

    public void Push(QueryParameterScope scope) => _stack.Push(scope);
    public void Pop() => _stack.Pop();

    public IEnumerator<QueryParameterScope> GetEnumerator() => ((IEnumerable<QueryParameterScope>)_stack).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_stack).GetEnumerator();
}