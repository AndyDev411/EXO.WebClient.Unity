using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
