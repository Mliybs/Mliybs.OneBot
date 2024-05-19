using Mliybs.OneBot.V11.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mliybs.OneBot.V11.Data.Receivers.Messages
{
    [CustomTypeIdentifier("group")]
    public class GroupMessageReceiver : MessageReceiver
    {
        [JsonPropertyName("sub_type"), JsonConverter(typeof(JsonStringEnumConverter))]
        public GroupType SubType { get; set; }

#nullable disable
        [JsonPropertyName("sender")]
        public GroupSender Sender { get; set; }
#nullable enable

        [JsonPropertyName("group_id")]
        public long GroupId { get; set; }

        [JsonPropertyName("anonymous")]
        public Anonymous? Anonymous { get; set; }
    }

    public enum GroupType
    {
        Normal,
        Anonymous,
        Notice
    }

    public class GroupSender : Sender
    {
        [JsonPropertyName("card")]
        public string? Card { get; set; }

        [JsonPropertyName("area")]
        public string? Area { get; set; }

        [JsonPropertyName("level")]
        public int? Level { get; set; }

        [JsonPropertyName("role"), JsonConverter(typeof(JsonStringEnumConverter))]
        public Role? Role { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }

    public enum Role
    {
        Owner,
        Admin,
        Member
    }

#nullable disable
    public class Anonymous
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("flag")]
        public string Flag { get; set; }
    }
}
