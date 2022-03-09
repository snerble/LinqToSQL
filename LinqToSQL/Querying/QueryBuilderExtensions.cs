using LinqToSQL.Utilities;
using System.Reflection;

namespace LinqToSQL.Querying;

/// <summary>
/// Provides extension methods for instances of <see cref="IQueryBuilder"/>.
/// </summary>
public static class QueryBuilderExtensions
{
    /// <summary>
    /// Inserts a reference to the specified entity.
    /// </summary>
    /// <param name="type">The type of the entity to reference.</param>
    /// <inheritdoc cref="IQueryBuilder.Reference(string, string?)"/>
    public static IQueryBuilder Reference(this IQueryBuilder @this, Type type)
    {
        return @this.Reference(EntityUtilities.GetTableName(type));
    }

    /// <summary>
    /// Inserts a reference to the specified column.
    /// </summary>
    /// <param name="member">The member of the entity whose column to reference.</param>
    /// <inheritdoc cref="IQueryBuilder.Reference(string, string?)"/>
    public static IQueryBuilder Reference(this IQueryBuilder @this, MemberInfo member)
    {
        string table = EntityUtilities.GetTableName(member.DeclaringType!);
        string column = EntityUtilities.GetColumnName(member);

        return @this.Reference(table, column);
    }
}