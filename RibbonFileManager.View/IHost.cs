using RibbonFileManager.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RibbonFileManager.View
{
    public interface IHost
    {
        void SendGlobalMessage(IMessage message);
        IList<IModule> GetModules();
        IConfiguration GetConfiguration(IModule module);
    }
}
