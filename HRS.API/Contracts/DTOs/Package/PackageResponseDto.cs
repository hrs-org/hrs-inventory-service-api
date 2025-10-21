namespace HRS.API.Contracts.DTOs.Package;

public class PackageItemResponseDto
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class PackageRateResponseDto
{
    public int MinDays { get; set; }
    public decimal DailyRate { get; set; }
    public bool IsActive { get; set; }
}

public class PackageResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }

    public ICollection<PackageItemResponseDto>? Items { get; set; }
    public ICollection<PackageRateResponseDto>? Rates { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
