using RibbonFileManager.View;
using Start9.Api.Contracts;
using System.AddIn.Pipeline;
using System.Collections;

namespace RibbonFileManager.Adapter
{    
    public class IConfigurationContractToViewAddInAdapter : IConfiguration
    {
        private IConfigurationContract _contract;
        private ContractHandle _handle;

        static IConfigurationContractToViewAddInAdapter()
        {
        }

        public IConfigurationContractToViewAddInAdapter(IConfigurationContract contract)
        {
            _contract = contract;
            _handle = new ContractHandle(contract);
        }

        public IDictionary Entries
        {
            get
            {
                return _contract.Entries;
            }
        }

        internal IConfigurationContract GetSourceContract()
        {
            return _contract;
        }
    }
}

