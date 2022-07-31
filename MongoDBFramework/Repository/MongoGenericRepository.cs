using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDBFramework.Contracts;
using MongoDBFramework.Data;
using MongoDBFramework.Exceptions;

namespace MongoDBFramework.Repository
{
    public class MongoGenericRepository<T> : IMongoGenericRepository<T> where T : IMongoObject
    { 
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly ILogger<MongoGenericRepository<T>> _logger;

        public MongoGenericRepository(IMongoClient client, IConfiguration configuration, ILogger<MongoGenericRepository<T>> logger)
        {
            _logger = logger;
            var collectionName = configuration[$"MongoGenericRepo:{typeof(T).Name}:Collection"];
            var databaseName = configuration[$"MongoGenericRepo:{typeof(T).Name}:Database"];

            if (collectionName is null || databaseName is null) 
            { 
                throw new ArgumentNullException($"Config for {typeof(MongoGenericRepository<T>).Name}"); 
            }
            this._collection = client.GetDatabase(databaseName).GetCollection<BsonDocument>(collectionName);
            
        }
        public async Task AddAsync(T entity)
        {
            var document = entity.ToBsonDocument();
            await _collection.InsertOneAsync(document);
        }

        public async Task DeleteAsync(T entity)
        {
            var entityBson = entity.ToBsonDocument();
            var found = await _collection.FindAsync(entityBson);
            if (found is null)
            {
                throw new KeyNotFoundException($"{typeof(IMongoObject)}");
            }
            await _collection.DeleteOneAsync(entityBson);
        }

        public async Task<bool> Exists(string key, object value)
        {
            var entity = await GetAsync(key, value);
            return entity != null;
        }

        public async Task<bool> Exists(T entity)
        {
            var result = await GetAsync(entity);
            return result != null;
        }

        public async Task<List<T>> GetAllAsync()
        {
            var records = await _collection.FindAsync(new BsonDocument());
            var results = new List<T>();

            foreach (var document in records.ToList())
            {
                results.Add(BsonSerializer.Deserialize<T>(document));
            }

            return results;
        }

        public async Task<List<T>> GetAllAsync(BsonDocument query)
        {
            var records = await _collection.FindAsync(query);
            var results = new List<T>();

            foreach (var document in records.ToList())
            {
                results.Add(BsonSerializer.Deserialize<T>(document));
            }

            return results;
        }

        public async Task<List<T>> GetAllAsync(string key, object value)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(key, value);
            var records = await _collection.FindAsync(filter);
            var results = new List<T>();

            foreach (var document in records.ToList())
            {
                results.Add(BsonSerializer.Deserialize<T>(document));
            }

            return results;
        }

        public async Task<T> GetAsync(T entity)
        {
            var records = await _collection.FindAsync(entity.ToBsonDocument());
            var result = records.FirstOrDefault();

            if (result is null)
            {
                throw new MongoNotFoundException(nameof(IMongoObject), entity);
            }

            return BsonSerializer.Deserialize<T>(result);
        }

        public async Task<T> GetAsync(BsonDocument query)
        {
            var records = await _collection.FindAsync(query);
            var result = records.FirstOrDefault();

            if (result is null)
            {
                throw new MongoNotFoundException(nameof(IMongoObject), query);
            }

            return BsonSerializer.Deserialize<T>(result);
        }

        public async Task<T> GetAsync(string key, object value)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(key, value);
            var records = await _collection.FindAsync(filter);

            var result = records.FirstOrDefault();
            if (result is null)
            {
                throw new MongoNotFoundException(key, value);
            }

            return BsonSerializer.Deserialize<T>(result);
        }

        public async Task UpdateAsync(string key, string value, T source)
        {
            var entity = GetAsync(key, value);
            if (entity is null)
            {
                throw new KeyNotFoundException($"{key}: {value}");
            }
            var filter = Builders<BsonDocument>.Filter.Eq(key, value);
            await _collection.ReplaceOneAsync(filter, source.ToBsonDocument());
        }
        public async Task UpdateAsync(T source)
        {
            var entity = GetAsync("_id", source.Id);
            if (entity is null)
            {
                throw new MongoNotFoundException(nameof(T), source);
            }
            var filter = Builders<BsonDocument>.Filter.Eq("_id", source.Id);
            await _collection.ReplaceOneAsync(filter, source.ToBsonDocument());
        }
    }
}
