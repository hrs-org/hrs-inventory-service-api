using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using MongoDB.Driver;

namespace HRS.Infrastructure.Repositories;

public class ItemRepository : CrudRepository<Item>, IItemRepository
{
    private new readonly IMongoCollection<Item> _collection;

    public ItemRepository(MongoContext context) : base(context, "Items")
    {
        _collection = context.Database.GetCollection<Item>("Items");
    }

    public async Task<IEnumerable<Item>> GetRootItemsAsync()
    {
        var filter = Builders<Item>.Filter.Eq(i => i.ParentId, null);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<Item?> GetByIdWithChildrenAsync(int id)
    {
        var filter = Builders<Item>.Filter.Eq(i => i.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
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
}
