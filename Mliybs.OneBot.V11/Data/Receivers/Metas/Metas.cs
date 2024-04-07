using Mliybs.OneBot.V11.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mliybs.OneBot.V11.Data.Receivers.Metas
{
#nullable disable
    [CustomTypeIdentifier("lifecycle")]
    public class LifecycleMetaReceiver : MetaReceiver
    {
        [JsonPropertyName("sub_type"), JsonConverter(typeof(JsonStringEnumConverter))]
        public LifecycleType SubType { get; set; }

        public enum LifecycleType
        {
            Enable,
            Disable,
            Connect
        }
    }

    [CustomTypeIdentifier("heartbeat")]
    public class HeartbeatMetaReceiver : MetaReceiver
    {
        [JsonPropertyName("status")]
        public HeartbeatStatus Status { get; set; }

        [JsonPropertyName("interval")]
        public long Interval { get; set; }
    }

    public class HeartbeatStatus
    {
#nullable enable
        [JsonPropertyName("online")]
        public bool Online { get; set; }
#nullable disable

        [JsonPropertyName("good")]
        public bool Good { get; set; }
    }
}
