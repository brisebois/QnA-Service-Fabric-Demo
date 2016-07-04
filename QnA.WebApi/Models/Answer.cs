using Newtonsoft.Json;

namespace QnA.WebApi.Models
{
    [JsonObject]
    public class Answer
    {
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}