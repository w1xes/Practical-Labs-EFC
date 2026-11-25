using System.ComponentModel.DataAnnotations;
using TaskEFC.Models;

namespace TaskEFC.ViewModels;

public class OrderIndexViewModel
{
    public required IReadOnlyList<Order> Orders { get; init; }

    /// <summary>The order whose details are displayed, if the user selected one.</summary>
    public Order? SelectedOrder { get; init; }
}

public class OrderCreateViewModel
{
    [Required]
    [Display(Name = "Customer")]
    public int? CustomerId { get; set; }

    [Required]
    [Display(Name = "SuperMarket")]
    public int? SupermarketId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "OrderDate")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? OrderDate { get; set; }

    /// <summary>One line per available product; lines with a positive quantity are ordered.</summary>
    public List<OrderLineViewModel> Lines { get; set; } = new();
}

public class OrderLineViewModel
{
    public int ProductId { get; set; }

    // Name and price are for display only and are always read from the database.
    public string? ProductName { get; set; }

    public double Price { get; set; }

    [Range(0, OrderDetail.MaxQuantity)]
    public double? Quantity { get; set; }
}
