using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json.Nodes;

namespace newPostsFeed.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContentController : ControllerBase
    {
        private readonly IDatabase _redisDatabase;
        public ContentController(IConfiguration configuration)
        {
            Console.WriteLine(configuration["REDIS_URI"]);
            var redis = ConnectionMultiplexer.Connect(configuration["REDIS_URI"]);
            _redisDatabase = redis.GetDatabase();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var hashEntriesMessages = _redisDatabase.HashGetAll("Message");

            var jsonArray = new JsonArray();
            foreach (var obj in hashEntriesMessages)
            {
                var objectid = obj.Name.ToString();
                var message = obj.Value.ToString();
                var name = _redisDatabase.HashGet("Name", objectid);

                var jsonObject = new JsonObject();
                jsonObject.Add("ObjectID", objectid);
                jsonObject.Add("message", message);
                jsonObject.Add("name", name.ToString());

                jsonArray.Add(jsonObject);
            }

            return Ok(jsonArray.ToString());
        }
    }
}
