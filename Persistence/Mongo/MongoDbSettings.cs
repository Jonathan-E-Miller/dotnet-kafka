namespace Persistence.Mongo
{
    public sealed class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string TopicsCollectionName { get; set; }
    }
}
