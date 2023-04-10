using Confluent.Kafka;
using System.Text;
using System.Text.Unicode;
using Kafka.Public;
using Kafka.Public.Loggers;


namespace newPostsFeed
{
    public class KafkaConsumer : IHostedService
    {
        private readonly ILogger<KafkaConsumer> _logger;
        private ClusterClient _cluster;

        public KafkaConsumer(ILogger<KafkaConsumer> logger)
        {
            _logger = logger;
            _cluster = new ClusterClient(new Configuration
            {
                Seeds = "localhost:9092"
            }, new ConsoleLogger());
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cluster.ConsumeFromLatest("NEW_CONTENT");
            _cluster.MessageReceived += record =>
            {
                _logger.LogInformation("NEW CONTENT: " + Encoding.UTF8.GetString(record.Value as byte[]));
            };
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cluster.Dispose();
            return Task.CompletedTask;
        }

    }
}