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

    public async Task<IEnumerable<Item>> GetRootItemsAsync(string storeId)
    {
        var filter = Builders<Item>.Filter.And(
            Builders<Item>.Filter.Eq(i => i.ParentId, null),
            Builders<Item>.Filter.Eq(i => i.StoreId, storeId)
        );
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<Item?> GetParentItemAsync(string childId)
    {
        var child = await GetByIdAsync(childId);
        if (child?.ParentId == null) return null;

        return await GetByIdAsync(child.ParentId);
    }

    public async Task<IEnumerable<Item>> SearchAsync(string? keyword)
    {
        var filterBuilder = Builders<Item>.Filter;
        FilterDefinition<Item> filter;

        var filters = new List<FilterDefinition<Item>>();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            filters.Add(filterBuilder.Regex(i => i.Name, new MongoDB.Bson.BsonRegularExpression(keyword, "i")));
        }

        filter = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;

        return await _collection.Find(filter).ToListAsync();
    }

    public async Task RemoveItem(Item entity)
    {
        await _collection.DeleteOneAsync(i => i.Id == entity.Id);
    }
}
