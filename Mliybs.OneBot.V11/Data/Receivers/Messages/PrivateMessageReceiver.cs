using Mliybs.OneBot.V11.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mliybs.OneBot.V11.Data.Receivers.Messages
{
#nullable disable
    [CustomTypeIdentifier("private")]
    public class PrivateMessageReceiver : MessageReceiver
    {
        [JsonPropertyName("sub_type"), JsonConverter(typeof(JsonStringEnumConverter))]
        public PrivateType SubType { get; set; }

        [JsonPropertyName("sender")]
        public Sender Sender { get; set; }
    }

    public enum PrivateType
    {
        Friend,
        Group,
        Other
    }
}
