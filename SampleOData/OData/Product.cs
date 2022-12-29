using System.ComponentModel.DataAnnotations;

namespace WebApi;

public class Product
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    //public Guid? CategoryId { get; set; }
    //public Category? Category { get; set; }
}
