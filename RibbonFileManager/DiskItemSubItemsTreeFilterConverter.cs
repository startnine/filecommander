using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
//using WindowsSharp.DiskItems;

namespace RibbonFileManager
{
    public class DiskItemSubItemsTreeFilterConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            var collection = value as List<DiskItem>;

            return collection.All(folder => (folder.ItemCategory == DiskItemCategory.Directory) || ((folder.ItemCategory == DiskItemCategory.Directory) && Directory.Exists(folder.ItemPath)));
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
