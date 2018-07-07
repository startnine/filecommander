using RibbonFileManager.Views;
using System;
using System.AddIn;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace RibbonFileManager
{
    [AddIn("Ribbon File Manager", Description = "The Windows 8 file manager - now with more plex!", Version = "1.0.0.0", Publisher = "Start9")]
    public class RibbonFileManagerAddIn : IModule
    {
        public static RibbonFileManagerAddIn Instance { get; private set; }

        public IConfiguration Configuration { get; set; } = null;

        public IMessageContract MessageContract => null;

        public IReceiverContract ReceiverContract => new RibbonFileManagerReceiverContract();

        public IHost Host { get; private set; }

        public void Initialize(IHost host)
        {
            void Start()
            {
                Instance = this;
                Host = host;
                Application.ResourceAssembly = Assembly.GetExecutingAssembly();
                App.Main();
            }

            var t = new Thread(Start);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {

                MessageBox.Show(e.ExceptionObject.ToString(), "Uh oh E R R O R E");
            };
        }
    }
    public class RibbonFileManagerReceiverContract : IReceiverContract
    {
        public RibbonFileManagerReceiverContract()
        {
            OpenFolderEntry.MessageReceived += (sender, e) =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() => ((MainWindow) Application.Current.MainWindow).Navigate((String)e.Message.Object)));
            };
        }
        public IList<IReceiverEntry> Entries => new[] { OpenFolderEntry };

        public IReceiverEntry OpenFolderEntry { get; } = new ReceiverEntry(typeof(String), "Open folder");
    }
}
