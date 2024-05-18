using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Mliybs.OneBot.V11.Data.Messages;
using Mliybs.OneBot.V11.Data.Receivers;

#nullable disable
namespace Mliybs.OneBot.V11.Data.Receivers.Messages
{
    public abstract class MessageReceiver : ReceiverBase
    {
        [JsonPropertyName("message_type"), JsonConverter(typeof(JsonStringEnumConverter))]
        public OneBotMessageType MessageType { get; set; }

        [JsonPropertyName("message_id")]
        public int MessageId { get; set; }

        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("message"), JsonIgnore]
        public MessageChain Message { get; set; }

        [JsonPropertyName("raw_message")]
        public string RawMessage { get; set; }

        [JsonPropertyName("font")]
        public int Font { get; set; }

        public MessageQuickOperator With(OneBot bot) => new(this, bot);
    }

    public interface ISender<T> where T : Sender
    {
        public T Sender { get; set; }
    }

#nullable enable
    public class Sender
    {
        [JsonPropertyName("user_id")]
        public long? UserId { get; set; }

        [JsonPropertyName("nickname")]
        public string? Nickname { get; set; }

        [JsonPropertyName("sex"), JsonConverter(typeof(JsonStringEnumConverter))]
        public Sex? Sex { get; set; }

        [JsonPropertyName("age")]
        public int? Age { get; set; }
    }

    public enum Sex
    {
        Male,
        Female,
        Unknown
    }
}
