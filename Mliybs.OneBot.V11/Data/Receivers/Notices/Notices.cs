using Mliybs.OneBot.V11.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mliybs.OneBot.V11.Data.Receivers.Notices
{
#nullable disable
    [CustomTypeIdentifier("group_upload")]
    public class GroupUploadNoticeReceiver : NoticeReceiver
    {
        public override string NoticeType => "group_upload";

        [JsonPropertyName("group_id")]
        public long GroupId { get; set; }

        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("file")]
        public FileData File { get; set; }

        public class FileData
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("size")]
            public long Size { get; set; }

            [JsonPropertyName("busid")]
            public long Busid { get; set; }
        }
    }

    [CustomTypeIdentifier("group_admin")]
    public class GroupAdminNoticeReceiver : NoticeReceiver
    {
        public override string NoticeType => "group_admin";

        [JsonPropertyName("sub_type"), JsonConverter(typeof(JsonStringEnumConverter))]
        public GroupAdminType SubType { get; set; }

        [JsonPropertyName("group_id")]
        public long GroupId { get; set; }

        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        public enum GroupAdminType
        {
            Set,
            Unset
        }
    }

    [CustomTypeIdentifier("group_decrease")]
    public class GroupDecreaseNoticeReceiver : NoticeReceiver
    {
        public override string NoticeType => "group_decrease";

        [JsonPropertyName("sub_type"), JsonConverter(typeof(JsonStringEnumConverter))]
        public GroupDecreaseType SubType { get; set; }

        [JsonPropertyName("group_id")]
        public long GroupId { get; set; }

        [JsonPropertyName("operator_id")]
        public long OperatorId { get; set; }

        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        public enum GroupDecreaseType
        {
            Leave,
            Kick,
            Kick_Me
        }
    }

    [CustomTypeIdentifier("group_increase")]
    public class GroupIncreaseNoticeReceiver : NoticeReceiver
    {
        public override string NoticeType => "group_increase";

        [JsonPropertyName("sub_type"), JsonConverter(typeof(JsonStringEnumConverter))]
        public GroupIncreaseType SubType { get; set; }

        [JsonPropertyName("group_id")]
        public long GroupId { get; set; }

        [JsonPropertyName("operator_id")]
        public long OperatorId { get; set; }

        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        public enum GroupIncreaseType
        {
            Approve,
            Invite
        }
    }

    [CustomTypeIdentifier("group_ban")]
    public class GroupBanNoticeReceiver : NoticeReceiver
    {
        public override string NoticeType => "group_ban";

        [JsonPropertyName("sub_type"), JsonConverter(typeof(JsonStringEnumConverter))]
        public GroupBanType SubType { get; set; }

        [JsonPropertyName("group_id")]
        public long GroupId { get; set; }

        [JsonPropertyName("operator_id")]
        public long OperatorId { get; set; }

        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        public enum GroupBanType
        {
            Ban,
            Lift_Ban
        }
    }

    [CustomTypeIdentifier("friend_add")]
    public class FirendAddNoticeReceiver : NoticeReceiver
    {
        public override string NoticeType => "friend_add";

        [JsonPropertyName("user_id")]
        public long UserId { get; set; }
    }

    [CustomTypeIdentifier("group_recall")]
    public class GroupRecallNoticeReceiver : NoticeReceiver
    {
        public override string NoticeType => "group_recall";

        [JsonPropertyName("group_id")]
        public long GroupId { get; set; }

        [JsonPropertyName("operator_id")]
        public long OperatorId { get; set; }

        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("message_id")]
        public long MessageId { get; set; }
    }

    [CustomTypeIdentifier("friend_recall")]
    public class FriendRecallNoticeReceiver : NoticeReceiver
    {
        public override string NoticeType => "friend_recall";

        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("message_id")]
        public long MessageId { get; set; }
    }

    [CustomTypeIdentifier("notify")]
    public class NotifyNoticeReceiver : NoticeReceiver
    {
        public override string NoticeType => "notify";

        [JsonPropertyName("sub_type"), JsonConverter(typeof(JsonStringEnumConverter))]
        public NotifyType SubType { get; set; }

        [JsonPropertyName("group_id")]
        public long GroupId { get; set; }

        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

#nullable enable
        [JsonPropertyName("target_id")]
        public long? TargetId { get; set; }

        [JsonPropertyName("honor_type"), JsonConverter(typeof(JsonStringEnumConverter))]
        public HonorDataType HonorType { get; set; }
#nullable disable

        public enum NotifyType
        {
            Poke,
            Lucky_King,
            Honor
        }
        
        public enum HonorDataType
        {
            Talkative,
            Performer,
            Emotion
        }
    }
}
