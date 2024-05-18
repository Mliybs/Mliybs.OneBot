using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using Mliybs.OneBot.V11.Data.Messages;
using Mliybs.OneBot.V11.Utils;
using Mliybs.OneBot.V11.Data.Receivers;
using Mliybs.OneBot.V11.Data.Receivers.Messages;
using Mliybs.OneBot.V11.Data.Receivers.Notices;
using Mliybs.OneBot.V11.Data.Receivers.Requests;
using Mliybs.OneBot.V11.Data.Receivers.Metas;
using static System.Net.Mime.MediaTypeNames;

namespace Mliybs.OneBot.V11
{
    public interface IOneBotHandler : IDisposable
    {
        IObservable<MessageReceiver> MessageReceived { get; }

        IObservable<NoticeReceiver> NoticeReceived { get; }

        IObservable<RequestReceiver> RequestReceived { get; }

        IObservable<MetaReceiver> MetaReceived { get; }

        IObservable<UnknownReceiver> UnknownReceived { get; }

        IDictionary<string, Action<ReplyResult>> OnReply { get; }

        Task ConnectAsync(bool reconnect = false);

        Task<ReplyResult> SendAsync(string action, object data);
    }

    internal class WebsocketOneBotHandler : IOneBotHandler
    {
        private ClientWebSocket socket = new();

        private readonly Uri uri;

        private readonly Subject<MessageReceiver> messageReceived = new();

        private readonly Subject<NoticeReceiver> noticeReceived = new();

        private readonly Subject<RequestReceiver> requestReceived = new();

        private readonly Subject<MetaReceiver> metaReceived = new();

        private readonly Subject<UnknownReceiver> unknownReceived = new();

        private readonly Dictionary<string, Action<ReplyResult>> onReply = new();

        public WebsocketOneBotHandler(Uri _uri, string? token = null)
        {
            if (token is not null) socket.Options.SetRequestHeader("Authorization", "Bearer " + token);
            uri = _uri;
        }

        public async Task ConnectAsync(bool reconnect = false)
        {
            if (reconnect)
            {
                if (socket.State == WebSocketState.Open) await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                socket.Dispose();
            }
            if (socket.State != WebSocketState.Open) socket = new();
            await socket.ConnectAsync(uri, CancellationToken.None);
            _ = Run();
        }

        public async Task Run()
        {
            while (socket.State == WebSocketState.Open)
            {
                try
                {
                    var bytes = ArrayPool<byte>.Shared.Rent(1024);
                    int read = 0;
                    string text;
                    while (true)
                    {
                        var result = await socket.ReceiveAsync(bytes.AsMemory(read), CancellationToken.None);
                        if (result.EndOfMessage)
                        {
                            text = Encoding.UTF8.GetString(bytes.AsSpan(0, read + result.Count));
                            ArrayPool<byte>.Shared.Return(bytes);
                            break;
                        }
                        else
                        {
                            read += result.Count;
                            if (read < bytes.Length) continue;
                            var _bytes = ArrayPool<byte>.Shared.Rent(bytes.Length + 1024);
                            Buffer.BlockCopy(bytes, 0, _bytes, 0, bytes.Length);
                            ArrayPool<byte>.Shared.Return(bytes);
                            bytes = _bytes;
                        }
                    }
                    if (string.IsNullOrEmpty(text)) break;
                    UtilHelpers.Handle(text, messageReceived, noticeReceived, requestReceived, metaReceived, unknownReceived, onReply);
                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
        }

        public async Task<ReplyResult> SendAsync(string action, object data)
        {
            var (json, id) = UtilHelpers.BuildWebSocketJson(action, data);
            var bytes = Encoding.UTF8.GetBytes(json);
            var task = this.WaitForReply(id);
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            return await task;
        }

        public void Dispose()
        {
            if (socket.State == WebSocketState.Open) socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait();
            socket.Dispose();
        }


        public IObservable<MessageReceiver> MessageReceived => messageReceived.AsObservable();

        public IObservable<NoticeReceiver> NoticeReceived => noticeReceived.AsObservable();

        public IObservable<RequestReceiver> RequestReceived => requestReceived.AsObservable();

        public IObservable<MetaReceiver> MetaReceived => metaReceived.AsObservable();

        public IObservable<UnknownReceiver> UnknownReceived => unknownReceived.AsObservable();

        public IDictionary<string, Action<ReplyResult>> OnReply => onReply;
    }
}
