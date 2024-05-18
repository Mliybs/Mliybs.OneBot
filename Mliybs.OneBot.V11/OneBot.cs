using Mliybs.OneBot.V11.Data;
using Mliybs.OneBot.V11.Data.Messages;
using Mliybs.OneBot.V11.Data.Receivers;
using Mliybs.OneBot.V11.Data.Receivers.Messages;
using Mliybs.OneBot.V11.Data.Receivers.Metas;
using Mliybs.OneBot.V11.Data.Receivers.Notices;
using Mliybs.OneBot.V11.Data.Receivers.Requests;
using Mliybs.OneBot.V11.Utils;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mliybs.OneBot.V11
{
    public enum OneBotConnectionType
    {
        HTTP,
        HTTP_POST,
        WebSocket,
        WebSocketReverse
    }

    public enum OneBotMessageType
    {
        Private,
        Group
    }

    public class OneBot
    {
        protected readonly IOneBotHandler handler;

        public OneBot(Uri uri, OneBotConnectionType connectionType, string? token = null)
        {
            switch (connectionType)
            {
                case OneBotConnectionType.WebSocket:
                    handler = new WebsocketOneBotHandler(uri, token);
                    break;

                default: throw new NotImplementedException();
            }
        }

        public OneBot(string url, OneBotConnectionType connectionType, string? token = null)
            : this(new Uri(url), connectionType, token) { }

        public OneBot(IOneBotHandler botHandler) => handler = botHandler;

        public async Task ConnectAsync() => await handler.ConnectAsync();

        public async Task<Message> SendPrivateMessage(long userId, MessageChain message, bool autoEscape = false)
        {
            var (json, id) = (OneBotStatics.SendPrivateMessage, new
            {
                user_id = userId,
                message,
                auto_escape = autoEscape
            }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<Message>()!;
        }

        public async Task<Message> SendGroupMessage(long groupId, MessageChain message, bool autoEscape = false)
        {
            var (json, id) = (OneBotStatics.SendGroupMessage, new
            {
                group_id = groupId,
                message,
                auto_escape = autoEscape
            }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<Message>()!;
        }

        public async Task<Message> SendMessage(OneBotMessageType messageType, long id, MessageChain message, bool autoEscape = false)
        {
            object obj = messageType switch
            {
                OneBotMessageType.Private => new { user_id = id, message, auto_escape = autoEscape },
                OneBotMessageType.Group => new { group_id = id, message, auto_escape = autoEscape },
                _ => throw new ArgumentException("消息类型不正确！", nameof(messageType))
            };
            var (json, _id) = (OneBotStatics.SendMessage, obj).BuildJson();
            var task = _id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<Message>()!;
        }

        public async Task DeleteMessage(int messageId)
        {
            var (json, id) = (OneBotStatics.DeleteMessage, new { message_id = messageId }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task<DetailedMessage> GetMessage(int messageId)
        {
            var (json, id) = (OneBotStatics.GetMessage, new { message_id = messageId }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<DetailedMessage>()!;
        }

        public async Task<NodeMessages> GetForwardMessage(string messageId)
        {
            var (json, id) = (OneBotStatics.GetForwardMessage, new { id = messageId }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            var data = (await task).Data;
            var message = data.GetProperty("message").DeserializeMessageChain();
            return new NodeMessages()
            {
                Message = message
            };
        }

        public async Task SendLike(long userId, int times = 1)
        {
            var (json, id) = (OneBotStatics.SendLike, new { user_id = userId, times }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupKick(long groupId, long userId, bool rejectAddRequest = false)
        {
            var (json, id) = (OneBotStatics.SetGroupKick, new
            {
                group_id = groupId,
                user_id = userId,
                reject_add_request = rejectAddRequest
            }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupBan(long groupId, long userId, long duration = 30 * 60)
        {
            var (json, id) = (OneBotStatics.SetGroupBan, new
            {
                group_id = groupId,
                user_id = userId,
                duration
            }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupAnonymousBan(long groupId, Anonymous anonymous, long duration = 30 * 60)
        {
            var (json, id) = (OneBotStatics.SetGroupAnonymousBan, new
            {
                group_id = groupId,
                anonymous,
                duration
            }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupAnonymousBan(long groupId, string flag, long duration = 30 * 60)
        {
            var (json, id) = (OneBotStatics.SetGroupAnonymousBan, new
            {
                group_id = groupId,
                anonymous_flag = flag,
                duration
            }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupWholeBan(long groupId, bool enable = true)
        {
            var (json, id) = (OneBotStatics.SetGroupWholeBan, new { group_id = groupId, enable }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupAdmin(long groupId, long userId, bool enable = true)
        {
            var (json, id) = (OneBotStatics.SetGroupAdmin, new
            {
                group_id = groupId,
                user_id = userId,
                enable
            }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupAnonymous(long groupId, bool enable = true)
        {
            var (json, id) = (OneBotStatics.SetGroupAnonymous, new { group_id = groupId, enable, }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupCard(long groupId, long userId, string card = "")
        {
            var (json, id) = (OneBotStatics.SetGroupCard, new
            {
                group_id = groupId,
                user_id = userId,
                card
            }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupName(long groupId, string groupName)
        {
            var (json, id) = (OneBotStatics.SetGroupName, new { group_id = groupId, group_name = groupName }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupLeave(long groupId, bool isDismiss)
        {
            var (json, id) = (OneBotStatics.SetGroupLeave, new { group_id = groupId, is_dismiss = isDismiss }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupSpecialTitle(long groupId, long userId, string specialTitle, long duration = -1)
        {
            var (json, id) = (OneBotStatics.SetGroupSpecialTitle, new
            {
                group_id = groupId,
                user_id = userId,
                special_title = specialTitle,
                duration
            }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetFriendAddRequest(string flag, bool approve = true, string remark = "")
        {
            var (json, id) = (OneBotStatics.SetFriendAddRequest, new { flag, approve, remark }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupAddRequest(string flag, GroupRequestReceiver.GroupRequestType type, bool approve = true, string reason = "")
        {
            object obj = type switch
            {
                GroupRequestReceiver.GroupRequestType.Add => new { flag, type = "add", approve, reason },
                GroupRequestReceiver.GroupRequestType.Invite => new { flag, type = "invite", approve, reason },
                _ => throw new ArgumentException("请求类型不正确！", nameof(type))
            };
            var (json, id) = (OneBotStatics.SetFriendAddRequest, obj).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task<LoginInfo> GetLoginInfo()
        {
            var (json, id) = (OneBotStatics.GetLoginInfo, new { }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<LoginInfo>()!;
        }

        public async Task<StrangerInfo> GetStrangerInfo(long userId, bool noCache = false)
        {
            var (json, id) = (OneBotStatics.GetStrangerInfo, new { user_id = userId, no_cache = noCache }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<StrangerInfo>()!;
        }

        public async Task<FriendInfo[]> GetFriendList()
        {
            var (json, id) = (OneBotStatics.GetFriendList, new { }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<FriendInfo[]>()!;
        }

        public async Task<GroupInfo> GetGroupInfo(long groupId, bool noCache = false)
        {
            var (json, id) = (OneBotStatics.GetGroupInfo, new { group_id = groupId, no_cache = noCache }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<GroupInfo>()!;
        }

        public async Task<GroupInfo[]> GetGroupList()
        {
            var (json, id) = (OneBotStatics.GetGroupList, new { }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<GroupInfo[]>()!;
        }

        public async Task<GroupMemberInfo> GetGroupMemberInfo(long groupId, long userId, bool noCache = false)
        {
            var (json, id) = (OneBotStatics.GetGroupMemberInfo, new
            {
                group_id = groupId,
                user_id = userId,
                no_cache = noCache
            }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<GroupMemberInfo>()!;
        }

        public async Task<GroupMemberInfo[]> GetGroupMemberList(long groupId)
        {
            var (json, id) = (OneBotStatics.GetGroupMemberList, new { group_id = groupId }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<GroupMemberInfo[]>()!;
        }

        public async Task<HonorInfos> GetGroupHonorInfo(long groupId, string type)
        {
            var (json, id) = (OneBotStatics.GetGroupHonorInfo, new
            {
                group_id = groupId,
                type
            }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<HonorInfos>()!;
        }

        public async Task<CookiesInfo> GetCookies(string domain)
        {
            var (json, id) = (OneBotStatics.GetCookies, new { domain }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<CookiesInfo>()!;
        }

        public async Task<CsrfInfo> GetCsrfToken()
        {
            var (json, id) = (OneBotStatics.GetCsrfToken, new { }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<CsrfInfo>()!;
        }

        public async Task<Credentials> GetCredentials(string domain)
        {
            var (json, id) = (OneBotStatics.GetCredentials, new { domain }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<Credentials>()!;
        }

        public async Task<FileInfo> GetRecord(string file, string outFormat)
        {
            var (json, id) = (OneBotStatics.GetRecord, new { file, out_format = outFormat }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<FileInfo>()!;
        }

        public async Task<FileInfo> GetImage(string file)
        {
            var (json, id) = (OneBotStatics.GetImage, new { file }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<FileInfo>()!;
        }

        public async Task<bool> CanSendImage()
        {
            var (json, id) = (OneBotStatics.CanSendImage, new { }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<BooleanInfo>()!;
        }

        public async Task<bool> CanSendRecord()
        {
            var (json, id) = (OneBotStatics.CanSendRecord, new { }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<BooleanInfo>()!;
        }

        public async Task<StatusInfo> GetStatus()
        {
            var (json, id) = (OneBotStatics.GetStatus, new { }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<StatusInfo>()!;
        }

        public async Task<VersionInfo> GetVersionInfo()
        {
            var (json, id) = (OneBotStatics.GetVersionInfo, new { }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<VersionInfo>()!;
        }

        public async Task SetRestart(int delay)
        {
            var (json, id) = (OneBotStatics.SetRestart, new { delay }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task CleanCache()
        {
            var (json, id) = (OneBotStatics.CleanCache, new { }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task<JsonElement> Custom(string action, object data)
        {
            var (json, id) = (action, data).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data;
        }

        public IObservable<MessageReceiver> MessageReceived => handler.MessageReceived;

        public IObservable<NoticeReceiver> NoticeReceived => handler.NoticeReceived;

        public IObservable<RequestReceiver> RequestReceived => handler.RequestReceived;

        public IObservable<MetaReceiver> MetaReceived => handler.MetaReceived;

        public IObservable<UnknownReceiver> UnknownReceived => handler.UnknownReceived;
    }
}
