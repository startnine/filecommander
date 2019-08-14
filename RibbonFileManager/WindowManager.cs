using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RibbonFileManager
{
    public static class WindowManager
    {
        public static Location WindowDefaultLocation = new ShellLocation(new Guid("20D04FE0-3AEA-1069-A2D8-08002B30309D")); //Environment.ExpandEnvironmentVariables(@"%userprofile%");

        public static ObservableCollection<MainWindow> OpenWindows = new ObservableCollection<MainWindow>();
    }
}
