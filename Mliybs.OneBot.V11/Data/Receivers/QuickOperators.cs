using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mliybs.OneBot.V11.Data.Messages;
using Mliybs.OneBot.V11.Data.Receivers.Messages;
using Mliybs.OneBot.V11.Utils;

namespace Mliybs.OneBot.V11.Data
{
    public class MessageQuickOperator(MessageReceiver receiver, OneBot bot)
    {
#if !NET6_0_OR_GREATER
        private static readonly ThreadLocal<Random> random = new(() => new());
#endif

        public async Task<Message> Send(MessageChain message)
        {
            if (receiver is GroupMessageReceiver group)
                return await bot.SendGroupMessage(group.GroupId, message).ConfigureAwait(false);

            if (receiver is PrivateMessageReceiver @private)
                return await bot.SendPrivateMessage(@private.UserId, message).ConfigureAwait(false);

            throw new InvalidOperationException("receiver类型不支持！");
        }

        public async Task<Message> SendRandom(params MessageChain[] messages)
        {
#if NET6_0_OR_GREATER
            return await Send(messages[Random.Shared.Next(messages.Length)]).ConfigureAwait(false);
#else
            return await Send(messages[random.Value.Next(messages.Length)]).ConfigureAwait(false);
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
                    .AddRange(message)
                    .Build();
                return await bot.SendGroupMessage(group.GroupId, @new).ConfigureAwait(false);
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
                return await bot.SendPrivateMessage(@private.UserId, message).ConfigureAwait(false);
            }

            throw new InvalidOperationException("receiver类型不支持！");
        }

        public async Task<Message> ReplyRandom(params MessageChain[] messages)
        {
#if NET6_0_OR_GREATER
            return await Reply(messages[Random.Shared.Next(messages.Length)]).ConfigureAwait(false);
#else
            return await Reply(messages[random.Value.Next(messages.Length)]).ConfigureAwait(false);
#endif
        }

        public async Task<MessageChain?> AsReplied()
        {
            if (receiver.MessageType == OneBotMessageType.Private) return new(receiver.Message);

            switch (receiver.Message)
            {
                case [ReplyMessage reply, AtMessage at, var message, ..]:
                    if ((await bot.GetMessage(reply.Data.Id).ConfigureAwait(false)).Sender.UserId == receiver.SelfId && at.Data.QQ == receiver.SelfId.ToString())
                        return
                        [
                            message is TextMessage text && text.Data.Text.StartsWith(' ')
                                ? (TextMessage)text.Data.Text[1..]
                                : message,
                            ..receiver.Message.Skip(3),
                        ];
                    
                    return null;
                
                case [ReplyMessage reply, var message, ..]:
                    if ((await bot.GetMessage(reply.Data.Id).ConfigureAwait(false)).Sender.UserId == receiver.SelfId)
                        return
                        [
                            message, ..receiver.Message.Skip(2)
                        ];

                    return null;
                
                case [AtMessage at, var message, ..]:
                    if (at.Data.QQ == receiver.SelfId.ToString())
                        return
                        [
                            message is TextMessage text && text.Data.Text.StartsWith(' ')
                                ? (TextMessage)text.Data.Text[1..]
                                : message,
                            ..receiver.Message.Skip(2),
                        ];

                    return null;

                default:
                    return null;
            }
        }
    }
}