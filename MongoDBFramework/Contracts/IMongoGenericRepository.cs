using MongoDB.Bson;

namespace MongoDBFramework.Contracts
{
    public interface IMongoGenericRepository<T>
    {
        Task AddAsync(T source);
        Task DeleteAsync(T entity);
        Task<bool> Exists(string key, object value);
        Task<bool> Exists(T entity);
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetAllAsync(BsonDocument query);
        Task<List<T>> GetAllAsync(string key, object value);
        Task<T> GetAsync(T entity);
        Task<T> GetAsync(BsonDocument query);
        Task<T> GetAsync(string key, object value);
        Task UpdateAsync(T source);
        Task UpdateAsync(string key, string value, T source);
    }
}