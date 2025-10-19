using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using MongoDB.Driver;

namespace HRS.Infrastructure.Repositories;

public class PackageRepository : CrudRepository<Package>, IPackageRepository
{
    private new readonly IMongoCollection<Package> _collection;

    public PackageRepository(MongoContext context) : base(context, "Packages")
    {
        _collection = context.Database.GetCollection<Package>("Packages");
    }

    public async Task<IEnumerable<Package>> GetAllWithDetailsAsync()
    {
        return await _collection.Find(FilterDefinition<Package>.Empty).ToListAsync();
    }

    public async Task<Package?> GetByIdWithDetailsAsync(int id)
    {
        var filter = Builders<Package>.Filter.Eq(p => p.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<Package?> GetByIdWithItemsAsync(int id)
    {
        var filter = Builders<Package>.Filter.Eq(p => p.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }
}
