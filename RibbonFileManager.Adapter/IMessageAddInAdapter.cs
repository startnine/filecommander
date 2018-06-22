using RibbonFileManager.View;
using Start9.Api.Contracts;
using System.Runtime.Remoting;

namespace RibbonFileManager.Adapter
{    
    public class IMessageAddInAdapter
    {
        internal static IMessage ContractToViewAdapter(IMessageContract contract)
        {
            if (contract == null)
            {
                return null;
            }

            if (RemotingServices.IsObjectOutOfAppDomain(contract) != true && contract is IMessageViewToContractAddInAdapter)
            {
                return ((IMessageViewToContractAddInAdapter) contract).GetSourceView();
            }
            else
            {
                return new IMessageContractToViewAddInAdapter(contract);
            }
        }

        internal static IMessageContract ViewToContractAdapter(IMessage view)
        {
            if (view == null)
            {
                return null;
            }

            if (view is IMessageContractToViewAddInAdapter)
            {
                return ((IMessageContractToViewAddInAdapter) view).GetSourceContract();
            }
            else
            {
                return new IMessageViewToContractAddInAdapter(view);
            }
        }
    }
}

