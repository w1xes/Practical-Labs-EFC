using System.ComponentModel.DataAnnotations;

namespace TaskEFC.Models;

public class Order
{
    public int Id { get; set; }

    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [Display(Name = "SuperMarket")]
    public int SupermarketId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "OrderDate")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime OrderDate { get; set; }

    public Customer? Customer { get; set; }

    [Display(Name = "SuperMarket")]
    public Supermarket? Supermarket { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    /// <summary>Requires the order details and their products to be loaded.</summary>
    [Display(Name = "Total cost")]
    [DisplayFormat(DataFormatString = "{0:F2}")]
    public double TotalCost => OrderDetails.Sum(d => d.Cost);
}
