using Mliybs.OneBot.V11;
using Mliybs.OneBot.V11.Data.Messages;
using Mliybs.OneBot.V11.Data.Receivers.Messages;
using System.Diagnostics;
using System.Reactive.Linq;

AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
{
    Debug.WriteLine(e.Exception);
};

var bot = new OneBot("ws://localhost:3001", OneBotConnectionType.WebSocket);

Console.ReadKey();

var result = await bot.GetGroupMemberList(1145141919810);

Debug.WriteLine(result);

Console.ReadKey();