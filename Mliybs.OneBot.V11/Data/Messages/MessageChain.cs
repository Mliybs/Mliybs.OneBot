using System;
using System.Collections.Generic;
using System.Text;

namespace Mliybs.OneBot.V11.Data.Messages
{
    public class MessageChain : List<MessageBase>
    {
        public MessageChain() { }
        public MessageChain(IEnumerable<MessageBase> messages) : base(messages) { }
    }
}
