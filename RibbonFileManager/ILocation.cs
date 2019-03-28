using Start9.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Controls;

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

        public override Boolean Equals(Object obj) => Equals(obj as Location);
        public Boolean Equals(Location other) => Name == other?.Name;
        public override Int32 GetHashCode() => Name.GetHashCode();

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

        public static Dictionary<Guid, string> Names = new Dictionary<Guid, string>
        {
            {ThisPcGuid, "This PC"}
        };

        public override String Name => Names[LocationGuid];

        public override String LocationPath => Name;

        public override BreadcrumbItem[] BreadcrumbsSegments => new BreadcrumbItem[] { new BreadcrumbItem(Name, Name) };

        public override Icon Icon => null;
    }
}
