using Confluent.Kafka;
using Persistence.Models;
using Persistence.Mongo;

namespace MvcConsumer.Workers
{
    public sealed class KafkaWorker : BackgroundService
    {
        private readonly ILogger<KafkaWorker> _logger;
        private readonly IConfiguration _configuration;
        private readonly List<string> _topics;
        private IMongoRepository<Topic> _repository;

        public KafkaWorker(ILogger<KafkaWorker> logger, IConfiguration configuration, IMongoRepository<Topic> repository)
        {
            _logger = logger;
            _configuration = configuration;
            _repository = repository;
            _topics = new List<string>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = _configuration.GetSection("Kafka").GetChildren();

            var kafkaSettings = new List<KeyValuePair<string, string>>();

            config.ToList().ForEach(x => kafkaSettings.Add(new KeyValuePair<string, string>(x.Key, x.Value)));

            using (var consumer = new ConsumerBuilder<string, string>(kafkaSettings).Build())
            {
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var topics = _repository.All();
                        var newTopics = new List<string>();
                        foreach (Topic topic in topics)
                        {
                            if (!_topics.Contains(topic.Name))
                            {
                                _topics.Add(topic.Name);
                                newTopics.Add(topic.Name);
                            }
                        }

                        if (newTopics.Any())
                        {
                            consumer.Subscribe(_topics);
                        }

                        var cr = consumer.Consume(1000);
                        if (cr != null)
                        {
                            Topic topic = await _repository.FindOneAsync(x => x.Name == cr.Topic);
                            if (topic != null)
                            {
                                topic.Messages.Add(new Message()
                                {
                                    User = cr.Message.Key,
                                    Text = cr.Message.Value
                                });

                                await _repository.ReplaceOneAsync(topic);
                            }
                            _logger.Log(LogLevel.Information, $"Consumed event from topic {cr.Topic} with key {cr.Message.Key,-10} and value {cr.Message.Value}");
                        }
                    }
                }

                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                finally
                {
                    consumer.Close();
                }
            }
        }
    }
}
