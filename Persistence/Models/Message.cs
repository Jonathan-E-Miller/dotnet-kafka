using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace Persistence.Models
{
    public sealed class Message
    {
        public string User { get; set; }
        public string Text { get; set; }
    }
}
