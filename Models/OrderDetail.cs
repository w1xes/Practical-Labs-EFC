using System.ComponentModel.DataAnnotations;

namespace TaskEFC.Models;

/// <summary>
/// A single product line of an order.
/// </summary>
public class OrderDetail
{
    public const double MinQuantity = 0.001;
    public const double MaxQuantity = 100_000;

    public int Id { get; set; }

    public int OrderId { get; set; }

    [Display(Name = "Product")]
    public int ProductId { get; set; }

    [Range(MinQuantity, MaxQuantity)]
    public double Quantity { get; set; }

    public Order? Order { get; set; }

    public Product? Product { get; set; }

    /// <summary>Requires <see cref="Product"/> to be loaded.</summary>
    [DisplayFormat(DataFormatString = "{0:F2}")]
    public double Cost => (Product?.Price ?? 0) * Quantity;
}
