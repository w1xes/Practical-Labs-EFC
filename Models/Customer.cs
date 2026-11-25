using System.ComponentModel.DataAnnotations;

namespace TaskEFC.Models;

public class Customer
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "LastName")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "FirstName")]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Address { get; set; }

    [EnumDataType(typeof(Discount))]
    public Discount? Discount { get; set; }

    [Display(Name = "Customer")]
    public string FullName => $"{FirstName} {LastName}";

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
