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

        public ReplyMessage? BeReplied => Find(x => x is ReplyMessage) as ReplyMessage;

        public string GetCQCode()
        {
            var builder = new StringBuilder();
            ForEach(x => builder.Append(x.GetCQCode()));
            return builder.ToString();
        }

        public string GetPlainText()
        {
            var builder = new StringBuilder();
            ForEach(x =>
            {
                if (x is TextMessage text) builder.Append(text);
            });
            return builder.ToString();
        }

        public MessageChain NoReplyAtFirst()
        {
            switch (this)
            {
                case [ReplyMessage, AtMessage, TextMessage text, ..]:
                    if (text.Data.Text.StartsWith(' ')) text.Data.Text = text.Data.Text[1..];
                    RemoveRange(0, 2);
                    return this;
                
                case [AtMessage, TextMessage text, ..]:
                    if (text.Data.Text.StartsWith(' ')) text.Data.Text = text.Data.Text[1..];
                    RemoveAt(0);
                    return this;

                default: return this;
            }
        }

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
