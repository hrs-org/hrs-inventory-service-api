using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using MongoDB.Driver;

namespace HRS.Infrastructure.Repositories;

public class ItemRateRepository : CrudRepository<ItemRate>, IItemRateRepository
{
    private new readonly IMongoCollection<ItemRate> _collection;

    public ItemRateRepository(MongoContext context) : base(context, "ItemRates")
    {
        _collection = context.Database.GetCollection<ItemRate>("ItemRates");
    }

    public async Task<IEnumerable<ItemRate>> GetRatesByItemIdAsync(int itemId, bool activeOnly = true)
    {
        var filterBuilder = Builders<ItemRate>.Filter;
        var filter = filterBuilder.Eq(r => r.ItemId, itemId);

        if (activeOnly)
        {
            filter &= filterBuilder.Eq(r => r.IsActive, true);
        }

        var sort = Builders<ItemRate>.Sort.Ascending(r => r.MinDays);

        return await _collection.Find(filter).Sort(sort).ToListAsync();
    }

    public async Task<ItemRate?> GetApplicableRateAsync(int itemId, int rentalDays)
    {
        var filterBuilder = Builders<ItemRate>.Filter;
        var filter = filterBuilder.Eq(r => r.ItemId, itemId)
                   & filterBuilder.Eq(r => r.IsActive, true)
                   & filterBuilder.Lte(r => r.MinDays, rentalDays);

        var sort = Builders<ItemRate>.Sort.Descending(r => r.MinDays);

        return await _collection.Find(filter).Sort(sort).FirstOrDefaultAsync();
    }
}
