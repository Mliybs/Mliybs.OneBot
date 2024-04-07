using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mliybs.OneBot.V11.Data.Receivers.Notices
{
#nullable disable
    public abstract class NoticeReceiver : ReceiverBase
    {
        [JsonPropertyName("notice_type")]
        public abstract string NoticeType { get; }
    }
}
