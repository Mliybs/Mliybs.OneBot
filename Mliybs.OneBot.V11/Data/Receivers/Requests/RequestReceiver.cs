using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mliybs.OneBot.V11.Data.Receivers.Requests
{
#nullable disable
    public abstract class RequestReceiver : ReceiverBase
    {
        [JsonPropertyName("request_type"), JsonConverter(typeof(JsonStringEnumConverter))]
        public RequestType RequestType { get; set; }
    }

    public enum RequestType
    {
        Friend,
        Group
    }
}
