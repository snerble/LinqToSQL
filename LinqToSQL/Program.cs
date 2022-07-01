// Entities
//  To tables
//  Collect data for mapping fields to sql

// Select
//  Map command output to entities
//  Map command output to anonymous type objects
//   Obtain mapping data from Expressions
//    !!! Class for handling Expression objects

// Where
//  Obtain logic info from Expressions
//   Obtain mapping data from Expressions
//    !!! Class for handling Expression objects

// Custom IQueryable interface

using LinqToSQL;

using var dbContext = new AppDbContext(@"Data Source=C:\Users\Conor\Desktop\Nepgeardam.db");

var query = dbContext.Commands
    .Select(c => new { c.Id, c.Name })
    //.OrderBy(x => x.Name)
    .GroupBy(x => x.Name)
    .QueryAsync();

await foreach (var item in query)
{
    Console.WriteLine(item);
}

Console.WriteLine();