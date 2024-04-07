using Mliybs.OneBot.V11.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mliybs.OneBot.V11.Data.Receivers.Requests
{
#nullable disable
    [CustomTypeIdentifier("friend")]
    public class FriendRequestReceiver : RequestReceiver
    {
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("flag")]
        public string Flag { get; set; }
    }

    [CustomTypeIdentifier("group")]
    public class GroupRequestReceiver : RequestReceiver
    {
        [JsonPropertyName("sub_type"), JsonConverter(typeof(JsonStringEnumConverter))]
        public GroupRequestType SubType { get; set; }

        [JsonPropertyName("group_id")]
        public long GroupId { get; set; }

        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("flag")]
        public string Flag { get; set; }

        public enum GroupRequestType
        {
            Add,
            Invite
        }
    }
}
