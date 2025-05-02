using Unity;
using UnityEngine;
using EXO.WebClient;
using EXO.Networking.Common;
using MikeSchweitzer.WebSocket;

public class SimpleWsConnectionFactory : MonoBehaviour, IConnectionFactory
{

    public string serverUrl = "wss://play.theboizgaming.com:8080/ws";

    public IConnection CreateConnection()
    {
        var con = this.GetComponent<WebSocketConnection>();

        return new SimpleWsConnection(serverUrl, con);
    }
}

