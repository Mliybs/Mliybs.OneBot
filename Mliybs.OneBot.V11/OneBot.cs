using Mliybs.OneBot.V11.Data;
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

        public async Task<Message> SendPrivateMsg(long userId, string message, bool autoEscape = false)
        {
            var (json, id) = ("send_private_msg", new
            {
                user_id = userId,
                message,
                auto_escape = autoEscape
            }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<Message>()!;
        }
        public async Task<Message> SendGroupMsg(long groupId, string message, bool autoEscape = false)
        {
            var (json, id) = ("send_group_msg", new
            {
                group_id = groupId,
                message,
                auto_escape = autoEscape
            }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<Message>()!;
        }

        public async Task<Message> SendMsg(OneBotMessageType messageType, long id, string message, bool autoEscape = false)
        {
            object obj = messageType switch
            {
                OneBotMessageType.Private => new { user_id = id, message, auto_escape = autoEscape },
                OneBotMessageType.Group => new { group_id = id, message, auto_escape = autoEscape },
                _ => throw new ArgumentException()
            };
            var (json, _id) = ("send_msg", obj).BuildJson();
            var task = _id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<Message>()!;
        }

        public async Task DeleteMsg(int messageId)
        {
            var (json, id) = ("delete_msg", new { message_id = messageId }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task<DetailedMessage> GetMsg(int messageId)
        {
            var (json, id) = ("get_msg", new { message_id = messageId }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<DetailedMessage>()!;
        }

        public async Task<NodeMessages> GetForwardMsg(string messageId)
        {
            var (json, id) = ("get_forward", new { id = messageId }).BuildJson();
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
            var (json, id) = ("send_like", new { user_id = userId, times }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupKick(long groupId, long userId, bool rejectAddRequest = false)
        {
            var (json, id) = ("set_group_kick", new
            {
                group_id = groupId,
                user_id = userId,
                reject_add_request = rejectAddRequest
            }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupBan(long groupId, long userId, long duration = 30 * 60)
        {
            var (json, id) = ("set_group_ban", new
            {
                group_id = groupId,
                user_id = userId,
                duration
            }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupAnonymousBan(long groupId, Anonymous anonymous, long duration = 30 * 60)
        {
            var (json, id) = ("set_group_anonymous_ban", new
            {
                group_id = groupId,
                anonymous,
                duration
            }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupAnonymousBan(long groupId, string flag, long duration = 30 * 60)
        {
            var (json, id) = ("set_group_anonymous_ban", new
            {
                group_id = groupId,
                anonymous_flag = flag,
                duration
            }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupWholeBan(long groupId, bool enable = true)
        {
            var (json, id) = ("set_group_whole_ban", new { group_id = groupId, enable }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupAdmin(long groupId, long userId, bool enable = true)
        {
            var (json, id) = ("set_group_admin", new
            {
                group_id = groupId,
                user_id = userId,
                enable
            }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupAnonymous(long groupId, bool enable = true)
        {
            var (json, id) = ("set_group_anonymous", new { group_id = groupId, enable, }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupCard(long groupId, long userId, string card = "")
        {
            var (json, id) = ("set_group_card", new
            {
                group_id = groupId,
                user_id = userId,
                card
            }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupName(long groupId, string groupName)
        {
            var (json, id) = ("set_group_name", new { group_id = groupId, group_name = groupName }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupLeave(long groupId, bool isDismiss)
        {
            var (json, id) = ("set_group_leave", new { group_id = groupId, is_dismiss = isDismiss }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupSpecialTitle(long groupId, long userId, string specialTitle, long duration = -1)
        {
            var (json, id) = ("set_group_special_title", new
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
            var (json, id) = ("set_friend_add_request", new { flag, approve, remark }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task SetGroupAddRequest(string flag, GroupRequestReceiver.GroupRequestType type, bool approve = true, string reason = "")
        {
            object obj = type switch
            {
                GroupRequestReceiver.GroupRequestType.Add => new { flag, type = "add", approve, reason },
                GroupRequestReceiver.GroupRequestType.Invite => new { flag, type = "invite", approve, reason },
                _ => throw new ArgumentException()
            };
            var (json, id) = ("set_group_add_request", obj).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task<LoginInfo> GetLoginInfo()
        {
            var (json, id) = ("get_login_info", new { }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<LoginInfo>()!;
        }

        public async Task<StrangerInfo> GetStrangerInfo(long userId, bool noCache = false)
        {
            var (json, id) = ("get_stranger_info", new { user_id = userId, no_cache = noCache }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<StrangerInfo>()!;
        }

        public async Task<FriendInfo[]> GetFriendList()
        {
            var (json, id) = ("get_friend_list", new { }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<FriendInfo[]>()!;
        }

        public async Task<GroupInfo> GetGroupInfo(long groupId, bool noCache = false)
        {
            var (json, id) = ("get_group_info", new { group_id = groupId, no_cache = noCache }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<GroupInfo>()!;
        }

        public async Task<GroupInfo[]> GetGroupList()
        {
            var (json, id) = ("get_group_list", new { }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<GroupInfo[]>()!;
        }

        public async Task<GroupMemberInfo> GetGroupMemberInfo(long groupId, long userId, bool noCache = false)
        {
            var (json, id) = ("get_group_member_info", new
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
            var (json, id) = ("get_group_member_list", new { group_id = groupId }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<GroupMemberInfo[]>()!;
        }

        public async Task<HonorInfos> GetGroupHonorInfo(long groupId, string type)
        {
            var (json, id) = ("get_group_honor_info", new
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
            var (json, id) = ("get_cookies", new { domain }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<CookiesInfo>()!;
        }

        public async Task<CsrfInfo> GetCsrfToken()
        {
            var (json, id) = ("get_csrf_token", new { }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<CsrfInfo>()!;
        }

        public async Task<Credentials> GetCredentials(string domain)
        {
            var (json, id) = ("get_credentials", new { domain }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<Credentials>()!;
        }

        public async Task<FileInfo> GetRecord(string file, string outFormat)
        {
            var (json, id) = ("get_record", new { file, out_format = outFormat }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<FileInfo>()!;
        }

        public async Task<FileInfo> GetImage(string file)
        {
            var (json, id) = ("get_image", new { file }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<FileInfo>()!;
        }

        public async Task<BooleanInfo> CanSendImage()
        {
            var (json, id) = ("can_send_image", new { }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<BooleanInfo>()!;
        }

        public async Task<BooleanInfo> CanSendRecord()
        {
            var (json, id) = ("can_send_record", new { }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<BooleanInfo>()!;
        }

        public async Task<StatusInfo> GetStatus()
        {
            var (json, id) = ("get_status", new { }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<StatusInfo>()!;
        }

        public async Task<VersionInfo> GetVersionInfo()
        {
            var (json, id) = ("get_version_info", new { }).BuildJson();
            var task = id.WaitForReply(handler);
            await handler.SendAsync(json);
            return (await task).Data.Deserialize<VersionInfo>()!;
        }

        public async Task SetRestart(int delay)
        {
            var (json, id) = ("set_restart", new { delay }).BuildJson();
            await handler.SendAsync(json);
        }

        public async Task CleanCache()
        {
            var (json, id) = ("clean_cache", new { }).BuildJson();
            await handler.SendAsync(json);
        }


        public IObservable<MessageReceiver> MessageReceived => handler.MessageReceived;

        public IObservable<NoticeReceiver> NoticeReceived => handler.NoticeReceived;

        public IObservable<RequestReceiver> RequestReceived => handler.RequestReceived;

        public IObservable<MetaReceiver> MetaReceived => handler.MetaReceived;

        public IObservable<UnknownReceiver> UnknownReceived => handler.UnknownReceived;
    }
}
