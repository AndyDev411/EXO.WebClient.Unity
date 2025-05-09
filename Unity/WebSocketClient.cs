// File: Scripts/WebSocketClient.cs
using System;
using System.Runtime.InteropServices;
using UnityEngine;

using EXO.Networking.Common;

public class WebSocketClient : MonoBehaviour, IConnection
{

    public string serverUrl;

    [DllImport("__Internal")]
    private static extern void WS_Create(string url, string receiverObject, string onMessageRecievedCallback, string onConnectCallback, string onSocketClosedCallback);

    [DllImport("__Internal")]
    private static extern void WS_Send(IntPtr data, int length);

    [DllImport("__Internal")]
    private static extern void WS_Close();

    // Ensure GameObject name matches this script's
    private string ReceiverName => this.name;
    
    private const string ON_MESSAGE_RECIEVED_CALLBACK = "OnWebSocketMessage";
    private const string ON_CONNECTION_CALLBACK = "OnWebSocketConnect";
    private const string ON_CONNECTION_CLOSE_CALLBACK = "OnWebSocketClose";

    public bool Connected => throw new NotImplementedException();

    public event Action<byte[]> OnRecieveEvent;
    public event Action OnConnectionStartEvent;
    public event Action OnDisconnectedEvent;

    // Initialize WebSocket connection
    public void Connect(string url)
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        if (string.IsNullOrEmpty(url)) {
            Debug.LogError("WebSocketClient.Connect: URL is null or empty");
            return;
        }
        WS_Create(url, ReceiverName, ON_MESSAGE_RECIEVED_CALLBACK, ON_CONNECTION_CALLBACK, ON_CONNECTION_CLOSE_CALLBACK);
#endif
    }

    // Send raw byte array
    public void SendBytes(byte[] data)
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        if (data == null || data.Length == 0) {
            Debug.LogError("WebSocketClient.SendBytes: data is null or empty");
            return;
        }
        var ptr = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, ptr, data.Length);
        WS_Send(ptr, data.Length);
        Marshal.FreeHGlobal(ptr);
#endif
    }

    // Close the WebSocket
    public void Close()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        WS_Close();
#endif
    }

    public void SendTextToServer(string msg)
    {
        SendBytes(System.Text.Encoding.UTF8.GetBytes(msg));
    }

    /// <summary>
    /// Function that is called when data is recieved from the server.
    /// </summary>
    /// <param name="base64Data"> Base 64 string. </param>
    public void OnWebSocketMessage(string base64Data)
    {
        if (string.IsNullOrEmpty(base64Data))
        {
            Debug.LogWarning("OnWebSocketMessage: received empty message");
            return;
        }
        try
        {
            // Decode the bytes...
            var bytes = Convert.FromBase64String(base64Data);

            // Call the on recieved...
            OnRecieveEvent?.Invoke(bytes);
        }
        catch (Exception ex)
        {
            Debug.LogError($"OnWebSocketMessage: invalid base64 data - {ex}");
        }
    }

    /// <summary>
    /// Function used to notify Unity when the client has connected;
    /// </summary>
    public void OnWebSocketConnect()
    {
        OnConnectionStartEvent?.Invoke();
    }

    /// <summary>
    /// Function used to notify Unity when the client has been disconnected.
    /// </summary>
    public void OnWebSocketClose()
    {
        OnDisconnectedEvent?.Invoke();
    }

    #region IConnection

    public void Send(byte[] toSend)
    {
        SendBytes(toSend);
    }

    public void Send(string toSend)
    {
        Send(System.Text.Encoding.UTF8.GetBytes(toSend));
    }

    public void DisconnectAsync()
    {
        WS_Close();
    }

    public void OnUpdate()
    {

    }

    public void Connect()
    {
        Connect(serverUrl);
    }

    public void Dispose()
    {
        DisconnectAsync();
    }

    #endregion
}
