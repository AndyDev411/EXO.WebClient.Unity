using UnityEngine;
using EXO.WebClient;
using EXO.Networking.Common;

public class SimpleConnectionFactory : MonoBehaviour, IConnectionFactory
{

    public IConnection CreateConnection()
    {
        var connection = this.GetComponent<IConnection>();

        return connection;
    }
}

