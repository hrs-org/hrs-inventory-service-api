using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HRS.Domain.Entities;

public class Package
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("name")]
    [BsonRequired]
    public string Name { get; set; } = null!;

    [BsonElement("description")]
    [BsonIgnoreIfNull]
    public string? Description { get; set; }

    [BsonElement("basePrice")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal BasePrice { get; set; }

    [BsonElement("items")]
    public ICollection<PackageItem> PackageItems { get; set; } = [];

    [BsonElement("rates")]
    public ICollection<PackageRate> PackageRates { get; set; } = [];

    [BsonElement("createdById")]
    public int CreatedById { get; set; }

    [BsonIgnore]
    public object? CreatedBy { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedById")]
    [BsonIgnoreIfNull]
    public int? UpdatedById { get; set; }

    [BsonIgnore]
    public object? UpdatedBy { get; set; }

    [BsonElement("updatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
