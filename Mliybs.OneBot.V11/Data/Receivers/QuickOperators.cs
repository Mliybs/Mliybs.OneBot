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
        public async Task<Message> Send(MessageChain message)
        {
            if (receiver is GroupMessageReceiver group)
                return await bot.SendGroupMessage(group.GroupId, message);

            if (receiver is PrivateMessageReceiver @private)
                return await bot.SendPrivateMessage(@private.UserId, message);

            else throw new InvalidOperationException("receiver类型不支持！");
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
    }
}