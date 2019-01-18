using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WindowsSharp.DiskItems;

namespace RibbonFileManager
{
    public class Config : DependencyObject
    {
        public ObservableCollection<DiskItem> QuickDestinations { get; set; } = new ObservableCollection<DiskItem>()
        {
            new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\Desktop")),
            new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\Downloads")),
            new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\Documents")),
            new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\Music")),
            new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\Pictures")),
            new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\Videos"))
        };

        public static Config Instance = new Config();

        private Config()
        { }
    }
}
