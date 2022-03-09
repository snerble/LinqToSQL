using LinqToSQL.Entities;
using Microsoft.Data.Sqlite;
using System.Diagnostics.CodeAnalysis;

namespace LinqToSQL;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(string connectionString) : base(new SqliteConnection(connectionString))
    {
    }

    [NotNull]
    public DbSet<Command>? Commands { get; set; }
}
