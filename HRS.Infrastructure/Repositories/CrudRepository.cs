using System.Linq.Expressions;
using HRS.Domain.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HRS.Infrastructure.Repositories;

public class CrudRepository<T> : ICrudRepository<T> where T : class
{
    protected readonly IMongoDatabase _db;
    protected readonly IMongoCollection<T> _collection;

    public CrudRepository(IMongoDatabase db, string collectionName)
    {
        _db = db;
        _collection = db.GetCollection<T>(collectionName);
    }

    public virtual async Task<T?> GetByIdAsync(object id)
    {
        var objectId = ConvertToObjectId(id);
        var filter = Builders<T>.Filter.Eq("_id", objectId);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _collection.InsertManyAsync(entities);
    }

    public virtual async Task UpdateAsync(T entity, object id)
    {
        var objectId = ConvertToObjectId(id);
        var filter = Builders<T>.Filter.Eq("_id", objectId);
        await _collection.ReplaceOneAsync(filter, entity);
    }
    public virtual async Task UpdateQuantityAsync(object id, int quantity)
    {
        var objectId = ConvertToObjectId(id);
        var filter = Builders<T>.Filter.Eq("_id", objectId);
        var update = Builders<T>.Update.Set("Quantity", quantity);
        await _collection.UpdateOneAsync(filter, update);
    }

    public virtual async Task RemoveAsync(object id)
    {
        var objectId = ConvertToObjectId(id);
        var filter = Builders<T>.Filter.Eq("_id", objectId);
        await _collection.DeleteOneAsync(filter);
    }

    public virtual async Task RemoveRangeAsync(IEnumerable<object> ids)
    {
        var objectIds = ids.Select(ConvertToObjectId);
        var filter = Builders<T>.Filter.In("_id", objectIds);
        await _collection.DeleteManyAsync(filter);
    }

    private static object ConvertToObjectId(object id)
    {
        if (id is string strId && ObjectId.TryParse(strId, out var objectId))
        {
            return objectId;
        }
        return id;
    }
}
