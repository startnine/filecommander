using RibbonFileManager.View;
using Start9.Api.Contracts;
using System.AddIn.Pipeline;
using System.Collections;

namespace RibbonFileManager.Adapter
{
    public class IHostViewToContractAddInAdapter : ContractBase, IHostContract
    {
        private IHost _view;
        public IHostViewToContractAddInAdapter(IHost view)
        {
            _view = view;
        }
        public virtual void SendGlobalMessage(IMessageContract message)
        {
            _view.SendGlobalMessage(IMessageAddInAdapter.ContractToViewAdapter(message));
        }
        public virtual System.AddIn.Contract.IListContract<IModuleContract> GetModules()
        {
            return CollectionAdapters.ToIListContract(_view.GetModules(), IModuleAddInAdapter.ViewToContractAdapter, IModuleAddInAdapter.ContractToViewAdapter);
        }
        public virtual IConfigurationContract GetConfiguration(IModuleContract module)
        {
            return IConfigurationAddInAdapter.ViewToContractAdapter(_view.GetConfiguration(IModuleAddInAdapter.ContractToViewAdapter(module)));
        }
        internal IHost GetSourceView()
        {
            return _view;
        }
    }
}

