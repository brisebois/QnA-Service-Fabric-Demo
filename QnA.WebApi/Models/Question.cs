using Newtonsoft.Json;

namespace QnA.WebApi.Models
{
    [JsonObject]
    public class Question
    {
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}