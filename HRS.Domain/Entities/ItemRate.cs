using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRS.Shared.Core.Dtos;

namespace HRS.Domain.Entities;

[Table("ItemRates")]
public class ItemRate
{
    [Key] public int Id { get; set; }

    [Required] public int ItemId { get; set; }

    [ForeignKey(nameof(ItemId))] public Item Item { get; set; } = null!;

    [Required][Range(1, int.MaxValue)] public int MinDays { get; set; }

    [Required]
    [Range(0.0, double.MaxValue)]
    public decimal DailyRate { get; set; }

    [Required] public bool IsActive { get; set; } = true;

    [Required] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CreatedById { get; set; }
    [ForeignKey(nameof(CreatedById))] public UserResponseDto CreatedBy { get; set; } = null!;

    public int? UpdatedById { get; set; }
    [ForeignKey(nameof(UpdatedById))] public UserResponseDto? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
