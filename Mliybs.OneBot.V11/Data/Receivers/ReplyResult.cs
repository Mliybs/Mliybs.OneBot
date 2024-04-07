using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mliybs.OneBot.V11.Data.Receivers
{
#nullable disable
    public class ReplyResult
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("retcode")]
        public int RetCode { get; set; }

        [JsonPropertyName("data"), JsonIgnore]
        public JsonElement Data { get; set; }

#nullable enable
        [JsonPropertyName("echo")]
        public string? Echo { get; set; }
    }
}
