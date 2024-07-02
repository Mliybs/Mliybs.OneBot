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
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;

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

        Task<ReplyResult> SendAsync(string action, object data);
    }

    internal class WebsocketOneBotHandler : IOneBotHandler
    {
        private CancellationTokenSource source = new();

        private ClientWebSocket socket = new();

        private readonly Subject<MessageReceiver> messageReceived = new();

        private readonly Subject<NoticeReceiver> noticeReceived = new();

        private readonly Subject<RequestReceiver> requestReceived = new();

        private readonly Subject<MetaReceiver> metaReceived = new();

        private readonly Subject<UnknownReceiver> unknownReceived = new();

        private readonly ConcurrentDictionary<string, Action<ReplyResult>> onReply = new();

        public WebsocketOneBotHandler(Uri uri, string? token = null)
        {
            if (token is not null) socket.Options.SetRequestHeader("Authorization", "Bearer " + token);
            socket.ConnectAsync(uri, CancellationToken.None).GetAwaiter().GetResult();
            _ = Run();
        }

        public WebsocketOneBotHandler(Uri uri, out Action reconnect, string? token = null) : this(uri, token)
        {
            reconnect = () =>
            {
                source.Cancel();
                source.Dispose();
                source = new();
                socket.Abort();
                socket.Dispose();
                socket = new();
                socket.ConnectAsync(uri, CancellationToken.None).GetAwaiter().GetResult();
                _ = Run();
            };
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
                        var result = await socket.ReceiveAsync(bytes.AsMemory(read), source.Token).ConfigureAwait(false);
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
                // 捕获OperationCanceled和TaskCanceled（如果有的话）
                catch (OperationCanceledException)
                {
                    break;
                }
                // 捕获SocketException Operation Canceled
                catch (SocketException e) when (e.ErrorCode == 125)
                {
                    break;
                }
                // 捕获包装异常
                catch (Exception e) when (e.InnerException is SocketException ex && ex.ErrorCode == 125)
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
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, source.Token).ConfigureAwait(false);
            return await task.ConfigureAwait(false);
        }

        public void Dispose()
        {
            source.Cancel();
            source.Dispose();
            socket.Abort();
            socket.Dispose();
        }

        public IObservable<MessageReceiver> MessageReceived => messageReceived.AsObservable();

        public IObservable<NoticeReceiver> NoticeReceived => noticeReceived.AsObservable();

        public IObservable<RequestReceiver> RequestReceived => requestReceived.AsObservable();

        public IObservable<MetaReceiver> MetaReceived => metaReceived.AsObservable();

        public IObservable<UnknownReceiver> UnknownReceived => unknownReceived.AsObservable();

        public IDictionary<string, Action<ReplyResult>> OnReply => onReply;
    }

    internal class HttpOneBotHandler : IOneBotHandler
    {
        private readonly string url;

        private readonly string local;

        private readonly HttpClient client = new();

        private readonly HttpListener listener = new();

        private readonly Subject<MessageReceiver> messageReceived = new();

        private readonly Subject<NoticeReceiver> noticeReceived = new();

        private readonly Subject<RequestReceiver> requestReceived = new();

        private readonly Subject<MetaReceiver> metaReceived = new();

        private readonly Subject<UnknownReceiver> unknownReceived = new();

        private readonly ConcurrentDictionary<string, Action<ReplyResult>> onReply = new();

        public HttpOneBotHandler(string _url, string _local, string? token = null)
        {
            url = _url;
            if (token is not null) client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            local = _local;
            var builder = new UriBuilder(_local)
            {
                Path = "/"
            };
            listener.Prefixes.Add(builder.ToString());
            _ = ListenerRunAsync();
        }

        public async Task<ReplyResult> SendAsync(string action, object data)
        {
            var builder = new UriBuilder(url)
            {
                Path = action
            };
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
            var res = await client.PostAsync(builder.Uri, new ByteArrayContent(bytes)).ConfigureAwait(false);
            var json = JsonDocument.Parse(await res.Content.ReadAsStringAsync().ConfigureAwait(false));
            var result = json.Deserialize<ReplyResult>(UtilHelpers.Options)!;
            result.Data = json.RootElement.GetProperty("data");
            return result;
        }

        public async Task ListenerRunAsync()
        {
            listener.Start();
            while (listener.IsListening)
            {
                var context = await listener.GetContextAsync().ConfigureAwait(false);
                if (context.Request.HttpMethod != "POST" || context.Request.RawUrl != new Uri(local).AbsolutePath)
                {
                    context.Response.StatusCode = 404;
                    context.Response.Close();
                    continue;
                }
                var bytes = ArrayPool<byte>.Shared.Rent(1024);
                int read = 0;
                string text;
                while (true)
                {
                    var length = await context.Request.InputStream.ReadAsync(bytes.AsMemory(read)).ConfigureAwait(false);
                    if (length == 0)
                    {
                        text = Encoding.UTF8.GetString(bytes.AsSpan(0, read + length));
                        ArrayPool<byte>.Shared.Return(bytes);
                        break;
                    }
                    else
                    {
                        read += length;
                        if (read < bytes.Length) continue;
                        var _bytes = ArrayPool<byte>.Shared.Rent(bytes.Length + 1024);
                        Buffer.BlockCopy(bytes, 0, _bytes, 0, bytes.Length);
                        ArrayPool<byte>.Shared.Return(bytes);
                        bytes = _bytes;
                    }
                }
                if (string.IsNullOrEmpty(text)) break;
                UtilHelpers.Handle(text, messageReceived, noticeReceived, requestReceived, metaReceived, unknownReceived, onReply);
                context.Response.StatusCode = 204;
                context.Response.Close();
            }
        }

        public void Dispose()
        {
            client.Dispose();
            listener.Stop();
            ((IDisposable)listener).Dispose();
        }

        public IObservable<MessageReceiver> MessageReceived => messageReceived.AsObservable();

        public IObservable<NoticeReceiver> NoticeReceived => noticeReceived.AsObservable();

        public IObservable<RequestReceiver> RequestReceived => requestReceived.AsObservable();

        public IObservable<MetaReceiver> MetaReceived => metaReceived.AsObservable();

        public IObservable<UnknownReceiver> UnknownReceived => unknownReceived.AsObservable();

        public IDictionary<string, Action<ReplyResult>> OnReply => onReply;
    }

    internal class WebsocketReverseOneBotHandler : IOneBotHandler
    {
        private CancellationTokenSource source = new();

        private WebSocket? socket = null;

        private readonly string url;

        private readonly HttpListener listener = new();

        private readonly Subject<MessageReceiver> messageReceived = new();

        private readonly Subject<NoticeReceiver> noticeReceived = new();

        private readonly Subject<RequestReceiver> requestReceived = new();

        private readonly Subject<MetaReceiver> metaReceived = new();

        private readonly Subject<UnknownReceiver> unknownReceived = new();

        private readonly ConcurrentDictionary<string, Action<ReplyResult>> onReply = new();

        public WebsocketReverseOneBotHandler(string _url, string? token = null)
        {
            url = _url;
            var builder = new UriBuilder(url)
            {
                Path = "/"
            };
            if (builder.Scheme == "wss") builder.Scheme = "https";
            if (builder.Scheme == "ws") builder.Scheme = "http";
            listener.Prefixes.Add(builder.ToString());
            _ = Run(token);
        }

        public WebsocketReverseOneBotHandler(string _url, out Func<bool> connected, string? token = null) : this(_url, token)
        {
            connected = () => socket?.State == WebSocketState.Open;
        }

        public async Task Run(string? token = null)
        {
            listener.Start();
            while (listener.IsListening)
            {
                var context = await listener.GetContextAsync().ConfigureAwait(false);
                if (!context.Request.IsWebSocketRequest)
                {
                    context.Response.StatusCode = 404;
                    context.Response.Close();
                    continue;
                }
                if (token is not null)
                {
                    var values = context.Request.Headers.GetValues("Authorization");
                    if (values is null || values.Length == 0)
                    {
                        context.Response.StatusCode = 404;
                        context.Response.Close();
                        continue;
                    }
                    var contain = true;
                    foreach (var value in values) if (value.Remove(0, 7) == token) contain = false;
                    if (contain)
                    {
                        context.Response.StatusCode = 404;
                        context.Response.Close();
                        continue;
                    }
                }
                if (context.Request.RawUrl != new Uri(url).AbsolutePath)
                {
                    context.Response.StatusCode = 404;
                    context.Response.Close();
                    continue;
                }
                socket = (await context.AcceptWebSocketAsync(null).ConfigureAwait(false)).WebSocket;
                while (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        var bytes = ArrayPool<byte>.Shared.Rent(1024);
                        int read = 0;
                        string text;
                        while (true)
                        {
                            var result = await socket.ReceiveAsync(bytes.AsMemory(read), source.Token).ConfigureAwait(false);
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
                socket.Dispose();
                socket = null;
            }
        }

        public async Task<ReplyResult> SendAsync(string action, object data)
        {
            var (json, id) = UtilHelpers.BuildWebSocketJson(action, data);
            var bytes = Encoding.UTF8.GetBytes(json);
            var task = this.WaitForReply(id);
            await socket!.SendAsync(bytes, WebSocketMessageType.Text, true, source.Token).ConfigureAwait(false);
            return await task.ConfigureAwait(false);
        }

        public void Dispose()
        {
            socket?.Dispose();
            listener.Stop();
            ((IDisposable)listener).Dispose();
        }

        public IObservable<MessageReceiver> MessageReceived => messageReceived.AsObservable();

        public IObservable<NoticeReceiver> NoticeReceived => noticeReceived.AsObservable();

        public IObservable<RequestReceiver> RequestReceived => requestReceived.AsObservable();

        public IObservable<MetaReceiver> MetaReceived => metaReceived.AsObservable();

        public IObservable<UnknownReceiver> UnknownReceived => unknownReceived.AsObservable();

        public IDictionary<string, Action<ReplyResult>> OnReply => onReply;
    }
}
