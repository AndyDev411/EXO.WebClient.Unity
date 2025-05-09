using System;
using UnityEngine;

public class ExoSystemClientMessageHandlerAttribute : Attribute
{
    public int handlerID;

    public ExoSystemClientMessageHandlerAttribute(int _handlerID)
    {
        handlerID = _handlerID;
    }
}
