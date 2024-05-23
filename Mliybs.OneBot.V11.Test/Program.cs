using Mliybs.OneBot.V11;
using Mliybs.OneBot.V11.Data.Messages;
using Mliybs.OneBot.V11.Data.Receivers.Messages;
using Mliybs.OneBot.V11.Data.Receivers.Metas;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text.Json;

AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
{
    Debug.WriteLine(e.Exception);
};

using var bot = OneBot.Websocket("ws://192.168.3.3:3001");

bot.MessageReceived
    .Subscribe(x => _ = x.With(bot).Send(x.Message.GetCQCode()));

Console.Read();

var result = await bot.GetGroupMemberList(1145141919810);

Debug.WriteLine(result);

Console.Read();