using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mliybs.OneBot.V11.Data.Messages
{
    public abstract class MessageBase
    {
        [JsonPropertyName("type")]
        public abstract string Type { get; }
    }
}
