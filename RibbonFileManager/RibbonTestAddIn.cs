using RibbonFileManager.Views;
using System;
using System.AddIn;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace RibbonFileManager
{
    [AddIn("Ribbon File Manager", Description = "Experimental ribbon-based file manager", Version = "1.0.0.0", Publisher = "Start9")]
    public class RibbonFileManagerAddIn : IModule
    {
        public static RibbonFileManagerAddIn Instance { get; private set; }

        public IConfiguration Configuration { get; } = new RibbonFileManagerConfiguration();

        public IMessageContract MessageContract { get; } = new RibbonFileManagerMessageContract();

        public IReceiverContract ReceiverContract => null;

        public IHost Host { get; private set; }

        public void Initialize(IHost host)
        {
            void Start()
            {
                Application.ResourceAssembly = Assembly.GetExecutingAssembly();
                App.Main();
                Instance = this;
            }

            var t = new Thread(Start);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }
    }

    public class RibbonFileManagerMessageContract : IMessageContract
    {
        public IList<IMessageEntry> Entries => new[] { ButtonClickedEntry };

        public IMessageEntry ButtonClickedEntry { get; } = new MessageEntry(typeof(DBNull), "Start button clicked");
    }

    public class RibbonFileManagerConfiguration : IConfiguration
    {
        public IList<IConfigurationEntry> Entries => new[] { new ConfigurationEntry(GroupItems, "Group Items") };

        public Boolean GroupItems { get; set; } = true;
    }
}
