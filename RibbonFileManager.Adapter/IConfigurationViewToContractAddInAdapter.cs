using RibbonFileManager.View;
using Start9.Api.Contracts;
using System.AddIn.Pipeline;
using System.Collections;

namespace RibbonFileManager.Adapter
{
    public class IConfigurationViewToContractAddInAdapter : ContractBase, IConfigurationContract
    {
        private IConfiguration _view;
        public IConfigurationViewToContractAddInAdapter(IConfiguration view)
        {
            _view = view;
        }

        public IDictionary Entries
        {
            get
            {
                return _view.Entries;
            }
        }

        internal IConfiguration GetSourceView()
        {
            return _view;
        }
    }
}

