using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HRS.Domain.Entities;

[BsonNoId]
public class PackageRate
{
    [BsonElement("id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();


    [BsonElement("minDays")]
    public int MinDays { get; set; }

    [BsonElement("dailyRate")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal DailyRate { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("createdById")]
    public int CreatedById { get; set; }

    [BsonIgnore]
    public object? CreatedBy { get; set; }

    [BsonElement("updatedById")]
    [BsonIgnoreIfNull]
    public int? UpdatedById { get; set; }

    [BsonIgnore]
    public object? UpdatedBy { get; set; }

    [BsonElement("updatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonIgnoreIfNull]
    public DateTime? UpdatedAt { get; set; }
}
