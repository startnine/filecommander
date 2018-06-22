using RibbonFileManager.View;
using Start9.Api.Plex;
using System;
using System.AddIn;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;

namespace RibbonFileManager
{
    [AddIn("Ribbon File Manager", Description = "Ribbon Test", Publisher = "Start9", Version = "1.0.0.0")]
    public class RibbonFileManagerAddIn : IModule
    {
        public IMessage SendMessage(IMessage message)
        {
            System.Windows.MessageBox.Show(message.Text);
            return Message.Empty;
        }

        public RibbonFileManagerAddIn()
        {
            void Start()
            {
                Application.ResourceAssembly = Assembly.GetExecutingAssembly();
                App.Main();
            }

            var t = new Thread(Start);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }
    }
}
