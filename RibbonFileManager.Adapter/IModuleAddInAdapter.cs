using RibbonFileManager.View;
using Start9.Api.Contracts;
using System.Runtime.Remoting;

namespace RibbonFileManager.Adapter
{ 
    public class IModuleAddInAdapter
    {
        internal static IModule ContractToViewAdapter(IModuleContract contract)
        {
            if (contract == null)
            {
                return null;
            }

            if (RemotingServices.IsObjectOutOfAppDomain(contract) != true && contract is IModuleViewToContractAddInAdapter)
            {
                return ((IModuleViewToContractAddInAdapter) contract).GetSourceView();
            }
            else
            {
                return new IModuleContractToViewAddInAdapter(contract);
            }
        }

        internal static IModuleContract ViewToContractAdapter(IModule view)
        {
            if (view == null)
            {
                return null;
            }

            if (view is IModuleContractToViewAddInAdapter)
            {
                return ((IModuleContractToViewAddInAdapter) view).GetSourceContract();
            }
            else
            {
                return new IModuleViewToContractAddInAdapter(view);
            }
        }
    }
}

