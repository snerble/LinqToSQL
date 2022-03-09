using System.ComponentModel.DataAnnotations;

namespace LinqToSQL.Entities;

public class Command
{
    [Key]
    public int? Id { get; set; }
    [Required]
    public string? Name { get; set; }
    public long Mode { get; set; }
    public long Permissions { get; set; }

    public override string? ToString()
    {
        return $"Id: {Id}, Name: {Name}";
    }
}