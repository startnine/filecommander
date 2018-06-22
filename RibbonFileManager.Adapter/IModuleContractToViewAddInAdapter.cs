using RibbonFileManager.View;
using Start9.Api.Contracts;
using System.AddIn.Pipeline;

namespace RibbonFileManager.Adapter
{
    public class IModuleContractToViewAddInAdapter : IModule
    {
        private IModuleContract _contract;
        private ContractHandle _handle;

        static IModuleContractToViewAddInAdapter()
        {
        }

        public IModuleContractToViewAddInAdapter(IModuleContract contract)
        {
            _contract = contract;
            _handle = new ContractHandle(contract);
        }

        public IConfiguration Configuration
        {
            get
            {
                return IConfigurationAddInAdapter.ContractToViewAdapter(_contract.Configuration);
            }
        }

        public void HostReceived(IHost host)
        {
            _contract.HostReceived(IHostAddInAdapter.ViewToContractAdapter(host));
        }

        public IMessage SendMessage(IMessage message)
        {
            return IMessageAddInAdapter.ContractToViewAdapter(_contract.SendMessage(IMessageAddInAdapter.ViewToContractAdapter(message)));
        }

        internal IModuleContract GetSourceContract()
        {
            return _contract;
        }
    }
}

