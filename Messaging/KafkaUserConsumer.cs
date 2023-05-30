using Confluent.Kafka;
using System.Text;
using System.Text.Unicode;
using Kafka.Public;
using Kafka.Public.Loggers;
using StackExchange.Redis;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Nodes;

namespace newPostsFeed
{
    public class KafkaUserConsumer : IHostedService
    {
        private readonly ILogger<KafkaUserConsumer> _logger;
        private ClusterClient _cluster;
        private readonly IDatabase _redisDatabase;
        private string kafkauri;


        public KafkaUserConsumer(ILogger<KafkaUserConsumer> logger, IConfiguration configuration)
        {
            _logger = logger;

            var redis = ConnectionMultiplexer.Connect(configuration["REDIS_URI"]);
            _redisDatabase = redis.GetDatabase();

            _cluster = new ClusterClient(new Configuration
            {
                Seeds = configuration["KAFKA_URI"],
            }, new ConsoleLogger());
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cluster.ConsumeFromLatest("DELETE_USER");

            _logger.LogInformation("SUBSCRIBED TO DELETE_USER");
            _cluster.MessageReceived += record =>
            {
                _logger.LogInformation(Encoding.UTF8.GetString(record.Value as byte[]));
                var json = JsonObject.Parse(Encoding.UTF8.GetString(record.Value as byte[]));
                var name = (string)json["Name"];

                var keysToDelete = _redisDatabase.HashKeys("Name").Where(key => _redisDatabase.HashGet("Name", key).ToString() == name).ToArray();

                _redisDatabase.HashDelete("Message", keysToDelete);
                _redisDatabase.HashDelete("Name", keysToDelete);
                _redisDatabase.HashDelete("CreatedAt", keysToDelete);

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