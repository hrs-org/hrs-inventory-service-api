using MongoDB.Driver;
using HRS.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace HRS.Infrastructure;

public class MongoContext
{
    private readonly IMongoDatabase _database;

    public MongoContext(IMongoClient client)
    {
        _database = client.GetDatabase("hrsdb-inventory");
    }

    public IMongoDatabase Database => _database;
    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }

    public async Task CreateIndexesAsync()
    {
        var itemsCollection = _database.GetCollection<Item>("Items");
        var packagesCollection = _database.GetCollection<Package>("Packages");

        var storeIdIndexModel = new CreateIndexModel<Item>(
            Builders<Item>.IndexKeys.Ascending(x => x.StoreId)
        );
        await itemsCollection.Indexes.CreateOneAsync(storeIdIndexModel);

        var packageStoreIdIndexModel = new CreateIndexModel<Package>(
            Builders<Package>.IndexKeys.Ascending(x => x.StoreId)
        );
        await packagesCollection.Indexes.CreateOneAsync(packageStoreIdIndexModel);
    }
}
