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
        private readonly IMongoRepository<Topic> _repository;
        private readonly IAdminClient _adminClient;
        private readonly IProducer<string, string> _producer;

        public KafkaContainer(
            ILogger<KafkaContainer> logger,
            IMongoRepository<Topic> repository,
            IProducer<string, string> producer,
            IAdminClient adminClient)
        {
            _logger = logger;
            _repository = repository;
            _producer = producer;
            _adminClient = adminClient;
        }

        public async Task CreateTopic(string topicName)
        {
            _logger.LogInformation($"Creating topic {topicName}");
            try
            {
                await _adminClient.CreateTopicsAsync(new TopicSpecification[] { new TopicSpecification { Name = topicName, ReplicationFactor = 1, NumPartitions = 1 } });
            }
            catch (CreateTopicsException e)
            {
                Console.WriteLine($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
            }
        }

        public async Task ProduceMessage(KafkaMessageRequest request)
        {
            _logger.LogInformation($"Producing Message {request.Message} for topic {request.Topic}");
            var topic = await _repository.FindOneAsync(x => x.Name == request.Topic);
            if (topic == null)
            {
                throw new UnknownTopicException($"Topic {request.Topic} has not yet been created");
            }

            await _producer.ProduceAsync(request.Topic, new Message<string, string> { Key = request.User, Value = request.Message });
        }
    }
}
