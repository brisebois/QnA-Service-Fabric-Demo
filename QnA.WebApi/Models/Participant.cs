using Newtonsoft.Json;

namespace QnA.WebApi.Models
{
    [JsonObject]
    public class Participant
    {
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}