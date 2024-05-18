using Mliybs.OneBot.V11.Data.Messages;
using Mliybs.OneBot.V11.Data.Receivers.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mliybs.OneBot.V11.Data
{
#nullable disable
    public class Message
    {
        [JsonPropertyName("message_id")]
        public int MessageId { get; set; }
    }

    public class DetailedMessage
    {
        [JsonPropertyName("time")]
        public int Time { get; set; }

        [JsonPropertyName("message_type")]
        public string MessageType { get; set; }

        [JsonPropertyName("message_id")]
        public int MessageId { get; set; }

        [JsonPropertyName("real_id")]
        public int RealId { get; set; }

        [JsonPropertyName("sender")]
        public Sender Sender { get; set; }

        [JsonPropertyName("message"), JsonIgnore]
        public MessageChain Message { get; set; }
    }

    public class NodeMessages
    {
        [JsonPropertyName("message")]
        public MessageChain Message { get; set; }
    }

    public class LoginInfo
    {
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }
    }

    public class StrangerInfo
    {
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("sex"), JsonConverter(typeof(JsonStringEnumConverter))]
        public Sex Sex { get; set; }

        [JsonPropertyName("age")]
        public int Age { get; set; }
    }

    public class FriendInfo
    {
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("remark")]
        public string Remark { get; set; }
    }

    public class GroupInfo
    {
        [JsonPropertyName("group_id")]
        public long GroupId { get; set; }

        [JsonPropertyName("group_name")]
        public string GroupName { get; set; }

        [JsonPropertyName("member_count")]
        public int MemberCount { get; set; }

        [JsonPropertyName("max_member_count")]
        public int MaxMemberCount { get; set; }
    }

    public class GroupMemberInfo
    {
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("card")]
        public string Card { get; set; }

        [JsonPropertyName("sex"), JsonConverter(typeof(JsonStringEnumConverter))]
        public Sex Sex { get; set; }

        [JsonPropertyName("age")]
        public int Age { get; set; }

        [JsonPropertyName("area")]
        public string Area { get; set; }

        [JsonPropertyName("join_time")]
        public int JoinTime { get; set; }

        [JsonPropertyName("last_sent_time")]
        public int LastSentTime { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("role"), JsonConverter(typeof(JsonStringEnumConverter))]
        public Role Role { get; set; }

        [JsonPropertyName("unfriendly")]
        public bool Unfriendly { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("title_expire_time")]
        public int TitleExpireTime { get; set; }

        [JsonPropertyName("card_changable")]
        public bool CardChangable { get; set; }
    }

    public class TalkativeInfo
    {
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        [JsonPropertyName("day_count")]
        public int DayCount { get; set; }
    }

    public class DefaultHonorInfo
    {
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    public class HonorInfos
    {
        [JsonPropertyName("group_id")]
        public long GroupId { get; set; }

        [JsonPropertyName("current_talkative")]
        public TalkativeInfo CurrentTalkative { get; set; }

        [JsonPropertyName("talkative_list")]
        public TalkativeInfo[] TalkativeList { get; set; }

        [JsonPropertyName("performer_list")]
        public DefaultHonorInfo[] PerformerList { get; set; }

        [JsonPropertyName("legend_list")]
        public DefaultHonorInfo[] LegendList { get; set; }

        [JsonPropertyName("strong_newbie_list")]
        public DefaultHonorInfo[] StrongNewbieList { get; set; }

        [JsonPropertyName("emotion_list")]
        public DefaultHonorInfo[] EmotionList { get; set; }
    }

    public class CookiesInfo
    {
        [JsonPropertyName("cookies")]
        public string Cookies { get; set; }
    }

    public class CsrfInfo
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }

    public class Credentials
    {
        [JsonPropertyName("cookies")]
        public string Cookies { get; set; }

        [JsonPropertyName("csrf_token")]
        public string CsrfToken { get; set; }
    }

    public class FileInfo
    {
        [JsonPropertyName("file")]
        public string File { get; set; }
    }

    public class BooleanInfo
    {
        [JsonPropertyName("yes")]
        public bool Yes { get; set; }

        public static implicit operator bool(BooleanInfo info) => info.Yes;
    }

    public class StatusInfo
    {
        [JsonPropertyName("online")]
        public bool Online { get; set; }

        [JsonPropertyName("good")]
        public bool Good { get; set; }
    }

    public class VersionInfo
    {
        [JsonPropertyName("app_name")]
        public string AppName { get; set; }

        [JsonPropertyName("app_version")]
        public string AppVersion { get; set; }

        [JsonPropertyName("protocol_version")]
        public string ProtocolVersion { get; set; }
    }
}
