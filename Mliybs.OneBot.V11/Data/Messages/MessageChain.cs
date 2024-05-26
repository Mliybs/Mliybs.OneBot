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

        public ReplyMessage? Reply => Find(x => x is ReplyMessage) as ReplyMessage;

        public string GetCQCode()
        {
            var builder = new StringBuilder();
            ForEach(x => x.GetCQCode(builder));
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

        public bool NoReplyCompare(string text) => NoReply() == text;

        public MessageChain NoReply() => NoReply(out var _);

        public MessageChain NoReply(out bool changed)
        {
            switch (this)
            {
                case [ReplyMessage, AtMessage, TextMessage text, ..]:
                    if (text.Data.Text.StartsWith(' ')) text.Data.Text = text.Data.Text[1..];
                    RemoveRange(0, 2);
                    changed = true;
                    return this;
                
                case [ReplyMessage, TextMessage, ..]:
                    RemoveAt(0);
                    changed = true;
                    return this;
                
                case [AtMessage, TextMessage text, ..]:
                    if (text.Data.Text.StartsWith(' ')) text.Data.Text = text.Data.Text[1..];
                    RemoveAt(0);
                    changed = true;
                    return this;

                default:
                    changed = false;
                    return this;
            }
        }

        public new MessageChain AddRange(IEnumerable<MessageBase> items)
        {
            foreach (var item in items) Add(item);
            return this;
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

        public static MessageChain operator +(MessageChain left, MessageChain right) =>
            left.AddRange(right);

        public static bool operator ==(MessageChain? chain, string text)
        {
            if (chain is not null and [TextMessage message]) return message == text;
            return false;
        }

        public static bool operator !=(MessageChain? chain, string text)
        {
            if (chain is not null and [TextMessage message]) return message != text;
            return true;
        }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
