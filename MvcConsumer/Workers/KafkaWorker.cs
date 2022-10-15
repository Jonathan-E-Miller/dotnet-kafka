using Confluent.Kafka;

namespace MvcConsumer.Workers
{
    public sealed class KafkaWorker : BackgroundService
    {
        private readonly ILogger<KafkaWorker> _logger;
        private readonly IConfiguration _configuration;
        private const string BOOTSTRAP_SERVER = "bootstrap.servers";
        private const string GROUP_ID = "group.id";
        private const string AUTO_OFFSET_RESET = "auto.offset.reset";

        public KafkaWorker(ILogger<KafkaWorker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = _configuration.GetSection("Kafka").GetChildren();

            var kafkaSettings = new List<KeyValuePair<string, string>>();

            config.ToList().ForEach(x => kafkaSettings.Add(new KeyValuePair<string, string>(x.Key, x.Value)));
            using (var consumer = new ConsumerBuilder<string, string>(kafkaSettings).Build())
            {
                consumer.Subscribe("users");
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var cr = consumer.Consume(1000);
                        if (cr != null)
                        {
                            Console.WriteLine($"Consumed event from topic 'Users' with key {cr.Message.Key,-10} and value {cr.Message.Value}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ctrl-C was pressed.
                }
                finally
                {
                    consumer.Close();
                }
            }

            return Task.CompletedTask;
        }
    }
}
