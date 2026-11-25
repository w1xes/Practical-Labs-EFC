using TaskEFC.Models;

namespace TaskEFC.ViewModels;

public static class CustomerSortOrder
{
    public const string LastNameDescending = "name_desc";
    public const string AddressAscending = "address";
    public const string AddressDescending = "address_desc";
}

public class CustomerIndexViewModel
{
    public required IReadOnlyList<Customer> Customers { get; init; }

    public string? SortOrder { get; init; }

    public string? SearchString { get; init; }

    private bool SortedByLastNameAscending =>
        SortOrder is not (CustomerSortOrder.LastNameDescending
            or CustomerSortOrder.AddressAscending
            or CustomerSortOrder.AddressDescending);

    /// <summary>Sort order requested by the next click on the LastName header.</summary>
    public string? NextLastNameSort =>
        SortedByLastNameAscending ? CustomerSortOrder.LastNameDescending : null;

    /// <summary>Sort order requested by the next click on the Address header.</summary>
    public string NextAddressSort =>
        SortOrder == CustomerSortOrder.AddressAscending
            ? CustomerSortOrder.AddressDescending
            : CustomerSortOrder.AddressAscending;

    public string LastNameIndicator => SortOrder switch
    {
        CustomerSortOrder.LastNameDescending => "▼",
        _ when SortedByLastNameAscending => "▲",
        _ => string.Empty
    };

    public string AddressIndicator => SortOrder switch
    {
        CustomerSortOrder.AddressAscending => "▲",
        CustomerSortOrder.AddressDescending => "▼",
        _ => string.Empty
    };
}
