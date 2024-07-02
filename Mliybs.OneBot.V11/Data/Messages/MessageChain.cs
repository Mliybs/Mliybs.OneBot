using Mliybs.OneBot.V11.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Mliybs.OneBot.V11.Data.Messages
{
    [JsonConverter(typeof(MessageChainConverter))]
    public class MessageChain : List<MessageBase>
    {
        private readonly Lazy<string> cqCode;

        private readonly Lazy<string> allText;

        public MessageChain()
        {
            cqCode = new(() =>
            {
                var builder = new StringBuilder();
                ForEach(x => x.GetCQCode(builder));
                return builder.ToString();
            });

            allText = new(() =>
            {
                var builder = new StringBuilder();
                ForEach(x =>
                {
                    if (x is TextMessage text) builder.Append(text);
                });
                return builder.ToString();
            });
        }
#nullable disable
        public MessageChain(IEnumerable<MessageBase> messages) : base(messages)
        {
            cqCode = new(() =>
            {
                var builder = new StringBuilder();
                ForEach(x => x.GetCQCode(builder));
                return builder.ToString();
            });

            allText = new(() =>
            {
                var builder = new StringBuilder();
                ForEach(x =>
                {
                    if (x is TextMessage text) builder.Append(text);
                });
                return builder.ToString();
            });
        }
#nullable restore
        public ReplyMessage? Reply => Find(x => x is ReplyMessage) as ReplyMessage;

        public string CQCode => cqCode.Value;

        public string AllText => allText.Value;

        public string? Text => this is [TextMessage text] ? text : null;

        public bool NoReplyCompare(string text) => NoReply() == text;

        public MessageChain NoReply() => NoReply(out var _);

        public MessageChain NoReply(out bool changed)
        {
            switch (this)
            {
                case [ReplyMessage, AtMessage, var message, ..]:
                    changed = true;
                    return
                    [
                        message is TextMessage text && text.Data.Text.StartsWith(' ')
                            ? (TextMessage)text.Data.Text[1..]
                            : message,
                        ..this.Skip(3)
                    ];
                
                case [ReplyMessage, var message, ..]:
                    changed = true;
                    return [message, ..this.Skip(2)];
                
                case [AtMessage, var message, ..]:
                    changed = true;
                    return
                    [
                        message is TextMessage _text && _text.Data.Text.StartsWith(' ')
                            ? (TextMessage)_text.Data.Text[1..]
                            : message,
                        ..this.Skip(2)
                    ];

                default:
                    changed = false;
                    return new();
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
