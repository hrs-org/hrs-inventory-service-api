using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using MongoDB.Driver;

namespace HRS.Infrastructure.Repositories;

public class PackageRepository : CrudRepository<Package>, IPackageRepository
{
    private new readonly IMongoCollection<Package> _collection;

    public PackageRepository(MongoContext context) : base(context.Database, "Packages")
    {
        _collection = context.Database.GetCollection<Package>("Packages");
    }

    public async Task<IEnumerable<Package>> GetAllWithDetailsAsync(string storeId)
    {
        var filter = Builders<Package>.Filter.Eq(p => p.StoreId, storeId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<Package?> GetByIdWithDetailsAsync(string id)
    {
        var filter = Builders<Package>.Filter.Eq(p => p.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<Package?> GetByIdWithItemsAsync(string id)
    {
        var filter = Builders<Package>.Filter.Eq(p => p.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<object> UpdateAsync(Package package, string id)
    {
        var filter = Builders<Package>.Filter.Eq(p => p.Id, id);
        var updateResult = await _collection.ReplaceOneAsync(filter, package);
        return updateResult;
    }
    public async Task<object> RemoveAsync(string id)
    {
        var filter = Builders<Package>.Filter.Eq(p => p.Id, id);
        var deleteResult = await _collection.DeleteOneAsync(filter);
        return deleteResult;
    }
}
