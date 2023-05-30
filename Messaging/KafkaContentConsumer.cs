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
    public class KafkaContentConsumer : IHostedService
    {
        private readonly ILogger<KafkaContentConsumer> _logger;
        private ClusterClient _cluster;
        private readonly IDatabase _redisDatabase;
        private string kafkauri;


        public KafkaContentConsumer(ILogger<KafkaContentConsumer> logger, IConfiguration configuration)
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
            _cluster.ConsumeFromLatest("NEW_CONTENT");

            _logger.LogInformation("SUBSCRIBED TO NEW_CONTENT");
            _cluster.MessageReceived += record =>
            {
                _logger.LogInformation(Encoding.UTF8.GetString(record.Value as byte[]));
                var json = JsonObject.Parse(Encoding.UTF8.GetString(record.Value as byte[]));
                var id = (string)json["Id"];
                var name = (string)json["Name"];
                var message = (string)json["Message"];
                var createdAt = (string)json["CreatedAt"];

                _redisDatabase.HashSet("Message", new[]
                { new HashEntry(id, message), });
                _redisDatabase.HashSet("Name", new[]
                { new HashEntry(id, name), });
                _redisDatabase.HashSet("CreatedAt", new[]
                { new HashEntry(id, createdAt), });

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