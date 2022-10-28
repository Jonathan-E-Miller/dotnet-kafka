using ApiProducer.Interfaces;
using ApiProducer.Models;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Persistence.Models;
using Persistence.Mongo;

namespace ApiProducer
{
    public class KafkaContainer : IKafkaContainer
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IMongoRepository<Topic> _repository;

        public KafkaContainer(ILogger<KafkaContainer> logger, IConfiguration configuration, IMongoRepository<Topic> repository)
        {
            _logger = logger;
            _configuration = configuration;
            _repository = repository;
        }

        public async Task CreateTopic(string topicName)
        {

            string bootstrapServer = _configuration.GetSection("Kafka")["bootstrap.servers"];
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServer }).Build())
            {
                try
                {
                    await adminClient.CreateTopicsAsync(new TopicSpecification[] { new TopicSpecification { Name = topicName, ReplicationFactor = 1, NumPartitions = 1 } });
                }
                catch (CreateTopicsException e)
                {
                    Console.WriteLine($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
                }
            }
        }

        public async Task ProduceMessage(KafkaMessageRequest request)
        {
            var topic = await _repository.FindOneAsync(x => x.Name == request.Topic);
            if (topic == null)
            {
                throw new UnknownTopicException($"Topic {request.Topic} has not yet been created");
            }

            var kafkaConfiguration = _configuration.GetSection("Kafka").GetChildren();
            var config = new List<KeyValuePair<string, string>>();

            kafkaConfiguration.ToList().ForEach(x => config.Add(new KeyValuePair<string, string>(x.Key, x.Value)));
            using (var producer = new ProducerBuilder<string, string>(config).Build())
            {
                await producer.ProduceAsync(request.Topic, new Message<string, string> { Key = request.User, Value = request.Message });
            }
        }
    }
}
