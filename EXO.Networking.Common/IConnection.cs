using System;

namespace EXO.Networking.Common
{
    public interface IConnection : IDisposable
    {

        public event Action<byte[]> OnRecieveEvent;
        public event Action OnConnectionStartEvent;
        public event Action OnDisconnectedEvent;

        public void Send(byte[] toSend);

        public void Send(string toSend);

        public void DisconnectAsync();

        public bool Connected { get; }

        public void OnUpdate();

        public void Connect();
    }
}


