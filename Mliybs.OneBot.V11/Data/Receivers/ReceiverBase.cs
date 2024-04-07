using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

#nullable disable
namespace Mliybs.OneBot.V11.Data.Receivers
{
    public abstract class ReceiverBase
    {
        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("self_id")]
        public long SelfId { get; set; }

        [JsonPropertyName("post_type")]
        public string PostType { get; set; }
    }
}
