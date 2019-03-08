using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace RibbonFileManager
{
    public class DiskItem : INotifyPropertyChanged
    {
        FileSystemInfo _info;

        ObservableCollection<DiskItem> _subItems = new ObservableCollection<DiskItem>();

        public ObservableCollection<DiskItem> SubItems
        {
            get// => _subItems;
            {
                if (_subItems.Count == 0)
                    PopulateSubItems();
                //_subItems = new ObservableCollection<DiskItem>();
                /*if (_info is DirectoryInfo)
                {
                    var info = _info as DirectoryInfo;
                    foreach (FileSystemInfo f in info.EnumerateFileSystemInfos())
                        _subItems.Add(new DiskItem(f));
                }*/

                return _subItems;
            }
            set
            {
                _subItems = value;
                NotifyPropertyChanged();
            }
        }
        public string ItemPath
        {
            get => _info.FullName;
            private set
            {
                SetFSInfo(value);
                NotifyPropertyChanged();
                NotifyPropertyChanged("ItemDisplayName");
                NotifyPropertyChanged("ItemRealName");
            }
        }

        public string ItemRealName
        {
            get => _info.Name;
        }

        string _itemDisplayName = null;

        public string ItemDisplayName
        {
            get
            {
                if (_itemDisplayName != null)
                    return _itemDisplayName;
                else
                    return ItemRealName;
            }
            set
            {
                _itemDisplayName = value;
                NotifyPropertyChanged();
            }
        }

        public enum DiskItemCategory
        {
            File,
            Shortcut,
            Directory,
            App
        }

        DiskItemCategory _itemCategory;

        public DiskItemCategory ItemCategory
        {
            get => _itemCategory;
            set
            {
                _itemCategory = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public System.Drawing.Icon ItemSmallIcon
        {
            get
            {
                return GetIcon(0x00000001 | 0x100);
            }
        }

        public System.Drawing.Icon ItemLargeIcon
        {
            get
            {
                return GetIcon(0x000000000 | 0x100);
            }
        }

        public System.Drawing.Icon ItemExtraLargeIcon
        {
            get
            {
                return GetIcon(0x000000000 | 0x100, 0x2);
            }
        }

        public System.Drawing.Icon ItemJumboIcon
        {
            get
            {
                return GetIcon(0x000000000 | 0x100, 0x4);
            }
        }

        System.Drawing.Icon GetIcon(UInt32 flags)
        {
            return GetIcon(flags, 0);
        }

        System.Drawing.Icon GetIcon(UInt32 flags, int imageList)
        {
            if (imageList != 0)
            {
                NativeMethods.ShFileInfo shInfo = new NativeMethods.ShFileInfo();
                NativeMethods.SHGetFileInfo(ItemPath, 0, ref shInfo, (UInt32)Marshal.SizeOf(shInfo), flags);
                System.Drawing.Icon result = (System.Drawing.Icon)(System.Drawing.Icon.FromHandle(shInfo.hIcon).Clone());
                NativeMethods.DestroyIcon(shInfo.hIcon);

                var hres = NativeMethods.SHGetImageList(imageList, ref NativeMethods.iidImageList, out NativeMethods.IImageList list);
                IntPtr resultHandle = IntPtr.Zero;
                list.GetIcon(shInfo.iIcon, 1, ref resultHandle);
                System.Drawing.Icon finalResult = (System.Drawing.Icon)(System.Drawing.Icon.FromHandle(resultHandle).Clone());
                NativeMethods.DestroyIcon(resultHandle);
                return finalResult;
            }
            else
            {
                NativeMethods.ShFileInfo shInfo = new NativeMethods.ShFileInfo();
                NativeMethods.SHGetFileInfo(ItemPath, 0, ref shInfo, (UInt32)Marshal.SizeOf(shInfo), flags);
                //return System.Drawing.Icon.FromHandle(shInfo.hIcon);
                System.Drawing.Icon result = (System.Drawing.Icon)(System.Drawing.Icon.FromHandle(shInfo.hIcon).Clone());
                NativeMethods.DestroyIcon(shInfo.hIcon);
                return result;
            }
        }

        public double ItemSize
        {
            get
            {
                if (_info is FileInfo)
                    return (_info as FileInfo).Length;
                else
                    return (double)0.0;
            }
        }

        public string FriendlyItemSize
        {
            get
            {
                if (_info is FileInfo)
                {
                    double size = ItemSize;
                    int unitCounter = 0;
                    while (size > 1024)
                    {
                        size = size / 1024;
                        unitCounter++;
                    }

                    if (unitCounter == 0)
                        return size.ToString() + " B";
                    else if (unitCounter == 1)
                        return size.ToString() + " KB";
                    else if (unitCounter == 2)
                        return size.ToString() + " MB";
                    else if (unitCounter == 3)
                        return size.ToString() + " GB";
                    else if (unitCounter == 4)
                        return size.ToString() + " TB";
                    else
                        return size.ToString() + " PB";
                }
                else return String.Empty;
            }
        }

        public DiskItem(String path)
        {
            ItemPath = Environment.ExpandEnvironmentVariables(path);
            if (Directory.Exists(ItemPath))
                ItemCategory = DiskItemCategory.Directory;

            //SetFSInfo(ItemPath);

            //WatchFileSystem();
        }

        public DiskItem(FileSystemInfo info)
        {
            _info = info;
            if (info is DirectoryInfo)
                ItemCategory = DiskItemCategory.Directory;

            //WatchFileSystem();
        }

        FileSystemWatcher _watcher;

        void PopulateSubItems()
        {
            if (_info is DirectoryInfo)
            {
                var info = _info as DirectoryInfo;
                int lastDirectoryIndex = 0;
                var infos = info.GetFileSystemInfos();
                foreach (FileSystemInfo f in infos)
                {
                    if (f is DirectoryInfo)
                    {
                        _subItems.Insert(lastDirectoryIndex, new DiskItem(f));
                        lastDirectoryIndex++;
                    }
                    else
                        _subItems.Add(new DiskItem(f));
                }
            }
        }

        void WatchFileSystem()
        {

            if (_info is DirectoryInfo)
                _watcher = new FileSystemWatcher(_info.FullName);
        }

        string SetFSInfo(string path)
        {
            string returnValue = Environment.ExpandEnvironmentVariables(path);
            if (Directory.Exists(path))
                _info = new DirectoryInfo(path);
            else if (File.Exists(path))
                _info = new FileInfo(path);
            else
                returnValue = null;

            return returnValue;
        }

        public void Open()
        {
            Open(OpenVerbs.Normal);
        }

        public enum OpenVerbs
        {
            Normal,
            Admin
        }

        public void Open(OpenVerbs verb)
        {
            if (verb == OpenVerbs.Admin)
                Process.Start(new ProcessStartInfo(ItemPath)
                {
                    Verb = "runas",
                    UseShellExecute = true
                });
            else
                Process.Start(new ProcessStartInfo(ItemPath)
                {
                    UseShellExecute = true
                });
        }

        public List<DiskItem> GetOpenWithPrograms()
        {
            List<DiskItem> assoc = new List<DiskItem>();
            string ext = Path.GetExtension(ItemPath);

            string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + ext + @"\OpenWithList";

            using (Microsoft.Win32.RegistryKey openWithListKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyPath))
            {
                if (openWithListKey != null)
                {
                    var mruListVal = openWithListKey.GetValue("MRUList");
                    if (mruListVal != null)
                    {
                        char[] mruList = mruListVal.ToString().ToCharArray();
                        foreach (char c in mruList)
                        {
                            var charVal = openWithListKey.GetValue(c.ToString());
                            if (charVal != null)
                            {
                                string exePath = ConvertProgIdToExecutablePath(charVal.ToString());
                                if (exePath != null)
                                    assoc.Add(new DiskItem(exePath));
                                else
                                    Debug.WriteLine("COULD NOT GET EXE PATH FROM PROGID: " + charVal.ToString());
                            }
                            else
                                Debug.WriteLine("COULD NOT GET PROGID: " + c.ToString());
                        }
                    }
                }
            }

            return assoc;
        }

        string ConvertProgIdToExecutablePath(string progId)
        {
            string outPath = null;
            using (Microsoft.Win32.RegistryKey appPathKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + progId))
            {
                if (appPathKey != null)
                {
                    var pathVal = appPathKey.GetValue("Path");
                    if (pathVal != null)
                    {
                        outPath = GetAssociatedProgramItemFromRegistry(pathVal);
                    }

                    if (outPath == null)
                    {
                        var defaultVal = appPathKey.GetValue(null);
                        if (defaultVal != null)
                        {
                            outPath = GetAssociatedProgramItemFromRegistry(defaultVal);
                            /*string defaultValue = defaultVal.ToString();
                            if (defaultValue != null)
                            {
                                Debug.WriteLine("pathValue: " + defaultValue);
                                defaultValue = Environment.ExpandEnvironmentVariables(defaultValue);

                                if (File.Exists(defaultValue))
                                    assoc = new DiskItem(defaultValue);
                            }*/
                        }
                    }
                }
            }

            return outPath;
        }

        string GetAssociatedProgramItemFromRegistry(object targetObject)
        {
            string item = null;
            string targetValue = targetObject.ToString();
            if (targetValue != null)
            {
                //Debug.WriteLine("targetValue: " + targetValue);
                targetValue = Environment.ExpandEnvironmentVariables(targetValue);

                if (File.Exists(targetValue))
                    item = targetValue;
            }
            return item;
        }

        public bool? RenameItem(string newName)
        {
            bool? returnValue = null;
            string oldPath = ItemPath;
            string newPath = Path.Combine(Directory.GetParent(ItemPath).ToString(), newName);
            if ((!File.Exists(newPath)) && (!Directory.Exists(newPath)))
            {
                if ((ItemCategory == DiskItemCategory.File) || (ItemCategory == DiskItemCategory.Shortcut))
                {
                    try
                    {
                        File.Move(oldPath, newPath);
                        returnValue = true;
                    }
                    catch (IOException ex)
                    {
                        returnValue = false;
                    }
                }
                else if (ItemCategory == DiskItemCategory.Directory)
                {
                    try
                    {
                        Directory.Move(oldPath, newPath);
                        returnValue = true;
                    }
                    catch (IOException ex)
                    {
                        returnValue = false;
                    }
                }
                else
                    returnValue = false;
            }

            if (returnValue == true)
            {
                ItemPath = newPath;
            }

            return returnValue;
        }

        public void ShowProperties()
        {
            var info = new NativeMethods.SHELLEXECUTEINFO()
            {
                lpVerb = "properties",
                nShow = 1,
                fMask = 0x00000040 | 0x0000000C
            };
            if (ItemCategory == DiskItemCategory.Directory)
                info.lpDirectory = ItemPath;
            else if ((ItemCategory == DiskItemCategory.File) | (ItemCategory == DiskItemCategory.Shortcut))
                info.lpFile = ItemPath;

            info.cbSize = Marshal.SizeOf(info);
        }

        private class NativeMethods
        {
            [DllImport("shell32.dll", EntryPoint = "#727")]
            public extern static int SHGetImageList(int iImageList, ref Guid riid, out IImageList ppv);

            public static Guid iidImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");

            [ComImportAttribute()]
            [GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
            [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
            //helpstring("Image List"),
            public interface IImageList
            {
                [PreserveSig]
                int Add(
                    IntPtr hbmImage,
                    IntPtr hbmMask,
                    ref int pi);

                [PreserveSig]
                int ReplaceIcon(
                    int i,
                    IntPtr hicon,
                    ref int pi);

                [PreserveSig]
                int SetOverlayImage(
                    int iImage,
                    int iOverlay);

                [PreserveSig]
                int Replace(
                    int i,
                    IntPtr hbmImage,
                    IntPtr hbmMask);

                [PreserveSig]
                int AddMasked(
                    IntPtr hbmImage,
                    int crMask,
                    ref int pi);

                [PreserveSig]
                int Draw(
                    ref IMAGELISTDRAWPARAMS pimldp);

                [PreserveSig]
                int Remove(
                int i);

                [PreserveSig]
                int GetIcon(
                    int i,
                    int flags,
                    ref IntPtr picon);

                [PreserveSig]
                int GetImageInfo(
                    int i,
                    ref IMAGEINFO pImageInfo);

                [PreserveSig]
                int Copy(
                    int iDst,
                    IImageList punkSrc,
                    int iSrc,
                    int uFlags);

                [PreserveSig]
                int Merge(
                    int i1,
                    IImageList punk2,
                    int i2,
                    int dx,
                    int dy,
                    ref Guid riid,
                    ref IntPtr ppv);

                [PreserveSig]
                int Clone(
                    ref Guid riid,
                    ref IntPtr ppv);

                [PreserveSig]
                int GetImageRect(
                    int i,
                    ref System.Drawing.Rectangle prc);

                [PreserveSig]
                int GetIconSize(
                    ref int cx,
                    ref int cy);

                [PreserveSig]
                int SetIconSize(
                    int cx,
                    int cy);

                [PreserveSig]
                int GetImageCount(
                ref int pi);

                [PreserveSig]
                int SetImageCount(
                    int uNewCount);

                [PreserveSig]
                int SetBkColor(
                    int clrBk,
                    ref int pclr);

                [PreserveSig]
                int GetBkColor(
                    ref int pclr);

                [PreserveSig]
                int BeginDrag(
                    int iTrack,
                    int dxHotspot,
                    int dyHotspot);

                [PreserveSig]
                int EndDrag();

                [PreserveSig]
                int DragEnter(
                    IntPtr hwndLock,
                    int x,
                    int y);

                [PreserveSig]
                int DragLeave(
                    IntPtr hwndLock);

                [PreserveSig]
                int DragMove(
                    int x,
                    int y);

                [PreserveSig]
                int SetDragCursorImage(
                    ref IImageList punk,
                    int iDrag,
                    int dxHotspot,
                    int dyHotspot);

                [PreserveSig]
                int DragShowNolock(
                    int fShow);

                [PreserveSig]
                int GetDragImage(
                    ref System.Drawing.Point ppt,
                    ref System.Drawing.Point pptHotspot,
                    ref Guid riid,
                    ref IntPtr ppv);

                [PreserveSig]
                int GetItemFlags(
                    int i,
                    ref int dwFlags);

                [PreserveSig]
                int GetOverlayImage(
                    int iOverlay,
                    ref int piIndex);
            };

            [StructLayout(LayoutKind.Sequential)]
            public struct IMAGEINFO
            {
                public IntPtr hbmImage;
                public IntPtr hbmMask;
                public int Unused1;
                public int Unused2;
                public System.Drawing.Rectangle rcImage;
            }

            public struct IMAGELISTDRAWPARAMS
            {
                public int cbSize;
                public IntPtr himl;
                public int i;
                public IntPtr hdcDst;
                public int x;
                public int y;
                public int cx;
                public int cy;
                public int xBitmap;        // x offest from the upperleft of bitmap
                public int yBitmap;        // y offset from the upperleft of bitmap
                public int rgbBk;
                public int rgbFg;
                public int fStyle;
                public int dwRop;
                public int fState;
                public int Frame;
                public int crEffect;
            }


            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool DestroyIcon(IntPtr hIcon);

            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr SHGetFileInfo(String pszPath, UInt32 dwFileAttributes, ref ShFileInfo psfi, UInt32 cbFileInfo, UInt32 uFlags);

            [StructLayout(LayoutKind.Sequential)]
            public struct ShFileInfo
            {
                public IntPtr hIcon;
                public Int32 iIcon;
                public UInt32 dwAttributes;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public String szDisplayName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
                public String szTypeName;
            }

            [DllImport("shell32.dll")]
            public static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

            [StructLayout(LayoutKind.Sequential)]//, CharSet = CharSet.Auto)]
            public struct SHELLEXECUTEINFO
            {
                public int cbSize;
                public uint fMask;
                public IntPtr hwnd;
                public String lpVerb;
                public String lpFile;
                public String lpParameters;
                public String lpDirectory;
                public int nShow;
                public IntPtr hInstApp; //int
                public IntPtr lpIDList; //int
                public String lpClass;
                public IntPtr hkeyClass; //int
                public uint dwHotKey;
                public IntPtr hIcon; //int
                public IntPtr hProcess; //int
            }
        }
    }
}
