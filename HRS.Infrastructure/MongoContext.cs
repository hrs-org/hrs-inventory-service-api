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
}
