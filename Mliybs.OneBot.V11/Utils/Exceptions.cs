using Mliybs.OneBot.V11.Data.Receivers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mliybs.OneBot.V11.Utils
{
    public class OperationFailedException(ReplyResult result) : Exception
    {
        public ReplyResult ReplyResult => result;
    }
}
