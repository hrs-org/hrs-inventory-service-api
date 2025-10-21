using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HRS.Domain.Entities;

[BsonNoId]
public class PackageItem
{
    [BsonId]
    [BsonElement("id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonRepresentation(BsonType.ObjectId)]
    public string ItemId { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonIgnore]
    public Item? Item { get; set; }

    [BsonElement("quantity")]
    public int Quantity { get; set; } = 1;
}
