using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace LinqToSQL.Utilities;

/// <summary>
/// Provides utility methods for entity types.
/// </summary>
internal static class EntityUtilities
{
    /// <summary>
    /// Returns the table name for the specified type.
    /// </summary>
    /// <param name="type">The type of the entity.</param>
    public static string GetTableName(Type type)
    {
        var attr = type.GetCustomAttribute<TableAttribute>();
        return attr?.Name ?? type.Name;
    }

    /// <summary>
    /// Returns the primary key member for the specified type.
    /// </summary>
    /// <param name="type">The type of the entity.</param>
    public static MemberInfo? GetKey(Type type)
    {
        return type
            .GetMembers(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault();
    }

    /// <summary>
    /// Returns the column name of the specified member.
    /// </summary>
    /// <param name="member">The member of an entity type.</param>
    public static string GetColumnName(MemberInfo member)
    {
        var attr = member.GetCustomAttribute<ColumnAttribute>();
        return attr?.Name ?? member.Name;
    }
}
