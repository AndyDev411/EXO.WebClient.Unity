using System;

public class ExoSystemHostMessageHandlerAttribute : Attribute
{
    public int handlerID;

    public ExoSystemHostMessageHandlerAttribute(int _handlerID)
    {
        handlerID = _handlerID;
    }
}
