using Mliybs.OneBot.V11.Data;
using Mliybs.OneBot.V11.Data.Messages;
using Mliybs.OneBot.V11.Data.Receivers;
using Mliybs.OneBot.V11.Data.Receivers.Messages;
using Mliybs.OneBot.V11.Data.Receivers.Metas;
using Mliybs.OneBot.V11.Data.Receivers.Notices;
using Mliybs.OneBot.V11.Data.Receivers.Requests;
using Mliybs.OneBot.V11.Utils;
using System;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mliybs.OneBot.V11
{
    public enum OneBotConnectionType
    {
        HTTP,
        WebSocket,
        WebSocketReverse
    }

    public enum OneBotMessageType
    {
        Private,
        Group
    }

    public class OneBot : IDisposable
    {
        protected IOneBotHandler handler;

        public static OneBot Http(string url, string local, string? token = null) => new(new HttpOneBotHandler(url, local, token));

        public static OneBot Websocket(Uri uri, string? token = null) => new(new WebsocketOneBotHandler(uri, token));

        public static OneBot Websocket(string url, string? token = null) => new(new WebsocketOneBotHandler(new(url), token));

        public static OneBot Websocket(Uri uri, out Action reconnect, string? token = null) => new(new WebsocketOneBotHandler(uri, out reconnect, token));

        public static OneBot Websocket(string url, out Action reconnect, string? token = null) => new(new WebsocketOneBotHandler(new(url), out reconnect, token));

        public static OneBot WebsocketReverse(string url, string? token = null) => new(new WebsocketReverseOneBotHandler(url, token));

        public static OneBot WebsocketReverse(string url, out Func<bool> connected, string? token = null) => new(new WebsocketReverseOneBotHandler(url, out connected, token));

        public OneBot(IOneBotHandler botHandler) => handler = botHandler;

        public void Dispose() => handler.Dispose();

        public async Task<Message> SendPrivateMessage(long userId, MessageChain message, bool autoEscape = false)
        {
            return (await handler.SendAsync(OneBotStatics.SendPrivateMessage, new
            {
                user_id = userId,
                message,
                auto_escape = autoEscape
            }).ConfigureAwait(false)).Data.Deserialize<Message>()!;
        }

        public async Task<Message> SendGroupMessage(long groupId, MessageChain message, bool autoEscape = false)
        {
            return (await handler.SendAsync(OneBotStatics.SendGroupMessage, new
            {
                group_id = groupId,
                message,
                auto_escape = autoEscape
            }).ConfigureAwait(false)).Data.Deserialize<Message>()!;
        }

        public async Task<Message> SendMessage(OneBotMessageType messageType, long id, MessageChain message, bool autoEscape = false)
        {
            object obj = messageType switch
            {
                OneBotMessageType.Private => new { user_id = id, message, auto_escape = autoEscape },
                OneBotMessageType.Group => new { group_id = id, message, auto_escape = autoEscape },
                _ => throw new ArgumentException("消息类型不正确！", nameof(messageType))
            };
            return (await handler.SendAsync(OneBotStatics.SendMessage, obj).ConfigureAwait(false)).Data.Deserialize<Message>()!;
        }

        public async Task DeleteMessage(int messageId)
        {
            await handler.SendAsync(OneBotStatics.DeleteMessage, new { message_id = messageId }).ConfigureAwait(false);
        }

        public async Task<DetailedMessage> GetMessage(int messageId)
        {
            return (await handler.SendAsync(OneBotStatics.GetMessage, new { message_id = messageId }).ConfigureAwait(false)).Data.Deserialize<DetailedMessage>()!;
        }

        public async Task<MessageChain> GetForwardMessage(string messageId)
        {
            return (await handler.SendAsync(OneBotStatics.GetForwardMessage, new { id = messageId }).ConfigureAwait(false)).Data.GetProperty("message").DeserializeMessageChain();
        }

        public async Task SendLike(long userId, int times = 1)
        {
            await handler.SendAsync(OneBotStatics.SendLike, new { user_id = userId, times }).ConfigureAwait(false);
        }

        public async Task SetGroupKick(long groupId, long userId, bool rejectAddRequest = false)
        {
            await handler.SendAsync(OneBotStatics.SetGroupKick, new
            {
                group_id = groupId,
                user_id = userId,
                reject_add_request = rejectAddRequest
            }).ConfigureAwait(false);
        }

        public async Task SetGroupBan(long groupId, long userId, long duration = 30 * 60)
        {
            await handler.SendAsync(OneBotStatics.SetGroupBan, new
            {
                group_id = groupId,
                user_id = userId,
                duration
            }).ConfigureAwait(false);
        }

        public async Task SetGroupAnonymousBan(long groupId, Anonymous anonymous, long duration = 30 * 60)
        {
            await handler.SendAsync(OneBotStatics.SetGroupAnonymousBan, new
            {
                group_id = groupId,
                anonymous,
                duration
            }).ConfigureAwait(false);
        }

        public async Task SetGroupAnonymousBan(long groupId, string flag, long duration = 30 * 60)
        {
            await handler.SendAsync(OneBotStatics.SetGroupAnonymousBan, new
            {
                group_id = groupId,
                anonymous_flag = flag,
                duration
            }).ConfigureAwait(false);
        }

        public async Task SetGroupWholeBan(long groupId, bool enable = true)
        {
            await handler.SendAsync(OneBotStatics.SetGroupWholeBan, new { group_id = groupId, enable }).ConfigureAwait(false);
        }

        public async Task SetGroupAdmin(long groupId, long userId, bool enable = true)
        {
            await handler.SendAsync(OneBotStatics.SetGroupAdmin, new
            {
                group_id = groupId,
                user_id = userId,
                enable
            }).ConfigureAwait(false);
        }

        public async Task SetGroupAnonymous(long groupId, bool enable = true)
        {
            await handler.SendAsync(OneBotStatics.SetGroupAnonymous, new { group_id = groupId, enable, }).ConfigureAwait(false);
        }

        public async Task SetGroupCard(long groupId, long userId, string card = "")
        {
            await handler.SendAsync(OneBotStatics.SetGroupCard, new
            {
                group_id = groupId,
                user_id = userId,
                card
            }).ConfigureAwait(false);
        }

        public async Task SetGroupName(long groupId, string groupName)
        {
            await handler.SendAsync(OneBotStatics.SetGroupName, new { group_id = groupId, group_name = groupName }).ConfigureAwait(false);
        }

        public async Task SetGroupLeave(long groupId, bool isDismiss)
        {
            await handler.SendAsync(OneBotStatics.SetGroupLeave, new { group_id = groupId, is_dismiss = isDismiss }).ConfigureAwait(false);
        }

        public async Task SetGroupSpecialTitle(long groupId, long userId, string specialTitle, long duration = -1)
        {
            await handler.SendAsync(OneBotStatics.SetGroupSpecialTitle, new
            {
                group_id = groupId,
                user_id = userId,
                special_title = specialTitle,
                duration
            }).ConfigureAwait(false);
        }

        public async Task SetFriendAddRequest(string flag, bool approve = true, string remark = "")
        {
            await handler.SendAsync(OneBotStatics.SetFriendAddRequest, new { flag, approve, remark }).ConfigureAwait(false);
        }

        public async Task SetGroupAddRequest(string flag, GroupRequestReceiver.GroupRequestType type, bool approve = true, string reason = "")
        {
            object obj = type switch
            {
                GroupRequestReceiver.GroupRequestType.Add => new { flag, type = "add", approve, reason },
                GroupRequestReceiver.GroupRequestType.Invite => new { flag, type = "invite", approve, reason },
                _ => throw new ArgumentException("请求类型不正确！", nameof(type))
            };
            await handler.SendAsync(OneBotStatics.SetFriendAddRequest, obj).ConfigureAwait(false);
        }

        public async Task<LoginInfo> GetLoginInfo()
        {
            return (await handler.SendAsync(OneBotStatics.GetLoginInfo, new { }).ConfigureAwait(false)).Data.Deserialize<LoginInfo>()!;
        }

        public async Task<StrangerInfo> GetStrangerInfo(long userId, bool noCache = false)
        {
            return (await handler.SendAsync(OneBotStatics.GetStrangerInfo, new { user_id = userId, no_cache = noCache }).ConfigureAwait(false)).Data.Deserialize<StrangerInfo>()!;
        }

        public async Task<FriendInfo[]> GetFriendList()
        {
            return (await handler.SendAsync(OneBotStatics.GetFriendList, new { }).ConfigureAwait(false)).Data.Deserialize<FriendInfo[]>()!;
        }

        public async Task<GroupInfo> GetGroupInfo(long groupId, bool noCache = false)
        {
            return (await handler.SendAsync(OneBotStatics.GetGroupInfo, new { group_id = groupId, no_cache = noCache }).ConfigureAwait(false)).Data.Deserialize<GroupInfo>()!;
        }

        public async Task<GroupInfo[]> GetGroupList()
        {
            return (await handler.SendAsync(OneBotStatics.GetGroupList, new { }).ConfigureAwait(false)).Data.Deserialize<GroupInfo[]>()!;
        }

        public async Task<GroupMemberInfo> GetGroupMemberInfo(long groupId, long userId, bool noCache = false)
        {
            return (await handler.SendAsync(OneBotStatics.GetGroupMemberInfo, new
            {
                group_id = groupId,
                user_id = userId,
                no_cache = noCache
            }).ConfigureAwait(false)).Data.Deserialize<GroupMemberInfo>()!;
        }

        public async Task<GroupMemberInfo[]> GetGroupMemberList(long groupId)
        {
            return (await handler.SendAsync(OneBotStatics.GetGroupMemberList, new { group_id = groupId }).ConfigureAwait(false)).Data.Deserialize<GroupMemberInfo[]>()!;
        }

        public async Task<HonorInfos> GetGroupHonorInfo(long groupId, string type)
        {
            return (await handler.SendAsync(OneBotStatics.GetGroupHonorInfo, new
            {
                group_id = groupId,
                type
            }).ConfigureAwait(false)).Data.Deserialize<HonorInfos>()!;
        }

        public async Task<CookiesInfo> GetCookies(string domain)
        {
            return (await handler.SendAsync(OneBotStatics.GetCookies, new { domain }).ConfigureAwait(false)).Data.Deserialize<CookiesInfo>()!;
        }

        public async Task<CsrfInfo> GetCsrfToken()
        {
            return (await handler.SendAsync(OneBotStatics.GetCsrfToken, new { }).ConfigureAwait(false)).Data.Deserialize<CsrfInfo>()!;
        }

        public async Task<Credentials> GetCredentials(string domain)
        {
            return (await handler.SendAsync(OneBotStatics.GetCredentials, new { domain }).ConfigureAwait(false)).Data.Deserialize<Credentials>()!;
        }

        public async Task<FileInfo> GetRecord(string file, string outFormat)
        {
            return (await handler.SendAsync(OneBotStatics.GetRecord, new { file, out_format = outFormat }).ConfigureAwait(false)).Data.Deserialize<FileInfo>()!;
        }

        public async Task<FileInfo> GetImage(string file)
        {
            return (await handler.SendAsync(OneBotStatics.GetImage, new { file }).ConfigureAwait(false)).Data.Deserialize<FileInfo>()!;
        }

        public async Task<bool> CanSendImage()
        {
            return (await handler.SendAsync(OneBotStatics.CanSendImage, new { }).ConfigureAwait(false)).Data.GetProperty("yes").GetBoolean();
        }

        public async Task<bool> CanSendRecord()
        {
            return (await handler.SendAsync(OneBotStatics.CanSendRecord, new { }).ConfigureAwait(false)).Data.GetProperty("yes").GetBoolean();
        }

        public async Task<StatusInfo> GetStatus()
        {
            return (await handler.SendAsync(OneBotStatics.GetStatus, new { }).ConfigureAwait(false)).Data.Deserialize<StatusInfo>()!;
        }

        public async Task<VersionInfo> GetVersionInfo()
        {
            return (await handler.SendAsync(OneBotStatics.GetVersionInfo, new { }).ConfigureAwait(false)).Data.Deserialize<VersionInfo>()!;
        }

        public async Task SetRestart(int delay)
        {
            await handler.SendAsync(OneBotStatics.SetRestart, new { delay }).ConfigureAwait(false);
        }

        public async Task CleanCache()
        {
            await handler.SendAsync(OneBotStatics.CleanCache, new { }).ConfigureAwait(false);
        }

        public async Task<JsonElement> Custom(string action, object data)
        {
            return (await handler.SendAsync(action, data).ConfigureAwait(false)).Data;
        }

        public IObservable<MessageReceiver> MessageReceived => handler.MessageReceived.ObserveOn(Scheduler.Default);

        public IObservable<NoticeReceiver> NoticeReceived => handler.NoticeReceived.ObserveOn(Scheduler.Default);

        public IObservable<RequestReceiver> RequestReceived => handler.RequestReceived.ObserveOn(Scheduler.Default);

        public IObservable<MetaReceiver> MetaReceived => handler.MetaReceived.ObserveOn(Scheduler.Default);

        public IObservable<UnknownReceiver> UnknownReceived => handler.UnknownReceived.ObserveOn(Scheduler.Default);
    }
}
