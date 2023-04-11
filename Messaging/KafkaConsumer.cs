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
    public class KafkaConsumer : IHostedService
    {
        private readonly ILogger<KafkaConsumer> _logger;
        private ClusterClient _cluster;
        private readonly IDatabase _redisDatabase;


        public KafkaConsumer(ILogger<KafkaConsumer> logger)
        {
            _logger = logger;

            var redis = ConnectionMultiplexer.Connect("localhost:6379");
            _redisDatabase = redis.GetDatabase();

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
                var json = JsonObject.Parse(Encoding.UTF8.GetString(record.Value as byte[]));
                var id = (string)json["Id"];
                var name = (string)json["Name"];
                var message = (string)json["Message"];
                var createdAt = (string)json["CreatedAt"];

                _logger.LogInformation(createdAt.ToString());

                _redisDatabase.HashSet("Message", new[]
                {
                    new HashEntry(id, message),
                });
                _redisDatabase.HashSet("Name", new[]
{
                    new HashEntry(id, name),
                });
                _redisDatabase.HashSet("CreatedAt", new[]
{
                    new HashEntry(id, createdAt),
                });

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