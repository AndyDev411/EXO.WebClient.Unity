using UnityEngine;
using EXO.Networking.Common;
using MikeSchweitzer.WebSocket;
using System;

public class SimpleWsConnection : IConnection
{

    WebSocketConnection connection;
    private string url;
    public SimpleWsConnection(string url, WebSocketConnection connection)
    {
        this.url = url;
        this.connection = connection;
    }

    public bool Connected => throw new NotImplementedException();

    public event Action<byte[]> OnRecieveEvent;
    public event Action OnConnectionStartEvent;

    public void Connect()
    {
        connection.MessageReceived += OnConnectionRecievedHandler;
        connection.StateChanged += OnStateChangeEvent;
        connection.Connect(url);

    }

    private void OnStateChangeEvent(WebSocketConnection connection, WebSocketState oldState, WebSocketState newState)
    {
        if (newState == WebSocketState.Connected)
        {
            OnConnectionStartEvent?.Invoke();
        }
    }

    private void OnConnectionRecievedHandler(WebSocketConnection connection, WebSocketMessage message)
    {
        OnRecieveEvent?.Invoke(message.Bytes);
    }

    public void DisconnectAsync()
    {
        connection.MessageReceived -= OnConnectionRecievedHandler;
        connection.Disconnect();
    }

    public void Dispose()
    {

    }

    public void OnUpdate()
    {
        
    }

    public void Send(byte[] toSend)
    {
        connection.AddOutgoingMessage(toSend);
    }

    public void Send(string toSend)
    {
        connection.AddOutgoingMessage(toSend);
    }
}
