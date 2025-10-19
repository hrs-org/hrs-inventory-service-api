using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using MongoDB.Driver;

namespace HRS.Infrastructure.Repositories;

public class PackageRateRepository : CrudRepository<PackageRate>, IPackageRateRepository
{
    private new readonly IMongoCollection<PackageRate> _collection;

    public PackageRateRepository(MongoContext context) : base(context, "PackageRates")
    {
        _collection = context.Database.GetCollection<PackageRate>("PackageRates");
    }

    public async Task<IEnumerable<PackageRate>> GetRatesByPackageIdAsync(int packageId)
    {
        var filterBuilder = Builders<PackageRate>.Filter;
        var filter = filterBuilder.Eq(r => r.PackageId, packageId)
                   & filterBuilder.Eq(r => r.IsActive, true);

        var sort = Builders<PackageRate>.Sort.Ascending(r => r.MinDays);

        return await _collection.Find(filter).Sort(sort).ToListAsync();
    }
}
