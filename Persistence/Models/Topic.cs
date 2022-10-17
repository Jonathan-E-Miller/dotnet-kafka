using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Persistence.Mongo;

namespace Persistence.Models
{
    [BsonCollection("topics")]
    public class Topic : Document
    {
        public string Name { get; set; }
        public List<Message> Messages { get; set; }
    }
}
