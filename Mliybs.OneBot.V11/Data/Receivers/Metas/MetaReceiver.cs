using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mliybs.OneBot.V11.Data.Receivers.Metas
{
#nullable disable
    public abstract class MetaReceiver : ReceiverBase
    {
        [JsonPropertyName("meta_event_type"), JsonConverter(typeof(JsonStringEnumConverter))]
        public MetaEventType MetaEventType { get; set; }
    }

    public enum MetaEventType
    {
        Lifecycle,
        Heartbeat
    }
}
