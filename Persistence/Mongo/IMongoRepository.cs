using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Mongo
{
    public interface IMongoRepository<T> where T : IDocument
    {
        IQueryable<T> All();
        Task InsertOneAsync(T entity);
        Task<IEnumerable<T>> FilterBy(Expression<Func<T, bool>> filterExpression);
        Task<T> FindOneAsync(Expression<Func<T, bool>> filterExpression);
        Task ReplaceOneAsync(T document);
    }
}
