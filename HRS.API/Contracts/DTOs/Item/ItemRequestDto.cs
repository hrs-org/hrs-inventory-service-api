using System.ComponentModel.DataAnnotations;

namespace HRS.API.Contracts.DTOs.Item;

public class ItemRequestDto
{
    [Required] public required string Name { get; set; }

    public string Description { get; set; } = string.Empty;

    [Required][Range(0, int.MaxValue)] public required int Quantity { get; set; }

    [Required]
    [Range(0.0, double.MaxValue)]
    public required decimal Price { get; set; }
}

public class ItemRateRequestDto
{
    [Range(1, int.MaxValue)] public int MinDays { get; set; }

    [Range(0.0, double.MaxValue)] public decimal DailyRate { get; set; }

    public bool IsActive { get; set; } = true;
}

public class ParentItemRequestDto : ItemRequestDto
{
    [Required] public required string StoreId { get; set; }
    public ICollection<ItemRateRequestDto>? Rates { get; set; }
}

public class AddItemRequestDto : ParentItemRequestDto
{
    public ICollection<ItemRequestDto>? Children { get; set; }
}

public class UpdateItemRequestDto : ParentItemRequestDto
{
    public string? Id { get; set; }
    public ICollection<UpdateItemRequestDto>? Children { get; set; }
}

public class UpdateItemQuantityRequestDto
{
    [Required] public string? Id { get; set; }

    [Required] public required int Quantity { get; set; }
}
