using UnityEngine;
using EXO.WebClient;
using EXO.Networking.Common;

public class NativeWebsocketConnectionFactory : MonoBehaviour, IConnectionFactory
{

    public string serverUrl = "wss://play.theboizgaming.com:8080/ws";

    public IConnection CreateConnection()
    {
        return new NativeWebSocketConnetion(serverUrl);
    }
}
