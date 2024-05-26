using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using Mliybs.OneBot.V11.Data.Messages;
using System.Text.Json;
using Mliybs.OneBot.V11.Data.Receivers;
using Mliybs.OneBot.V11.Data.Receivers.Messages;
using Mliybs.OneBot.V11.Data.Receivers.Notices;
using Mliybs.OneBot.V11.Data.Receivers.Requests;
using Mliybs.OneBot.V11.Data.Receivers.Metas;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Mliybs.OneBot.V11.Utils
{
    public static class UtilHelpers
    {
        public static readonly Dictionary<string, Type> MessageReceivers = new();

        public static readonly Dictionary<string, Type> NoticeReceivers = new();

        public static readonly Dictionary<string, Type> RequestReceivers = new();

        public static readonly Dictionary<string, Type> MetaReceivers = new();

        public static readonly Dictionary<string, Type> MessageTypes = new();

        public static readonly JsonSerializerOptions Options = new()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new StringIntConverter(),
                new NullableStringIntConverter(),
                new StringLongConverter(),
                new NullableStringLongConverter(),
                new BooleanConverter(),
                new NullableBooleanConverter()
            }
        };

        static UtilHelpers()
        {
            var types = typeof(UtilHelpers).Assembly.GetTypes();

            foreach (var type in types)
            {
                if (type.IsAbstract) continue;

                else if (type.IsSubclassOf(typeof(MessageBase)))
                    MessageTypes.Add(type.GetCustomAttribute<CustomTypeIdentifierAttribute>()?.Name ?? type.Name, type);

                else if (type.IsSubclassOf(typeof(MessageReceiver)))
                    MessageReceivers.Add(type.GetCustomAttribute<CustomTypeIdentifierAttribute>()?.Name ?? type.Name, type);

                else if (type.IsSubclassOf(typeof(NoticeReceiver)))
                    NoticeReceivers.Add(type.GetCustomAttribute<CustomTypeIdentifierAttribute>()?.Name ?? type.Name, type);

                else if (type.IsSubclassOf(typeof(RequestReceiver)))
                    RequestReceivers.Add(type.GetCustomAttribute<CustomTypeIdentifierAttribute>()?.Name ?? type.Name, type);

                else if (type.IsSubclassOf(typeof(MetaReceiver)))
                    MetaReceivers.Add(type.GetCustomAttribute<CustomTypeIdentifierAttribute>()?.Name ?? type.Name, type);
            }
        }

        internal static (string json, string id) BuildWebSocketJson(string action, object @params)
        {
            var id = Guid.NewGuid().ToString();
            var json = JsonSerializer.Serialize(new
            {
                action,
                @params,
                echo = id
            }, Options);
            return (json, id);
        }

        public static Task<ReplyResult> WaitForReply(this IOneBotHandler handler, string id)
        {
            TaskCompletionSource<ReplyResult> source = new();
            handler.OnReply.Add(id, x =>
            {
                if (x.Status == "ok")
                {
                    handler.OnReply.Remove(id);
                    source.SetResult(x);
                }

                else if (x.Status == "failed")
                {
                    handler.OnReply.Remove(id);
                    source.SetException(new OperationFailedException(x));
                }

                else if (x.Status == "async")
                {
                    handler.OnReply.Remove(id);
                    source.SetResult(x);
                }
            });
            return source.Task;
        }

        internal static MessageChain ToMessageChain(this IEnumerable<object?> messages) =>
            new(messages.OfType<MessageBase>());

        internal static MessageChain DeserializeMessageChain(this JsonElement json)
        {
            var array = json.EnumerateArray();
            return array.Select(x => x.Deserialize(MessageTypes[x.GetProperty("type").GetString()!], Options)).ToMessageChain();
        }

        public static void Handle(string text,
                IObserver<MessageReceiver> messageReceived,
                IObserver<NoticeReceiver> noticeReceived,
                IObserver<RequestReceiver> requestReceived,
                IObserver<MetaReceiver> metaReceived,
                IObserver<UnknownReceiver> unknownReceived,
                IDictionary<string, Action<ReplyResult>> onReply)
        {
            var json = JsonDocument.Parse(text);
            if (json.RootElement.TryGetProperty("post_type", out var element))
            {
                var type = element.GetString();
                if (type == "message")
                {
                    try
                    {
                        var _type = MessageReceivers[json.RootElement.GetProperty("message_type").GetString()!];
                        var obj = json.Deserialize(_type, Options)!;
                        messageReceived.OnNext((MessageReceiver)obj ?? throw new NullReferenceException());
                    }
                    catch (Exception e)
                    {
                        messageReceived.OnError(e);
                    }
                }
                else if (type == "notice")
                {
                    try
                    {
                        var _type = NoticeReceivers[json.RootElement.GetProperty("notice_type").GetString()!];
                        var obj = json.Deserialize(_type, Options)!;
                        noticeReceived.OnNext((NoticeReceiver)obj ?? throw new NullReferenceException());
                    }
                    catch (Exception e)
                    {
                        noticeReceived.OnError(e);
                    }
                }
                else if (type == "request")
                {
                    try
                    {
                        var _type = RequestReceivers[json.RootElement.GetProperty("request_type").GetString()!];
                        var obj = json.Deserialize(_type, Options)!;
                        requestReceived.OnNext((RequestReceiver)obj ?? throw new NullReferenceException());
                    }
                    catch (Exception e)
                    {
                        requestReceived.OnError(e);
                    }
                }
                else if (type == "meta_event")
                {
                    try
                    {
                        var _type = MetaReceivers[json.RootElement.GetProperty("meta_event_type").GetString()!];
                        var obj = json.Deserialize(_type, Options)!;
                        metaReceived.OnNext((MetaReceiver)obj ?? throw new NullReferenceException());
                    }
                    catch (Exception e)
                    {
                        metaReceived.OnError(e);
                    }
                }
                else unknownReceived.OnNext(new UnknownReceiver
                {
                    RawText = text
                });
            }
            else
            {
                try
                {
                    var result = json.Deserialize<ReplyResult>(Options)!;
                    result.Data = json.RootElement.GetProperty("data");
                    if (onReply.TryGetValue(result.Echo ?? throw new NullReferenceException(), out var action)) action.Invoke(result);
                }
                catch { }
            }
        }
    }
}
