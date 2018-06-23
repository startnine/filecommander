using RibbonFileManager.View;
using Start9.Api.Contracts;
using System.Runtime.Remoting;

namespace RibbonFileManager.Adapter
{
    public class IHostAddInAdapter
    {
        internal static IHost ContractToViewAdapter(IHostContract contract)
        {
            if (contract == null)
            {
                return null;
            }
            if (RemotingServices.IsObjectOutOfAppDomain(contract) != true && contract is IHostViewToContractAddInAdapter)
            {
                return ((IHostViewToContractAddInAdapter) contract).GetSourceView();
            }
            else
            {
                return new IHostContractToViewAddInAdapter(contract);
            }
        }

        internal static IHostContract ViewToContractAdapter(IHost view)
        {
            if (view == null)
            {
                return null;
            }

            if (view is IHostContractToViewAddInAdapter)
            {
                return ((IHostContractToViewAddInAdapter) view).GetSourceContract();
            }
            else
            {
                return new IHostViewToContractAddInAdapter(view);
            }
        }
    }
}

