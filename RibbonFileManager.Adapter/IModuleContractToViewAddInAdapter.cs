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

