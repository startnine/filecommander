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
        public static String WindowDefaultPath = Environment.ExpandEnvironmentVariables(@"%userprofile%");

        public static List<MainWindow> OpenWindows = new List<MainWindow>();

        public static MainWindow CreateWindow()
        {
            return CreateWindow(WindowDefaultPath);
        }

        public static MainWindow CreateWindow(String targetPath)
        {
            var path = Environment.ExpandEnvironmentVariables(targetPath);
            var found = false;

            if (!found)
            {
                var win = new MainWindow(path);
                win.Show();
                win.Focus();
                win.Activate();
                return win;
            }
            else
                return null;
        }

        public static void CloneWindow(MainWindow targetWindow)
        {
            var win = new MainWindow(targetWindow);

            win.Show();
            win.Focus();
            win.Activate();
        }
    }
}
