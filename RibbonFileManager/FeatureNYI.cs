using Start9.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RibbonFileManager
{
    public class FeatureNYI
    {
        public static bool EnableNYIIndication = true;

        static RotateTransform _upsideDownTransform = new RotateTransform(180);

        public static DependencyProperty IsNYIProperty =
            DependencyProperty.RegisterAttached("IsNYI", typeof(bool), typeof(FeatureNYI), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, IsNYIPropertyChangedCallback));

        private static void IsNYIPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender != null) && (sender is FrameworkElement el))
            {
                if (EnableNYIIndication)
                {
                    el.RenderTransformOrigin = new Point(0.5, 0.5);
                    el.RenderTransform = _upsideDownTransform;
                }
            }
        }

        public static bool GetIsNYI(DependencyObject el)
        {
            return (bool)el.GetValue(IsNYIProperty);
        }

        public static void SetIsNYI(DependencyObject el, bool value)
        {
            el.SetValue(IsNYIProperty, value);
        }
    }
}
