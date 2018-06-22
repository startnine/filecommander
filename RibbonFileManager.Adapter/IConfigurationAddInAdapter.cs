using RibbonFileManager.View;
using Start9.Api.Contracts;
using System.Runtime.Remoting;

namespace RibbonFileManager.Adapter
{
    public class IConfigurationAddInAdapter
    {
        internal static IConfiguration ContractToViewAdapter(IConfigurationContract contract)
        {
            if (contract == null)
            {
                return null;
            }
            if (RemotingServices.IsObjectOutOfAppDomain(contract) != true && contract is IConfigurationViewToContractAddInAdapter)
            {
                return ((IConfigurationViewToContractAddInAdapter) contract).GetSourceView();
            }
            else
            {
                return new IConfigurationContractToViewAddInAdapter(contract);
            }
        }

        internal static IConfigurationContract ViewToContractAdapter(IConfiguration view)
        {
            if (view == null)
            {
                return null;
            }

            if (view is IConfigurationContractToViewAddInAdapter)
            {
                return ((IConfigurationContractToViewAddInAdapter) view).GetSourceContract();
            }
            else
            {
                return new IConfigurationViewToContractAddInAdapter(view);
            }
        }
    }
}

