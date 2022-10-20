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
            logger.LogInformation("Constructing");
            _logger = logger;
            _configuration = configuration;
            _repository = repository;
            _topics = new List<string>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting background");
            var config = _configuration.GetSection("Kafka").GetChildren();

            var kafkaSettings = new List<KeyValuePair<string, string>>();

            config.ToList().ForEach(x => kafkaSettings.Add(new KeyValuePair<string, string>(x.Key, x.Value)));

            _logger.LogInformation("Settings Consumed");

            await Task.Run(() => WorkerMethodAsync(stoppingToken, kafkaSettings));

            _logger.LogInformation("Kafka Consumer has stopped");
        }

        private async void WorkerMethodAsync(CancellationToken stoppingToken, List<KeyValuePair<string, string>> kafkaSettings)
        {
            try
            {
                using (var consumer = new ConsumerBuilder<string, string>(kafkaSettings).Build())
                {
                    _logger.LogInformation("Inside using");
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
                                _logger.LogInformation($"Subscibing to {_topics.Count} topics");
                                consumer.Subscribe(_topics);
                            }

                            ConsumeResult<string, string> cr = consumer.Consume(1000);
                            if (cr != null)
                            {
                                Topic topic = await _repository.FindOneAsync(x => x.Name == cr.Topic);
                                if (topic != null)
                                {
                                    topic.Messages.Add(new Message()
                                    {
                                        User = cr.Message.Key,
                                        Text = cr.Message.Value,
                                        ReceivedAt = DateTime.Now
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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

    }
}
