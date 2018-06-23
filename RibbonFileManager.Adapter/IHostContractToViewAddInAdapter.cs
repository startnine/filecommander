using RibbonFileManager.View;
using Start9.Api.Contracts;
using System.AddIn.Pipeline;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RibbonFileManager.Adapter
{
    public class IHostContractToViewAddInAdapter : IHost
    {
        private IHostContract _contract;
        private ContractHandle _handle;
        static IHostContractToViewAddInAdapter()
        {
        }
        public IHostContractToViewAddInAdapter(IHostContract contract)
        {
            _contract = contract;
            _handle = new ContractHandle(contract);
        }
        public void SendGlobalMessage(IMessage message)
        {
            _contract.SendGlobalMessage(IMessageAddInAdapter.ViewToContractAdapter(message));
        }
        public IList<IModule> GetModules()
        {
            return CollectionAdapters.ToIList(_contract.GetModules(), IModuleAddInAdapter.ContractToViewAdapter, IModuleAddInAdapter.ViewToContractAdapter);
        }
        public IConfiguration GetConfiguration(IModule module)
        {
            return IConfigurationAddInAdapter.ContractToViewAdapter(_contract.GetConfiguration(IModuleAddInAdapter.ViewToContractAdapter(module)));
        }
        internal IHostContract GetSourceContract()
        {
            return _contract;
        }
    }
}

