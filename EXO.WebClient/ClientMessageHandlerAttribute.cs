using System;

namespace EXO.WebClient
{

    [AttributeUsage(AttributeTargets.Method)]
    public class ClientMessageHandlerAttribute : Attribute
    {

        public int HandlerID { get; private set; }

        public ClientMessageHandlerAttribute(int _handlerID)
        {
            HandlerID = _handlerID;
        }

    }
}
