using System.ComponentModel.DataAnnotations;

namespace TaskEFC.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 1_000_000)]
    [DisplayFormat(DataFormatString = "{0:F2}")]
    public double Price { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
