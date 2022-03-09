namespace LinqToSQL.Utilities;

/// <summary>
/// Provides extensions for instances of <see cref="IReadOnlyList{T}"/>.
/// </summary>
public static class ReadOnlyListExtensions
{
    /// <summary>
    /// Returns the index of the specified <paramref name="element"/>, or -1
    /// if the element is not in the <paramref name="list"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list of elements.</param>
    /// <param name="element">The element whose index to find.</param>
    public static int IndexOf<T>(this IReadOnlyList<T> list, T element)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (Equals(list[i], element))
                return i;
        }

        return -1;
    }
}
