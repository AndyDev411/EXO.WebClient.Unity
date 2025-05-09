using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EXO.Networking.Common;

using UnityEngine;

public class WinWebSocketConnection : MonoBehaviour, IConnection
{
    public string serverUrl = "ws://play.theboizgaming.com:8081/ws";

    private ClientWebSocket _ws;
    private Uri _uri;
    private CancellationTokenSource _cts;
    private readonly ConcurrentQueue<byte[]> _receiveQueue = new();

    public event Action<byte[]> OnRecieveEvent;
    public event Action OnConnectionStartEvent;
    public event Action OnDisconnectedEvent;

    public bool Connected => _ws != null && _ws.State == WebSocketState.Open;

    private void Start()
    {
        _uri = new Uri(serverUrl);
    }

    public void Connect()
    {
        if (Connected)
            throw new InvalidOperationException("Already connected.");

        _cts = new CancellationTokenSource();
        _ws = new ClientWebSocket();

        // fire-and-forget the connect + receive loop
        _ = ConnectAndReceiveLoopAsync(_cts.Token);
    }

    private async Task ConnectAndReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            await _ws.ConnectAsync(_uri, ct).ConfigureAwait(false);
            OnConnectionStartEvent?.Invoke();

            var buffer = new byte[4096];
            while (!ct.IsCancellationRequested && _ws.State == WebSocketState.Open)
            {
                var seg = new ArraySegment<byte>(buffer);
                WebSocketReceiveResult result = await _ws.ReceiveAsync(seg, ct).ConfigureAwait(false);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct).ConfigureAwait(false);
                    break;
                }

                int count = result.Count;
                // handle partial frames
                while (!result.EndOfMessage)
                {
                    if (count >= buffer.Length)
                        throw new Exception("Message too long for buffer");
                    seg = new ArraySegment<byte>(buffer, count, buffer.Length - count);
                    result = await _ws.ReceiveAsync(seg, ct).ConfigureAwait(false);
                    count += result.Count;
                }

                // enqueue the complete message
                var msg = new byte[count];
                Array.Copy(buffer, msg, count);
                _receiveQueue.Enqueue(msg);
            }
        }
        catch (OperationCanceledException) { OnDisconnectedEvent?.Invoke(); }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[EditorConnection] Receive loop error: {ex}");
            OnDisconnectedEvent?.Invoke();
        }
    }

    public void Send(byte[] toSend)
    {
        if (!Connected) throw new InvalidOperationException("Not connected.");
        var buf = new ArraySegment<byte>(toSend);
        _ = _ws.SendAsync(buf, WebSocketMessageType.Binary, true, CancellationToken.None)
               .ContinueWith(t =>
               {
                   if (t.Exception != null)
                       UnityEngine.Debug.LogError($"[EditorConnection] Send error: {t.Exception}");
               });
    }

    public void Send(string toSend)
    {
        var bytes = Encoding.UTF8.GetBytes(toSend);
        var buf = new ArraySegment<byte>(bytes);
        _ = _ws.SendAsync(buf, WebSocketMessageType.Text, true, CancellationToken.None)
               .ContinueWith(t =>
               {
                   if (t.Exception != null)
                       UnityEngine.Debug.LogError($"[EditorConnection] Send error: {t.Exception}");
               });
    }

    public void DisconnectAsync()
    {
        if (_ws == null) return;
        _cts.Cancel();
        _ = _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", CancellationToken.None)
               .ContinueWith(_ => { /* ignored */ });
    }

    public void OnUpdate()
    {
        while (_receiveQueue.TryDequeue(out var msg))
        {
            OnRecieveEvent?.Invoke(msg);
        }
    }

    public void Dispose()
    {
        DisconnectAsync();
        _ws?.Dispose();
        _cts?.Dispose();
        _receiveQueue.Clear();
        OnRecieveEvent = null;
        OnConnectionStartEvent = null;
    }
}
