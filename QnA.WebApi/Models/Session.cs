using System;
using Newtonsoft.Json;

namespace QnA.WebApi.Models
{
    [JsonObject]
    public class Session
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("start")]
        public DateTime StartTime { get; set; }
        [JsonProperty("end")]
        public DateTime EndTime { get; set; }
    }
}