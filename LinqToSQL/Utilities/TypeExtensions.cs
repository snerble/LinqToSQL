using System.Reflection;
using System.Runtime.CompilerServices;

namespace LinqToSQL.Utilities;

/// <summary>
/// Provides extensions for instances of <see cref="Type"/>.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Returns whether a type is anonymous or not.
    /// </summary>
    /// <param name="type">The type to test.</param>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    public static bool CheckIfAnonymousType(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
            && type.IsGenericType && type.Name.Contains("AnonymousType")
            && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
            && type.Attributes.HasFlag(TypeAttributes.NotPublic);
    }

    /// <summary>
    /// Returns whether the specified type is nullable.
    /// </summary>
    /// <param name="type">The type to check</param>
    public static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) != null;
}
