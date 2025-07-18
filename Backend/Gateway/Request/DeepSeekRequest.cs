using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;



namespace Gateway.Request
{
    public class DeepSeekRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = "deepseek-chat";

        [JsonProperty("messages")]
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }

    public class ChatMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; } // "user", "system", "assistant"

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
