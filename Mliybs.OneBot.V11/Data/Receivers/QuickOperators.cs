using System;
using System.Linq;
using System.Threading.Tasks;
using Mliybs.OneBot.V11.Data.Messages;
using Mliybs.OneBot.V11.Data.Receivers.Messages;
using Mliybs.OneBot.V11.Utils;

namespace Mliybs.OneBot.V11.Data
{
    public class MessageQuickOperator(MessageReceiver receiver, OneBot bot)
    {
#if !NET6_0_OR_GREATER
        private readonly Random random = new();
#endif

        public async Task<Message> Send(MessageChain message)
        {
            if (receiver is GroupMessageReceiver group)
                return await bot.SendGroupMessage(group.GroupId, message);

            if (receiver is PrivateMessageReceiver @private)
                return await bot.SendPrivateMessage(@private.UserId, message);

            else throw new InvalidOperationException("receiver类型不支持！");
        }

        public async Task<Message> SendRandom(params MessageChain[] messages)
        {
#if NET6_0_OR_GREATER
            return await Send(messages[Random.Shared.Next(messages.Length)]);
#else
            return await Send(messages[random.Next(messages.Length)]);
#endif
        }

        public async Task<Message> Reply(MessageChain message)
        {
            if (receiver is GroupMessageReceiver group)
            {
                var @new = new MessageChainBuilder()
                    .Reply(group.MessageId)
                    .If(group.Sender.UserId != null, x => x
                        .At(group.Sender.UserId!.Value)
                        .Text(" "))
                    .Build();
                return await bot.SendGroupMessage(group.GroupId, @new.Concat(message).ToMessageChain());
            }

            if (receiver is PrivateMessageReceiver @private)
            {
                message.Insert(0, new ReplyMessage()
                {
                    Data = new()
                    {
                        Id = @private.MessageId
                    }
                });
                return await bot.SendPrivateMessage(@private.UserId, message);
            }

            else throw new InvalidOperationException("receiver类型不支持！");
        }

        public async Task<Message> ReplyRandom(params MessageChain[] messages)
        {
#if NET6_0_OR_GREATER
            return await Reply(messages[Random.Shared.Next(messages.Length)]);
#else
            return await Reply(messages[random.Next(messages.Length)]);
#endif
        }

        public bool RepliedCompare(string text)
        {
            var task = RepliedCompareAsync(text);
            task.Wait();
            return task.Result;
        }

        public async Task<bool> RepliedCompareAsync(string text)
        {
            switch (receiver.Message)
            {
                case [ReplyMessage reply, AtMessage at, TextMessage message, ..]:
                    if (message.Data.Text.StartsWith(' ')) message.Data.Text = message.Data.Text[1..];
                    receiver.Message.RemoveRange(0, 2);
                    if ((await bot.GetMessage(reply.Data.Id)).Sender.UserId == receiver.SelfId && at.Data.QQ == receiver.SelfId.ToString())
                        return receiver.Message == text;
                    
                    return receiver.Message == text;
                
                case [ReplyMessage reply, TextMessage, ..]:
                    receiver.Message.RemoveAt(0);
                    if ((await bot.GetMessage(reply.Data.Id)).Sender.UserId == receiver.SelfId)
                        return receiver.Message == text;

                    return false;
                
                case [AtMessage at, TextMessage message, ..]:
                    if (message.Data.Text.StartsWith(' ')) message.Data.Text = message.Data.Text[1..];
                    receiver.Message.RemoveAt(0);
                    if (at.Data.QQ == receiver.SelfId.ToString())
                        return receiver.Message == text;
                    
                    return false;

                default:
                    // 上方已处理私聊时有ReplyMessage的情况
                    if (receiver.MessageType == OneBotMessageType.Private)
                        return receiver.Message == text;

                    return false;
            }
        }
    }
}