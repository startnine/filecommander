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

        public override BreadcrumbItem[] BreadcrumbsSegments => MainWindow.Converter.Invoke?.Invoke(Name);
            
        public override Icon Icon { get; }

    }

}
