using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace RibbonFileManager
{
    public class DriveFreeSpaceToUsedSpaceConverter : IMultiValueConverter
    {
        public Object Convert(Object[] values, Type targetType, Object parameter, CultureInfo culture)
        {
            if ((values[0] is Double freeSpace) && (values[1] is Double totalSpace))
                return totalSpace - freeSpace;
            else
                return 0.0;
        }

        public Object[] ConvertBack(Object value, Type[] targetTypes, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
