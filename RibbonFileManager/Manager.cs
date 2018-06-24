using Start9.Api.DiskItems;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace RibbonFileManager
{
    public static class Manager
    {
        public static ObservableCollection<MainWindow> OpenWindows { get; set; } = new ObservableCollection<MainWindow>();
        public static ObservableCollection<DiskItem> Clipboard { get; set; } = new ObservableCollection<DiskItem>();
        public static Boolean Cut { get; set; } = false;

        public static ObservableCollection<DiskItem> CopyTo(String targetPath)
        {
            var success = new ObservableCollection<DiskItem>();
            foreach(var d in Clipboard)
            {
                try
                {
                    var baseFilename = targetPath + @"\" + Path.GetFileName(d.ItemPath);
                    var outFilename = baseFilename;

                    var interval = 1;

                    var ext = Path.GetExtension(baseFilename);
                    while ((File.Exists(outFilename)) | (Directory.Exists(outFilename)))
                    {
                        outFilename = baseFilename + " (" + interval.ToString() + ")";
                        if (!(String.IsNullOrEmpty(ext)))
                        {
                            if (!(outFilename.EndsWith(ext)))
                            {
                                if (outFilename.Contains(ext))
                                {
                                    outFilename = outFilename.Replace(ext, "");
                                    outFilename = outFilename + ext;
                                }
                            }
                        }
                        interval += 1;
                    }
                    Debug.WriteLine("outFilename: " + outFilename);

                    if (Cut)
                    {
                        if (File.Exists(d.ItemPath))
                        {
                            File.Move(d.ItemPath, outFilename);
                        }
                        else
                        {
                            Directory.Move(d.ItemPath, outFilename);
                        }
                    }
                    else
                    {
                        if (File.Exists(d.ItemPath))
                        {
                            File.Copy(d.ItemPath, outFilename);
                        }
                        else
                        {
                            CopyDirectory(d.ItemPath, outFilename, true);
                        }
                    }
                    success.Add(new DiskItem(outFilename));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            return success;
        }

        //https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        static void CopyDirectory(String sourceDirName, String destDirName, Boolean copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            var dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (var subdir in dirs)
                {
                    var temppath = Path.Combine(destDirName, subdir.Name);
                    CopyDirectory(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

    }
}
