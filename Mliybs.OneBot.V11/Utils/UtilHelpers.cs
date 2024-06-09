using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
        public static readonly ConcurrentDictionary<string, Type> MessageReceivers = new();

        public static readonly ConcurrentDictionary<string, Type> NoticeReceivers = new();

        public static readonly ConcurrentDictionary<string, Type> RequestReceivers = new();

        public static readonly ConcurrentDictionary<string, Type> MetaReceivers = new();

        public static readonly ConcurrentDictionary<string, Type> MessageTypes = new();

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

                var name = type.GetCustomAttribute<CustomTypeIdentifierAttribute>()?.Name;

                if (name is null) continue;

                if (type.IsSubclassOf(typeof(MessageBase)))
                    MessageTypes.TryAdd(name, type);

                else if (type.IsSubclassOf(typeof(MessageReceiver)))
                    MessageReceivers.TryAdd(name, type);

                else if (type.IsSubclassOf(typeof(NoticeReceiver)))
                    NoticeReceivers.TryAdd(name, type);

                else if (type.IsSubclassOf(typeof(RequestReceiver)))
                    RequestReceivers.TryAdd(name, type);

                else if (type.IsSubclassOf(typeof(MetaReceiver)))
                    MetaReceivers.TryAdd(name, type);
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
            return array.Select(x => MessageTypes.TryGetValue(x.GetProperty("type").GetString()!, out var type) ?
                x.Deserialize(type, Options)
                : new UnknownMessage()
                {
                    Data = x
                }).ToMessageChain();
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
                    var _type = MessageReceivers[json.RootElement.GetProperty("message_type").GetString()!];
                    var obj = json.Deserialize(_type, Options)!;
                    messageReceived.OnNext((MessageReceiver)obj);
                }
                else if (type == "notice")
                {
                    try
                    {
                        var obj = NoticeReceivers.TryGetValue(json.RootElement.GetProperty("notice_type").GetString()!, out var _type)
                            ? json.Deserialize(_type, Options)!
                            : new UnknownNoticeReceiver()
                            {
                                Data = json.RootElement
                            };
                        noticeReceived.OnNext((NoticeReceiver)obj);
                    }
                    catch (JsonException)
                    {
                        noticeReceived.OnNext(new UnknownNoticeReceiver()
                        {
                            Data = json.RootElement
                        });
                    }
                }
                else if (type == "request")
                {
                    try
                    {
                        var obj = RequestReceivers.TryGetValue(json.RootElement.GetProperty("request_type").GetString()!, out var _type)
                            ? json.Deserialize(_type, Options)!
                            : new UnknownRequestReceiver()
                            {
                                Data = json.RootElement
                            };
                        requestReceived.OnNext((RequestReceiver)obj);
                    }
                    catch (JsonException)
                    {
                        requestReceived.OnNext(new UnknownRequestReceiver()
                        {
                            Data = json.RootElement
                        });
                    }
                }
                else if (type == "meta_event")
                {
                    var _type = MetaReceivers[json.RootElement.GetProperty("meta_event_type").GetString()!];
                    var obj = json.Deserialize(_type, Options)!;
                    metaReceived.OnNext((MetaReceiver)obj);
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
                    result.Data = json.RootElement.TryGetProperty("data", out var data)
                        ? data
                        : new();
                    if (onReply.TryGetValue(result.Echo ?? "", out var action)) action.Invoke(result);
                }
                catch { }
            }
        }
    }
}
