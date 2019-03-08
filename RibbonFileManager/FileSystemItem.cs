using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace RibbonFileManager
{
    public class FileSystemItem : INotifyPropertyChanged
    {
        FileSystemInfo _info;

        ObservableCollection<FileSystemItem> _subItems = new ObservableCollection<FileSystemItem>();

        public ObservableCollection<FileSystemItem> SubItems
        {
            get => _subItems;
            /*{
                _subItems = new ObservableCollection<FileSystemItem>();
                if (_info is DirectoryInfo)
                {
                    var info = _info as DirectoryInfo;
                    foreach (FileSystemInfo f in info.EnumerateFileSystemInfos())
                        _subItems.Add(new FileSystemItem(f));
                }

                return _subItems;
            }*/
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

        public FileSystemItem(String path)
        {
            ItemPath = Environment.ExpandEnvironmentVariables(path);

            SetFSInfo(ItemPath);

            //WatchFileSystem();
        }

        public FileSystemItem(FileSystemInfo info)
        {
            _info = info;

            //WatchFileSystem();
        }

        FileSystemWatcher _watcher;

        void PopulateSubItems()
        {
            if (_info is DirectoryInfo)
            {
                var info = _info as DirectoryInfo;
                foreach (FileSystemInfo f in info.EnumerateFileSystemInfos())
                    _subItems.Add(new FileSystemItem(f));
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
        }
    }
}
