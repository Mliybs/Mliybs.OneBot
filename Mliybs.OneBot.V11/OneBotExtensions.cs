using Mliybs.OneBot.V11.Data.Receivers.Messages;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace Mliybs.OneBot.V11
{
    public static class OneBotExtensions
    {
        public static IObservable<GroupMessageReceiver> AtGroup(this IObservable<MessageReceiver> source) => source.OfType<GroupMessageReceiver>();

        public static IObservable<PrivateMessageReceiver> AtPrivate(this IObservable<MessageReceiver> source) => source.OfType<PrivateMessageReceiver>();
    }
}
