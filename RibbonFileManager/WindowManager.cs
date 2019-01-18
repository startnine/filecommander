using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WindowsSharp.DiskItems;

namespace RibbonFileManager
{
    public static class WindowManager
    {
        public static string WindowDefaultPath = @"%userprofile%";

        public static List<MainWindow> OpenWindows = new List<MainWindow>();

        public static void CreateWindow()
        {
            CreateWindow(WindowDefaultPath);
        }

        public static void CreateWindow(string targetPath)
        {
            string path = Environment.ExpandEnvironmentVariables(targetPath);
            bool found = false;
            if (path != Environment.ExpandEnvironmentVariables(WindowDefaultPath))
            {
                foreach (MainWindow w in OpenWindows)
                {
                    if (w.FileManagerControl.CurrentPath == path)
                    {
                        w.Show();
                        w.Focus();
                        w.Activate();
                        found = true;
                    }

                    if (found)
                        break;
                }
            }

            if (!found)
            {
                var win = new MainWindow(path);
                win.Show();
                win.Focus();
                win.Activate();
            }
        }

        public static void CloneWindow(MainWindow targetWindow)
        {
            var win = new MainWindow();

            win.FileManagerControl.HistoryList = targetWindow.FileManagerControl.HistoryList;
            win.FileManagerControl.HistoryIndex = targetWindow.FileManagerControl.HistoryIndex;

            win.Show();
            win.Focus();
            win.Activate();
        }

        /*static Manager()
        {
            
            Favorites.Add(new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\Documents")));
            Favorites.Add(new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\Pictures")));
            Favorites.Add(new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\Downloads")));
        }*/
    }
}
