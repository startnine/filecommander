using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RibbonFileManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            /*for (int i = 0; i <  Resources.MergedDictionaries.Count; i++)
            {
                ResourceDictionary dictionary = Resources.MergedDictionaries.ElementAt(i);
                Resources.MergedDictionaries.Remove(dictionary);
                Resources.MergedDictionaries.Add(dictionary);
            }*/
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //TEMPORARY FOR OBVIOUS REASONS
            /*Resources.MergedDictionaries.Add(Start9.Wpf.Styles.Shale.ShaleAccents.Blue.Dictionary);
            Resources.MergedDictionaries.Insert(0, new ResourceDictionary()
            {
                Source = new Uri("/RibbonFileManager;component/Shale.xaml", UriKind.RelativeOrAbsolute)
            });*/
            // In case of data binding error, break glass (uncomment)

            //PresentationTraceSources.Refresh();
            //PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
            //PresentationTraceSources.DataBindingSource.Listeners.Add(new DebugTraceListener());
            //PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;
            base.OnStartup(e);
            Resources.MergedDictionaries.Insert(0, Start9.Wpf.Styles.Shale.ShaleAccents.Blue.Dictionary);
        }
    }

    public class DebugTraceListener : TraceListener
    {
        public override void Write(String message)
        {

        }

        public override void WriteLine(String message)
        {
            Debugger.Break();
        }
    }
}
