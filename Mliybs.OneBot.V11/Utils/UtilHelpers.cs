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

namespace Mliybs.OneBot.V11.Utils
{
    internal static class UtilHelpers
    {
        public static readonly Dictionary<string, Type> MessageReceivers = new();

        public static readonly Dictionary<string, Type> NoticeReceivers = new();

        public static readonly Dictionary<string, Type> RequestReceivers = new();

        public static readonly Dictionary<string, Type> MetaReceivers = new();

        public static readonly Dictionary<string, Type> MessageTypes = new();

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

        public static (string json, string id) BuildJson(this (string action, object @params) obj)
        {
            var id = Guid.NewGuid().ToString();
            var json = JsonSerializer.Serialize(new
            {
                obj.action,
                obj.@params,
                echo = id
            });
            return (json, id);
        }

        internal static Task<ReplyResult> WaitForReply(this string id, IOneBotHandler handler)
        {
            TaskCompletionSource<ReplyResult> source = new();
            handler.OnReply.Add(id, x =>
            {
                handler.OnReply.Remove(id);
                source.SetResult(x);
            });
            return source.Task;
        }

        internal static MessageChain ToMessageChain(this IEnumerable<object?> messages) =>
            new(messages.OfType<MessageBase>());

        public static MessageChain DeserializeMessageChain(this JsonElement json)
        {
            var array = json.EnumerateArray();
            return array.Select(x => x.Deserialize(MessageTypes[x.GetProperty("type").GetString()!])).ToMessageChain();
        }
    }
}
