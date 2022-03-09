using System.Linq.Expressions;
using System.Reflection;

namespace LinqToSQL.Utilities;

/// <summary>
/// Defines extension methods on the <see cref="Expression"/> class.
/// </summary>
public static class ExpressionExtensions
{
    private static readonly PropertyInfo Expression_DebugView_Prop;

    static ExpressionExtensions()
    {
        Expression_DebugView_Prop = typeof(Expression)
            .GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic)!;
    }

    /// <summary>
    /// Returns the value of the internal DebugView property from this <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The expression whose DebugView to get.</param>
    public static string GetDebugView(this Expression expression)
    {
        return (string)Expression_DebugView_Prop.GetValue(expression, null)!;
    }
}
