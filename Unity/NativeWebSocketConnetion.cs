using EXO.Networking.Common;
using UnityEngine;
using NativeWebSocket;
using System;

public class NativeWebSocketConnetion : IConnection
{

    private WebSocket socket;

    private bool connected = false;

    public NativeWebSocketConnetion(string url)
    {
        socket = new WebSocket(url);



        socket.OnOpen += () =>
        {
            connected = true;
        };

        socket.OnError += (e) =>
        {
            Debug.LogError(e);
        };

        socket.OnClose += (e) =>
        {
            connected = false;
        };

        socket.OnMessage += (bytes) =>
        {
            OnRecieveEvent?.Invoke(bytes);
        };

    }

    public bool Connected => throw new System.NotImplementedException();

    public event Action<byte[]> OnRecieveEvent;
    public event Action OnConnectionStartEvent;

    public async void Connect()
    {
        await socket.Connect();
    }

    public async void DisconnectAsync()
    {
        await socket.Close();
    }

    public async void Dispose()
    {
        await socket.Close();
    }

    public void OnUpdate()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (socket.State == WebSocketState.Open)
        {
            socket.DispatchMessageQueue();
        }
#endif
    }

    public void Send(byte[] toSend)
    {
        socket.Send(toSend).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public void Send(string toSend)
    {
        socket.Send(System.Text.Encoding.UTF8.GetBytes(toSend)).ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
