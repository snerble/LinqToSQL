using System.Data.Common;

namespace LinqToSQL;

public abstract class DbContext : IDisposable
{
    private readonly DbConnection _connection;

    private bool disposedValue;

    public DbContext(DbConnection connection)
    {
        _connection = connection;
        _connection.Open();
        PopulateDbSets();
    }

    private void PopulateDbSets()
    {
        var dbSetProps = GetType()
            .GetProperties()
            .Where(x => x.CanWrite)
            .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .ToArray();

        foreach (var prop in dbSetProps)
        {
            var dbSet = Activator.CreateInstance(prop.PropertyType, _connection);
            prop.SetValue(this, dbSet);
        }
    }

    public DbConnection GetConnection() => _connection;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _connection.Dispose();
            }

            disposedValue = true;
        }
    }

    ~DbContext()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}