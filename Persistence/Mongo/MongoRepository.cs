using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Mongo
{
    public class MongoRepository<T> : IMongoRepository<T> where T : IDocument
    {
        private readonly IMongoCollection<T> _collection;
        
        public MongoRepository(IOptions<MongoDbSettings> options)
        {
            var database = new MongoClient(options.Value.ConnectionString).GetDatabase(options.Value.DatabaseName);
            _collection = database.GetCollection<T>(GetCollectionName(typeof(T)));
            if (_collection == null)
            {
                database.CreateCollection(GetCollectionName(typeof(T)));
                _collection = database.GetCollection<T>(GetCollectionName(typeof(T)));
            }
        }

        private string? GetCollectionName(Type documentType)
        {
            BsonCollectionAttribute? attr = (BsonCollectionAttribute?)documentType.GetCustomAttributes(typeof(BsonCollectionAttribute), true).FirstOrDefault();
            return attr?.CollectionName;
        }

        public IQueryable<T> All()
        {
            return _collection.AsQueryable();
        }

        public async Task<IEnumerable<T>> FilterBy(Expression<Func<T, bool>> filterExpression)
        {
            return await _collection.Find(filterExpression).ToListAsync();
        }

        public async Task<T?> FindOneAsync(Expression<Func<T, bool>> filterExpression)
        {
            return await _collection.Find(filterExpression).FirstOrDefaultAsync();
        }

        public async Task ReplaceOneAsync(T document)
        {
            var filter = Builders<T>.Filter.Eq(doc => doc.Id, document.Id);
            await _collection.FindOneAndReplaceAsync(filter, document);
        }

        public async Task InsertOneAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public Task<T> FindById(string id)
        {
            return Task.Run(() =>
            {
                var objectId = new ObjectId(id);
                var filter = Builders<T>.Filter.Eq(doc => doc.Id, objectId);
                return _collection.Find(filter).SingleOrDefaultAsync();
            });
        }
    }
}
