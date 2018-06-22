using System;

namespace RibbonFileManager.View
{
    public interface IMessage
    {
        String Text { get; }
        Object Object { get; }
    }
}

