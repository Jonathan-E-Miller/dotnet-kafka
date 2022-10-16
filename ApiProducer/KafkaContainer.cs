using ApiProducer.Interfaces;
using ApiProducer.Models;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace ApiProducer
{
    public class KafkaContainer : IKafkaContainer
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public KafkaContainer(ILogger<KafkaContainer> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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

        public void ProduceMessage(KafkaMessageRequest request)
        {
            var kafkaConfiguration = _configuration.GetSection("Kafka").GetChildren();
            var config = new List<KeyValuePair<string, string>>();

            kafkaConfiguration.ToList().ForEach(x => config.Add(new KeyValuePair<string, string>(x.Key, x.Value)));
            using (var producer = new ProducerBuilder<string, string>(config).Build())
            {
                producer.Produce(request.Topic, new Message<string, string> { Key = request.User, Value = request.Message }, (deliveryReport) =>
                {
                    if (deliveryReport.Error.Code != ErrorCode.NoError)
                    {
                        _logger.LogError("Failed to send message to broker");
                    }
                    else
                    {
                        _logger.LogInformation($"Produced event to topic {request.Topic}: key = {request.User,-10} value = {request.Message}");
                    }
                });

            }
        }
    }
}
