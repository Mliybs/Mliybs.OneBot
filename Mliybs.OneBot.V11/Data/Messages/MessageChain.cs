using Mliybs.OneBot.V11.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mliybs.OneBot.V11.Data.Messages
{
    [JsonConverter(typeof(MessageChainConverter))]
    public class MessageChain : List<MessageBase>
    {
        public MessageChain() { }
        public MessageChain(IEnumerable<MessageBase> messages) : base(messages) { }

        public static implicit operator MessageChain(string text) => new()
        {
            new TextMessage()
            {
                Data = new()
                {
                    Text = text
                }
            }
        };
    }
}
