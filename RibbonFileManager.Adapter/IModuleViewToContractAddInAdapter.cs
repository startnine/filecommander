using RibbonFileManager.View;
using Start9.Api.Contracts;
using System.AddIn.Pipeline;

namespace RibbonFileManager.Adapter
{
    [AddInAdapter]
    public class IModuleViewToContractAddInAdapter : ContractBase, IModuleContract
    {
        private IModule _view;

        public IModuleViewToContractAddInAdapter(IModule view)
        {
            _view = view;
        }
        public IConfigurationContract Configuration
        {
            get
            {
                return IConfigurationAddInAdapter.ViewToContractAdapter(_view.Configuration);
            }
        }

        public void HostReceived(IHostContract host)
        {
            _view.HostReceived(IHostAddInAdapter.ContractToViewAdapter(host));
        }

        public virtual IMessageContract SendMessage(IMessageContract message)
        {
            return IMessageAddInAdapter.ViewToContractAdapter(_view.SendMessage(IMessageAddInAdapter.ContractToViewAdapter(message)));
        }

        internal IModule GetSourceView()
        {
            return _view;
        }
    }
}

