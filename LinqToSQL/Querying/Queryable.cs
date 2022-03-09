using LinqToSQL.Querying.Nodes;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LinqToSQL.Querying;

/// <summary>
/// Implementation of <see cref="IQueryable{TEntity}"/>.
/// </summary>
/// <inheritdoc cref="IQueryable{TEntity}"/>
internal class Queryable<TEntity> : IQueryable<TEntity>
{
    private readonly AbstractSyntaxTree _syntaxTree;
    private readonly DbConnection _connection;

    internal Queryable(DbConnection connection)
    {
        _connection = connection;
        _syntaxTree = new();
    }

    private Queryable(DbConnection connection, AbstractSyntaxTree ast)
    {
        _connection = connection;
        _syntaxTree = ast;
    }

    public async IAsyncEnumerable<TEntity> QueryAsync(
        DbTransaction? transaction = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = new SQLiteQueryBuilder();
        _syntaxTree.Build(query);

        using var command = query.Build();
        command.Connection = _connection;
        command.Transaction = transaction;

        var reader = await command.ExecuteReaderAsync(cancellationToken);

        // Map the results to instances of TEntity
        await using (reader.ConfigureAwait(false))
        {
            if (!reader.HasRows)
                yield break;

            // Return single values
            if (reader.FieldCount == 1)
            {
                while (reader.Read())
                    yield return (TEntity)Convert.ChangeType(reader.GetValue(0), typeof(TEntity));
            }
            // Check if the query has a NewExpression stored for TEntity and run the MapNewExpression
            else if (query.Properties.TryGetValue(typeof(List<NewExpression>), out var newExpressionsObj)
                && newExpressionsObj is List<NewExpression> newExpressions
                && newExpressions.SingleOrDefault(x => x.Type == typeof(TEntity)) is { } expression)
            {
                await foreach (var entity in MapNewExpression(reader, expression, cancellationToken))
                    yield return entity;
            }
            // Fallback to MapParameterless
            else
            {
                await foreach (var entity in MapParameterless(reader, cancellationToken))
                    yield return entity;
            }
        }
    }

    /// <summary>
    /// Maps a data reader to instances of <typeparamref name="TEntity"/> by using
    /// an instance of <see cref="NewExpression"/>.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async IAsyncEnumerable<TEntity> MapNewExpression(
        DbDataReader reader,
        NewExpression expression,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var parameters = expression.Constructor!.GetParameters();

        // Read the next record
        while (await reader.ReadAsync(cancellationToken))
        {
            // Prepare array of arguments
            object?[] args = new object[expression.Arguments.Count];

            // Populate all fields
            for (int i = 0; i < expression.Members!.Count; i++)
            {
                var arg = parameters[i];
                args[i] = ConvertValue(reader.GetValue(i), arg.ParameterType);
            }

            yield return (TEntity)expression.Constructor.Invoke(args);
        }
    }

    /// <summary>
    /// Maps a data reader to instances of <typeparamref name="TEntity"/> using a
    /// parameterless constructor.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async IAsyncEnumerable<TEntity> MapParameterless(
        DbDataReader reader,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Prepare a list of member info objects
        var members = Enumerable
            .Range(0, reader.FieldCount)
            .Select(i => reader.GetName(i))
            .Select(n => (MemberInfo?)typeof(TEntity).GetProperty(n) ?? typeof(TEntity).GetField(n)!)
            .Where(x => x != null)
            .ToArray();

        // Read the next record
        while (await reader.ReadAsync(cancellationToken))
        {
            // Construct a new instance
            TEntity instance = Activator.CreateInstance<TEntity>()!;

            // Populate all fields
            for (int i = 0; i < members.Length; i++)
            {
                object? value = reader.GetValue(i);
                MapMember(instance, members[i], value);
            }

            yield return instance;
        }
    }

    /// <summary>
    /// Maps a value to an instance member.
    /// </summary>
    /// <param name="instance">The instance to map to.</param>
    /// <param name="member">The member to map to.</param>
    /// <param name="value">The value to map.</param>
    private static void MapMember(object instance, MemberInfo member, object? value)
    {
        switch (member)
        {
            case FieldInfo field:
                field.SetValue(instance, ConvertValue(value, field.FieldType));
                break;

            case PropertyInfo prop:
                prop.SetValue(instance, ConvertValue(value, prop.PropertyType));
                break;
        }
    }

    /// <summary>
    /// Performs tedious conversions when needed.
    /// </summary>
    private static object? ConvertValue(object? value, Type type)
    {
        // Convert value to nullable if needed
        if (Nullable.GetUnderlyingType(type) is Type tnullable)
        {
            value = typeof(Nullable<>)
                .MakeGenericType(tnullable)
                .GetConstructors()
                .Single()
                .Invoke(new[] { Convert.ChangeType(value, tnullable) });
        }

        return value;
    }

    public IQueryable<TResult> Select<TResult>(Expression<Func<TEntity, TResult>> expression)
    {
        return new Queryable<TResult>(_connection, _syntaxTree.Append(new SelectNode(_syntaxTree, expression)));
    }

    public IQueryable<TEntity> Where(Expression<Predicate<TEntity>> expression)
    {
        return new Queryable<TEntity>(_connection, _syntaxTree.Append(new WhereNode(_syntaxTree, expression)));
    }

    public IQueryable<TEntity> GroupBy<TGroup>(Expression<Func<TEntity, TGroup>> expression)
    {
        throw new NotImplementedException();
    }

    public IQueryable<TEntity> OrderBy<TOrder>(Expression<Func<TEntity, TOrder>> expression, OrderDirection direction = OrderDirection.Ascending)
    {
        return new Queryable<TEntity>(_connection, _syntaxTree.Append(new OrderByNode(_syntaxTree, expression, direction)));
    }
}
