using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RibbonFileManager
{
    public class DiskItem : INotifyPropertyChanged
    {
        public override Int32 GetHashCode() => _info.GetHashCode();

        public override string ToString()
        {
            return ItemDisplayName;
        }
        FileSystemInfo _info;

        Boolean _populated = false;
        ObservableCollection<DiskItem> _subItems = new ObservableCollection<DiskItem>();

        public ObservableCollection<DiskItem> SubItems
        {
            get
            {
                if (!_populated)
                {
                    PopulateSubItems();
                    _populated = true;
                }

                return _subItems;
            }
            set
            {
                _subItems = value;
                NotifyPropertyChanged();
            }
        }
        public String ItemPath
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

        public String ItemRealName
        {
            get => _info.Name;
        }

        String _itemDisplayName = null;

        public String ItemDisplayName
        {
            get
            {
                return _itemDisplayName ?? ItemRealName;
            }
            set
            {
                _itemDisplayName = value;
                NotifyPropertyChanged();
            }
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

        public bool IsDrive => ((ItemCategory == DiskItemCategory.Directory) && (ItemPath.Length == 3) && (Char.IsLetter(ItemPath.ToCharArray()[0])) && (ItemPath.ToCharArray()[1] == ':'));

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Icon ItemSmallIcon
        {
            get
            {
                return GetIcon(0x00000001 | 0x100);
            }
        }

        public Icon ItemLargeIcon
        {
            get
            {
                return GetIcon(0x000000000 | 0x100);
            }
        }

        public Icon ItemExtraLargeIcon
        {
            get
            {
                return GetIcon(0x000000000 | 0x100, 0x2);
            }
        }

        public Icon ItemJumboIcon
        {
            get
            {
                return GetIcon(0x000000000 | 0x100, 0x4);
            }
        }

        Icon GetIcon(UInt32 flags)
        {
            var shInfo = new NativeMethods.ShFileInfo();
            NativeMethods.SHGetFileInfo(ItemPath, 0, ref shInfo, (UInt32)Marshal.SizeOf(shInfo), flags);
            var result = (Icon) Icon.FromHandle(shInfo.hIcon).Clone();
            NativeMethods.DestroyIcon(shInfo.hIcon);
            return result;
        }

        Icon GetIcon(UInt32 flags, Int32 imageList)
        {
            var shInfo = new NativeMethods.ShFileInfo();
            NativeMethods.SHGetFileInfo(ItemPath, 0, ref shInfo, (UInt32)Marshal.SizeOf(shInfo), flags);

            NativeMethods.SHGetImageList(imageList, ref NativeMethods.iidImageList, out var list);
            var resultHandle = IntPtr.Zero;
            list.GetIcon(shInfo.iIcon, 1, ref resultHandle);
            var finalResult = (Icon) Icon.FromHandle(resultHandle).Clone();
            NativeMethods.DestroyIcon(resultHandle);
            return finalResult;
        }

        public Double ItemSize => _info is FileInfo ? (_info as FileInfo).Length : 0.0;

        public String FriendlyItemSize
        {
            get
            {
                if (_info is FileInfo)
                {
                    var size = ItemSize;
                    var unitCounter = 0;
                    while (size > 1024)
                    {
                        size /= 1024;
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
                    else return unitCounter == 4 ? size.ToString() + " TB" : size.ToString() + " PB";
                }
                else return String.Empty;
            }
        }

        public DiskItem(String path)
        {
            ItemPath = Environment.ExpandEnvironmentVariables(path);
            if (Directory.Exists(ItemPath))
                ItemCategory = DiskItemCategory.Directory;
        }

        public DiskItem(FileSystemInfo info)
        {
            _info = info;
            if (info is DirectoryInfo)
                ItemCategory = DiskItemCategory.Directory;
        }

        FileSystemWatcher _watcher;

        void PopulateSubItems()
        {
            if (_info is DirectoryInfo)
            {
                var info = _info as DirectoryInfo;
                var lastDirectoryIndex = 0;
                var infos = info.GetFileSystemInfos();
                foreach (var f in infos)
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

        String SetFSInfo(String path)
        {
            var returnValue = Environment.ExpandEnvironmentVariables(path);
            if (Directory.Exists(path))
                _info = new DirectoryInfo(path);
            else if (File.Exists(path))
                _info = new FileInfo(path);
            else
                throw new IOException("File does not exist: " + path);

            return returnValue;
        }

        public enum OpenVerbs
        {
            Normal,
            Admin
        }

        public void Open(OpenVerbs verb = OpenVerbs.Normal)
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
            var assoc = new List<DiskItem>();
            var ext = Path.GetExtension(ItemPath);

            var keyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + ext + @"\OpenWithList";

            using (var openWithListKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyPath))
            {
                if (openWithListKey != null)
                {
                    var mruListVal = openWithListKey.GetValue("MRUList");
                    if (mruListVal != null)
                    {
                        var mruList = mruListVal.ToString().ToCharArray();
                        foreach (var c in mruList)
                        {
                            var charVal = openWithListKey.GetValue(c.ToString());
                            if (charVal != null)
                            {
                                var exePath = ConvertProgIdToExecutablePath(charVal.ToString());
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

        String ConvertProgIdToExecutablePath(String progId)
        {
            String outPath = null;
            using (var appPathKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + progId))
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
                        }
                    }
                }
            }

            return outPath;
        }

        String GetAssociatedProgramItemFromRegistry(Object targetObject)
        {
            String item = null;
            var targetValue = targetObject.ToString();
            if (targetValue != null)
            {
                targetValue = Environment.ExpandEnvironmentVariables(targetValue);

                if (File.Exists(targetValue))
                    item = targetValue;
            }
            return item;
        }

        public Boolean? RenameItem(String newName)
        {
            Boolean? returnValue = null;
            var oldPath = ItemPath;
            var newPath = Path.Combine(Directory.GetParent(ItemPath).ToString(), newName);
            if ((!File.Exists(newPath)) && (!Directory.Exists(newPath)))
            {
                if ((ItemCategory == DiskItemCategory.File) || (ItemCategory == DiskItemCategory.Shortcut))
                {
                    try
                    {
                        File.Move(oldPath, newPath);
                        returnValue = true;
                    }
                    catch (IOException)
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
                    catch (IOException)
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
            public extern static Int32 SHGetImageList(Int32 iImageList, ref Guid riid, out IImageList ppv);

            public static Guid iidImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");

            [ComImport]
            [Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IImageList
            {
                [PreserveSig]
                Int32 Add(
                    IntPtr hbmImage,
                    IntPtr hbmMask,
                    ref Int32 pi);

                [PreserveSig]
                Int32 ReplaceIcon(
                    Int32 i,
                    IntPtr hicon,
                    ref Int32 pi);

                [PreserveSig]
                Int32 SetOverlayImage(
                    Int32 iImage,
                    Int32 iOverlay);

                [PreserveSig]
                Int32 Replace(
                    Int32 i,
                    IntPtr hbmImage,
                    IntPtr hbmMask);

                [PreserveSig]
                Int32 AddMasked(
                    IntPtr hbmImage,
                    Int32 crMask,
                    ref Int32 pi);

                [PreserveSig]
                Int32 Draw(
                    ref IMAGELISTDRAWPARAMS pimldp);

                [PreserveSig]
                Int32 Remove(
                Int32 i);

                [PreserveSig]
                Int32 GetIcon(
                    Int32 i,
                    Int32 flags,
                    ref IntPtr picon);

                [PreserveSig]
                Int32 GetImageInfo(
                    Int32 i,
                    ref IMAGEINFO pImageInfo);

                [PreserveSig]
                Int32 Copy(
                    Int32 iDst,
                    IImageList punkSrc,
                    Int32 iSrc,
                    Int32 uFlags);

                [PreserveSig]
                Int32 Merge(
                    Int32 i1,
                    IImageList punk2,
                    Int32 i2,
                    Int32 dx,
                    Int32 dy,
                    ref Guid riid,
                    ref IntPtr ppv);

                [PreserveSig]
                Int32 Clone(
                    ref Guid riid,
                    ref IntPtr ppv);

                [PreserveSig]
                Int32 GetImageRect(
                    Int32 i,
                    ref Rectangle prc);

                [PreserveSig]
                Int32 GetIconSize(
                    ref Int32 cx,
                    ref Int32 cy);

                [PreserveSig]
                Int32 SetIconSize(
                    Int32 cx,
                    Int32 cy);

                [PreserveSig]
                Int32 GetImageCount(
                ref Int32 pi);

                [PreserveSig]
                Int32 SetImageCount(
                    Int32 uNewCount);

                [PreserveSig]
                Int32 SetBkColor(
                    Int32 clrBk,
                    ref Int32 pclr);

                [PreserveSig]
                Int32 GetBkColor(
                    ref Int32 pclr);

                [PreserveSig]
                Int32 BeginDrag(
                    Int32 iTrack,
                    Int32 dxHotspot,
                    Int32 dyHotspot);

                [PreserveSig]
                Int32 EndDrag();

                [PreserveSig]
                Int32 DragEnter(
                    IntPtr hwndLock,
                    Int32 x,
                    Int32 y);

                [PreserveSig]
                Int32 DragLeave(
                    IntPtr hwndLock);

                [PreserveSig]
                Int32 DragMove(
                    Int32 x,
                    Int32 y);

                [PreserveSig]
                Int32 SetDragCursorImage(
                    ref IImageList punk,
                    Int32 iDrag,
                    Int32 dxHotspot,
                    Int32 dyHotspot);

                [PreserveSig]
                Int32 DragShowNolock(
                    Int32 fShow);

                [PreserveSig]
                Int32 GetDragImage(
                    ref Point ppt,
                    ref Point pptHotspot,
                    ref Guid riid,
                    ref IntPtr ppv);

                [PreserveSig]
                Int32 GetItemFlags(
                    Int32 i,
                    ref Int32 dwFlags);

                [PreserveSig]
                Int32 GetOverlayImage(
                    Int32 iOverlay,
                    ref Int32 piIndex);
            };

            [StructLayout(LayoutKind.Sequential)]
            public struct IMAGEINFO
            {
                public IntPtr hbmImage;
                public IntPtr hbmMask;
                public Int32 Unused1;
                public Int32 Unused2;
                public Rectangle rcImage;
            }

            public struct IMAGELISTDRAWPARAMS
            {
                public Int32 cbSize;
                public IntPtr himl;
                public Int32 i;
                public IntPtr hdcDst;
                public Int32 x;
                public Int32 y;
                public Int32 cx;
                public Int32 cy;
                public Int32 xBitmap;        // x offest from the upperleft of bitmap
                public Int32 yBitmap;        // y offset from the upperleft of bitmap
                public Int32 rgbBk;
                public Int32 rgbFg;
                public Int32 fStyle;
                public Int32 dwRop;
                public Int32 fState;
                public Int32 Frame;
                public Int32 crEffect;
            }



            [DllImport("user32.dll", SetLastError = true)]
            public static extern Boolean DestroyIcon(IntPtr hIcon);

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
            public static extern Boolean ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

            [StructLayout(LayoutKind.Sequential)]//, CharSet = CharSet.Auto)]
            public struct SHELLEXECUTEINFO
            {
                public Int32 cbSize;
                public UInt32 fMask;
                public IntPtr hwnd;
                public String lpVerb;
                public String lpFile;
                public String lpParameters;
                public String lpDirectory;
                public Int32 nShow;
                public IntPtr hInstApp; //int
                public IntPtr lpIDList; //int
                public String lpClass;
                public IntPtr hkeyClass; //int
                public UInt32 dwHotKey;
                public IntPtr hIcon; //int
                public IntPtr hProcess; //int
            }
        }
    }

    public enum DiskItemCategory
    {
        File,
        Shortcut,
        Directory,
        App
    }
}
