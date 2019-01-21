using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
using WindowsSharp.DiskItems;

namespace RibbonFileManager
{
    public class DiskItemSubItemsTreeFilterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = value as List<DiskItem>;

            //if (collection != null)
            return collection.All(
            folder => (
            (folder.ItemCategory == DiskItem.DiskItemCategory.Directory)
            | (
            (folder.ItemCategory == DiskItem.DiskItemCategory.Directory)
            && Directory.Exists(folder.ItemPath)
            )));
            /*else
                return collection;*/
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
