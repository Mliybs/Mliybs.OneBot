using Mliybs.OneBot.V11.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mliybs.OneBot.V11.Data.Messages
{
#nullable disable
    [CustomTypeIdentifier("text")]
    public class TextMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "text";

        [JsonPropertyName("data")]
        public TextData Data { get; set; }

        public class TextData
        {
            [JsonPropertyName("text")]
            public string Text { get; set; }
        }

        public override string GetCQCode() => Data.Text;
    }

    [CustomTypeIdentifier("image")]
    public class ImageMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "image";

        [JsonPropertyName("data")]
        public ImageData Data { get; set; }

#nullable enable
        public class ImageData
        {
            [JsonPropertyName("file")]
            public string? File { get; set; }

            [JsonPropertyName("type"), JsonConverter(typeof(JsonStringEnumConverter))]
            public Flash? Type { get; set; }

            [JsonPropertyName("url")]
            public string? Url { get; set; }

            [JsonPropertyName("cache")]
            public bool? Cache { get; set; }

            [JsonPropertyName("proxy")]
            public bool? Proxy { get; set; }

            [JsonPropertyName("timeout")]
            public string? Timeout { get; set; }
        }
#nullable disable

        public enum Flash
        {
            Flash
        }
    }

    [CustomTypeIdentifier("face")]
    public class FaceMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "face";

        [JsonPropertyName("data")]
        public FaceData Data { get; set; }

        public class FaceData
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }
        }
    }

    [CustomTypeIdentifier("record")]
    public class RecordMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "record";

        [JsonPropertyName("data")]
        public RecordData Data { get; set; }

#nullable enable
        public class RecordData
        {
            [JsonPropertyName("file")]
            public string? File { get; set; }

            [JsonPropertyName("magic")]
            public bool? Magic { get; set; }

            [JsonPropertyName("url")]
            public string? Url { get; set; }

            [JsonPropertyName("cache")]
            public bool? Cache { get; set; }

            [JsonPropertyName("proxy")]
            public bool? Proxy { get; set; }

            [JsonPropertyName("timeout")]
            public string? Timeout { get; set; }
        }
#nullable disable
    }

    [CustomTypeIdentifier("video")]
    public class VideoMessgae : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "video";

        [JsonPropertyName("data")]
        public VideoData Data { get; set; }

#nullable enable
        public class VideoData
        {
            [JsonPropertyName("file")]
            public string? File { get; set; }

            [JsonPropertyName("url")]
            public string? Url { get; set; }

            [JsonPropertyName("cache")]
            public bool? Cache { get; set; }

            [JsonPropertyName("proxy")]
            public bool? Proxy { get; set; }

            [JsonPropertyName("timeout")]
            public string? Timeout { get; set; }
        }
#nullable disable
    }

    [CustomTypeIdentifier("at")]
    public class AtMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "at";

        public AtData Data { get; set; }

        public class AtData
        {
            [JsonPropertyName("qq")]
            public string QQ { get; set; }
        }
    }

    [CustomTypeIdentifier("rps")]
    public class RpsMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "rps";
    }

    [CustomTypeIdentifier("dice")]
    public class DiceMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "dice";
    }

    [CustomTypeIdentifier("shake")]
    public class ShakeMessgae : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "shake";
    }

    [CustomTypeIdentifier("poke")]
    public class PokeMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "poke";

        [JsonPropertyName("data")]
        public PokeData Data { get; set; }

        public class PokeData
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("id")]
            public long Id { get; set; }

#nullable enable
            [JsonPropertyName("name")]
            public string? Name { get; set; }
#nullable disable
        }
    }

    [CustomTypeIdentifier("anonymous")]
    public class AnonymousMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "anonymous";

        [JsonPropertyName("data")]
        public AnonymousData Data { get; set; }

        public class AnonymousData
        {
#nullable enable
            [JsonPropertyName("ignore")]
            public bool? Ignore { get; set; }
#nullable disable
        }
    }

    [CustomTypeIdentifier("share")]
    public class ShareMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "share";

        [JsonPropertyName("data")]
        public ShareData Data { get; set; }

        public class ShareData
        {
            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

#nullable enable
            [JsonPropertyName("content")]
            public string? Content { get; set; }

            [JsonPropertyName("image")]
            public string? Image { get; set; }
#nullable disable
        }
    }

    [CustomTypeIdentifier("contact")]
    public class ContactMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "contact";

        [JsonPropertyName("data")]
        public ContactData Data { get; set; }

        public class ContactData
        {
            [JsonPropertyName("type")]
            public ContactType Type { get; set; }

            [JsonPropertyName("id")]
            public long Id { get; set; }
        }

        public enum ContactType
        {
            QQ,
            Group
        }
    }

    [CustomTypeIdentifier("location")]
    public class LocationMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "location";

        [JsonPropertyName("data")]
        public LocationData Data { get; set; }

        public class LocationData
        {
            [JsonPropertyName("lat")]
            public string Lat { get; set; }

            [JsonPropertyName("lon")]
            public string Lon { get; set; }

#nullable enable
            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("content")]
            public string? Content { get; set; }
#nullable disable
        }
    }

    [CustomTypeIdentifier("music")]
    public class MusicMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "music";

        [JsonPropertyName("data")]
        public MusicData Data { get; set; }

        public class MusicData
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }

#nullable enable
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("url")]
            public string? Url { get; set; }

            [JsonPropertyName("audio")]
            public string? Audio { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("content")]
            public string? Content { get; set; }

            [JsonPropertyName("image")]
            public string? Image { get; set; }
#nullable disable
        }
    }

    [CustomTypeIdentifier("reply")]
    public class ReplyMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "reply";

        [JsonPropertyName("data")]
        public ReplyData Data { get; set; }

        public class ReplyData
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
        }
    }

    [CustomTypeIdentifier("forward")]
    public class ForwardMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "forward";

        [JsonPropertyName("data")]
        public ForwardData Data { get; set; }

        public class ForwardData
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
        }
    }

    [CustomTypeIdentifier("node")]
    public class NodeMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "node";

        [JsonPropertyName("data")]
        public NodeData Data { get; set; }

        public class NodeData
        {
#nullable enable
            [JsonPropertyName("id")]
            public int? Id { get; set; }

            [JsonPropertyName("user_id")]
            public long? UserId { get; set; }

            [JsonPropertyName("nickname")]
            public string? Nickname { get; set; }

            [JsonPropertyName("content")]
            public MessageChain? Content { get; set; }
#nullable disable
        }
    }

    [CustomTypeIdentifier("xml")]
    public class XmlMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "xml";

        [JsonPropertyName("data")]
        public XmlData Data { get; set; }

        public class XmlData
        {
            [JsonPropertyName("data")]
            public string Data { get; set; }
        }
    }

    [CustomTypeIdentifier("json")]
    public class JsonMessage : MessageBase
    {
        [JsonPropertyName("type")]
        public override string Type => "json";

        [JsonPropertyName("data")]
        public XmlData Data { get; set; }

        public class XmlData
        {
            [JsonPropertyName("data")]
            public string Data { get; set; }
        }
    }
}
