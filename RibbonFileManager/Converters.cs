using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RibbonFileManager
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            if ((Boolean)value)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Hidden;
            }
        }

        public Object ConvertBack(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Visible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class BoolInverterConverter : IValueConverter
    {
        private static BoolInverterConverter instance;

        public static BoolInverterConverter Instance => instance ?? (instance = new BoolInverterConverter());

        public Object Convert(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            return !((Boolean)value);
        }

        public Object ConvertBack(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Visible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class ThicknessLeftToSubtractDoubleConverter : IMultiValueConverter
    {
        public Object Convert(
            Object[] values, Type targetType, Object parameter, CultureInfo culture)
        {
            //var item = parameter as TreeViewItem;
            return (Double.Parse(values[0].ToString())) - (((Thickness)(values[1])).Left);
        }

        public Object[] ConvertBack(
            Object value, Type[] targetTypes, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NavPaneTreeViewItemMarginConverter : IValueConverter
    {
        public Object Convert(
            Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            Thickness basePadding = ((Thickness)(value));
            var param = Double.Parse(parameter.ToString());
            return new Thickness(basePadding.Left + param, basePadding.Top, basePadding.Right, basePadding.Bottom);
        }

        public Object ConvertBack(
            Object value, Type targetTypes, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DoubleAdderConverter : IValueConverter
    {
        public Object Convert(
            Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return Double.Parse(value.ToString()) + Double.Parse(parameter.ToString());
        }

        public Object ConvertBack(
            Object value, Type targetTypes, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DoubleComparisonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            double val = double.Parse(value.ToString());
            int paramFirstNumerical = 0;
            for (int i = 0; i < parameter.ToString().Length; i++)
            {
                if (char.IsNumber(parameter.ToString().ElementAt(i)))
                {
                    paramFirstNumerical = i;
                    break;
                }
            }

            double param = double.Parse(parameter.ToString().Substring(paramFirstNumerical));

            string opr = parameter.ToString().Substring(0, paramFirstNumerical);

            Debug.WriteLine(val.ToString() + "   " + opr + "   " + param.ToString());

            if (opr == ">")
                return val > param;
            else if (opr == "<")
                return val < param;
            else if (opr == ">=")
                return val >= param;
            else if (opr == "<=")
                return val <= param;
            else
                return val >= param;
        }

        public object ConvertBack(object value, Type targetType,
        object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringIsNullOrWhiteSpaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return String.IsNullOrWhiteSpace(value.ToString());
        }

        public object ConvertBack(object value, Type targetType,
        object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType,
        object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DoubleToThicknessConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            double val = double.Parse(value.ToString());
            Thickness output = new Thickness(0);
            var param = parameter.ToString().ToLower();

            if (param.Contains("left"))
                output.Left = val;

            if (param.Contains("top"))
                output.Top = val;

            if (param.Contains("right"))
                output.Right = val;

            if (param.Contains("bottom"))
                output.Bottom = val;

            Debug.WriteLine("RESULT THICKNESS: " + output.Left + ", " + output.Top + ", " + output.Right + ", " + output.Bottom);

            return output;
        }

        public Object ConvertBack(Object value, Type targetType,
            Object parameter, CultureInfo culture)
        {
            Thickness val = (Thickness)value;
            var param = parameter.ToString();
            if (param == "Top")
            {
                return val.Top;
            }
            else if (param == "Right")
            {
                return val.Right;
            }
            else if (param == "Bottom")
            {
                return val.Bottom;
            }
            else
            {
                return val.Left;
            }
        }
    }
}
