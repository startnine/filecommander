using Start9.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RibbonFileManager
{
    public abstract class Location : IEquatable<Location>
    {
        internal Location()
        {

        }

        public abstract String Name { get; }

        public abstract String LocationPath { get; }

        public abstract BreadcrumbItem[] BreadcrumbsSegments { get; }

        public abstract Icon Icon { get; }
        public abstract bool HasSpecialIcon { get; }
        public abstract UIElement SpecialIcon { get; }

        public override Boolean Equals(Object obj) => Equals(obj as Location);
        public Boolean Equals(Location other) => Name == other?.Name;
        public override Int32 GetHashCode() => Name.GetHashCode();
        public abstract IAsyncEnumerable<DiskItem> GetLocationContents(CancellationToken token, Boolean recursive);
    }

    [DebuggerDisplay("Path = {Item.ItemPath}")]
    public class DirectoryQuery : Location
    {
        public DirectoryQuery(String path)
        {
            Item = new DiskItem(path);
        }

        public DiskItem Item { get; }

        public override String Name => Item.ItemDisplayName;

        public override String LocationPath => Item.ItemPath;

        public override BreadcrumbItem[] BreadcrumbsSegments => MainWindow.Converter.Invoke?.Invoke(Item.ItemPath);

        public override Icon Icon => Item.ItemJumboIcon;
        public override bool HasSpecialIcon => Item.HasSpecialIcon;
        public override UIElement SpecialIcon
        {
            get
            {
                if (Item.HasSpecialIcon)
                    return Item.SpecialIcon;
                else
                    return null;
            }
        }

        public override async IAsyncEnumerable<DiskItem> GetLocationContents(CancellationToken token, Boolean recursive)
        {
            var entries = Directory.EnumerateFileSystemEntries(LocationPath, "*", new EnumerationOptions { RecurseSubdirectories = recursive, IgnoreInaccessible = true });
            var enumer = entries.GetEnumerator();
            while (await Task.Run(enumer.MoveNext))
            {
                token.ThrowIfCancellationRequested();
                yield return await Task.Run(() => new DiskItem(enumer.Current));
                //token.ThrowIfCancellationRequested();
            }
        }
    }

    [DebuggerDisplay("Path = {Path} Query = {Query}")]
    public class SearchQuery : Location
    {
        public SearchQuery(String path, String query, Boolean recursive = true)
        {
            Path = path;
            Query = query;
            Recursive = recursive;
        }

        public String Path { get; } 

        public String Query { get; }

        public Boolean Recursive { get; }

        public override String Name => $"{Query} - Search results in {Path}";

        public override String LocationPath => String.Empty; //TODO: Make this work as intended

        public override BreadcrumbItem[] BreadcrumbsSegments => MainWindow.Converter.Invoke?.Invoke(Name);
            
        public override Icon Icon { get; }
        public override bool HasSpecialIcon => false;
        public override UIElement SpecialIcon => null;

        public override async IAsyncEnumerable<DiskItem> GetLocationContents(CancellationToken token, Boolean recursive)
        {
            var entries = Directory.EnumerateFileSystemEntries(Path, Query, new EnumerationOptions { RecurseSubdirectories = recursive, IgnoreInaccessible = true });
            var enumer = entries.GetEnumerator();
            while (await Task.Run(enumer.MoveNext))
            {
                token.ThrowIfCancellationRequested();
                yield return await Task.Run(() => new DiskItem(enumer.Current));
                token.ThrowIfCancellationRequested();
            }
        }
    }


    [DebuggerDisplay("Path = {Item.ItemPath}")]
    public class ShellLocation : Location
    {
        public ShellLocation(Guid guid)
        {
            LocationGuid = guid;
        }

        public Guid LocationGuid { get; }

        public static Guid ThisPcGuid = new Guid("20D04FE0-3AEA-1069-A2D8-08002B30309D");

        public static Dictionary<Guid, String> Names = new Dictionary<Guid, String>
        {
            {ThisPcGuid, "This PC"}
        };

        public List<PropertyGroupDescription> PropertyGroupDescriptions
        {
            get
            {
                var descriptions = new List<PropertyGroupDescription>();
                if (LocationGuid == ThisPcGuid)
                {
                    descriptions.Add(new PropertyGroupDescription("IsDrive"));
                }
                return descriptions;
            }
        }


        public override String Name => Names[LocationGuid];

        public override String LocationPath => Name;

        public override BreadcrumbItem[] BreadcrumbsSegments => new BreadcrumbItem[] { new BreadcrumbItem(Name, Name) };

        public override Icon Icon => null;
        public override bool HasSpecialIcon => true;

        public override UIElement SpecialIcon
        {
            get
            {
                if (LocationGuid == ThisPcGuid)
                    return (UIElement)Application.Current.Resources["ComputerIcon"];
                else
                    return null;
            }
        }

        public override async IAsyncEnumerable<DiskItem> GetLocationContents(CancellationToken token, Boolean recursive)
        {
            if (LocationGuid == ThisPcGuid)
            {
                var entries = new List<DiskItem>()
                    {
                        /*Environment.ExpandEnvironmentVariables(@"%userprofile%\Desktop"),
                        Environment.ExpandEnvironmentVariables(@"%userprofile%\Documents"),
                        Environment.ExpandEnvironmentVariables(@"%userprofile%\Downloads"),
                        Environment.ExpandEnvironmentVariables(@"%userprofile%\Music"),
                        Environment.ExpandEnvironmentVariables(@"%userprofile%\Pictures"),
                        Environment.ExpandEnvironmentVariables(@"%userprofile%\Videos")*/
                        new DiskItem(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
                        {
                            HasSpecialIcon = true,
                            SpecialIconKey = "DesktopFolderIcon"
                        },
                        new DiskItem(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
                        {
                            HasSpecialIcon = true,
                            SpecialIconKey = "DocumentsFolderIcon"
                        },
                        new DiskItem(Environment.ExpandEnvironmentVariables(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")))
                        {
                            HasSpecialIcon = true,
                            SpecialIconKey = "DownloadsFolderIcon"
                        },
                        new DiskItem(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic))
                        {
                            HasSpecialIcon = true,
                            SpecialIconKey = "MusicFolderIcon"
                        },
                        new DiskItem(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures))
                        {
                            HasSpecialIcon = true,
                            SpecialIconKey = "PicturesFolderIcon"
                        },
                        new DiskItem(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos))
                        {
                            HasSpecialIcon = true,
                            SpecialIconKey = "VideosFolderIcon"
                        }
                    };

                foreach (var s in entries)
                {
                    yield return s;
                    token.ThrowIfCancellationRequested();
                }

                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (Directory.Exists(drive.RootDirectory.FullName))
                    {
                        yield return await Task.Run(() => new DiskItem(drive.RootDirectory.FullName));
                    }

                    token.ThrowIfCancellationRequested();
                }
            }
            else //We don't know what this GUID is
            {
                throw new Exception("Unrecognized Shell Location GUID");
            }
        }
    }
}
