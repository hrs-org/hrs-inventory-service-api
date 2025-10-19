using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HRS.Domain.Entities;

public class Item
{
    [BsonId]
    [BsonRepresentation(BsonType.Int32)]
    public int Id { get; set; }

    [BsonElement("name")]
    [BsonRequired]
    public string Name { get; set; } = null!;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("quantity")]
    [BsonRequired]
    public int Quantity { get; set; }

    [BsonElement("price")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Price { get; set; }

    [BsonElement("parentId")]
    [BsonIgnoreIfNull]
    public int? ParentId { get; set; }

    [BsonIgnore]
    public Item? Parent { get; set; }

    [BsonIgnore]
    public ICollection<Item> Children { get; set; } = [];

    [BsonIgnore]
    public ICollection<ItemRate> Rates { get; set; } = [];

    [BsonElement("createdById")]
    public int CreatedById { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedById")]
    [BsonIgnoreIfNull]
    public int? UpdatedById { get; set; }

    [BsonElement("updatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
