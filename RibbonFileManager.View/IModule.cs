using System.AddIn.Pipeline;

namespace RibbonFileManager.View
{
    [AddInBase]
    public interface IModule
    {
        IMessage SendMessage(IMessage message);
        IConfiguration Configuration
        {
            get;
        }
        void HostReceived(IHost host);
    }
}

