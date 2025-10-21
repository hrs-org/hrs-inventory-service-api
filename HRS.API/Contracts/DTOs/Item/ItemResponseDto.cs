namespace HRS.API.Contracts.DTOs.Item;

public class ItemResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string ParentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public ICollection<ItemRateResponseDto>? Rates { get; set; }
    public ICollection<ItemResponseDto>? Children { get; set; }
    public bool HasChildren => Children?.Count > 0;
    public int StoreId { get; set; }
}

public class ItemRateResponseDto
{
    public string Id { get; set; } = string.Empty;
    public int MinDays { get; set; }
    public decimal DailyRate { get; set; }
    public bool IsActive { get; set; }
}
