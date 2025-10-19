using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace HRS.Infrastructure.Repositories;

public class ItemRepository : CrudRepository<Item>, IItemRepository
{
    private new readonly IMongoCollection<Item> _collection;

    public ItemRepository(MongoContext context) : base(context.Database, "Items")
    {
        _collection = context.Database.GetCollection<Item>("Items");
    }

    public async Task<IEnumerable<Item>> GetRootItemsAsync()
    {
        var filter = Builders<Item>.Filter.Eq(i => i.ParentId, null);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<Item?> GetByIdWithChildrenAsync(string id)
    {
        // Use aggregation to load children
        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument("_id", new ObjectId(id))),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "Items" },
                { "localField", "_id" },
                { "foreignField", "parentId" },
                { "as", "children" }
            })
        };

        var result = await _collection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();

        if (result == null)
            return null;

        return BsonSerializer.Deserialize<Item>(result);
    }

    public async Task<IEnumerable<Item>> SearchAsync(string? keyword)
    {
        var filterBuilder = Builders<Item>.Filter;
        FilterDefinition<Item> filter;

        if (string.IsNullOrWhiteSpace(keyword))
        {
            filter = filterBuilder.Empty;
        }
        else
        {
            filter = filterBuilder.Regex(i => i.Name, new MongoDB.Bson.BsonRegularExpression(keyword, "i"));
        }

        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<Item?> GetByIdWithParentAsync(string id)
    {
        // Use aggregation to load parent
        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument("_id", new ObjectId(id))),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "Items" },
                { "localField", "parentId" },
                { "foreignField", "_id" },
                { "as", "parent" }
            }),
            new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$parent" },
                { "preserveNullAndEmptyArrays", true }
            })
        };

        var result = await _collection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();

        if (result == null)
            return null;

        return BsonSerializer.Deserialize<Item>(result);
    }

    public async Task RemoveItem(Item entity)
    {
        await _collection.DeleteOneAsync(i => i.Id == entity.Id);
    }
}
