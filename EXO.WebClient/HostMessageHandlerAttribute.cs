using System;

namespace EXO.WebClient
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HostMessageHandlerAttribute : Attribute
    {

        public int HandlerID { get; private set; }

        public HostMessageHandlerAttribute(int _handlerID)
        { 
            HandlerID = _handlerID; 
        }

    }
}
