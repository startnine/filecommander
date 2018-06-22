using System.AddIn.Pipeline;

namespace RibbonFileManager.View
{
    [AddInBase]
    public interface IModule
    {
        IMessage SendMessage(IMessage message);
    }
}

