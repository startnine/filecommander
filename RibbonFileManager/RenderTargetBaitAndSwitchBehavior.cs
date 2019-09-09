using Microsoft.Xaml.Behaviors;
using Start9.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RibbonFileManager
{
    public class RenderTargetBaitAndSwitchBehavior : Behavior<ContentPresenter>
    {
        FrameworkElement _originalContent = null;

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            (AssociatedObject.Content as FrameworkElement).Loaded += Content_Loaded;
        }

        private void Content_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Swap(sender as FrameworkElement);
            }
            catch (Exception ex)
            {
                (sender as FrameworkElement).SizeChanged += Element_SizeChanged;
            }
        }

        private void Element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if ((e.NewSize.Width > 0) && (e.NewSize.Height > 0))
                Swap(sender as FrameworkElement);
        }

        void Swap(FrameworkElement element)
        {
            BitmapImage bmp = new BitmapImage();
            using (var fileStream = new MemoryStream())
            {
                RenderTargetBitmap bitmap = new RenderTargetBitmap((Int32)SystemScaling.WpfUnitsToRealPixels(element.ActualWidth), (Int32)SystemScaling.WpfUnitsToRealPixels(element.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(element);

                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(fileStream);

                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = fileStream;
                bmp.EndInit();
            }

            var img = new Image()
            {
                /*Width = element.ActualWidth,
                Height = element.ActualHeight,*/
                Source = bmp
            };
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
            _originalContent = AssociatedObject.Content as FrameworkElement;
            AssociatedObject.Content = img;
            img.Loaded += (sneder, args) => AssociatedObject.SizeChanged += AssociatedObject_SizeChanged;

            //Debug.WriteLine("Swap complete");
        }

        private void AssociatedObject_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if ((e.WidthChanged || e.HeightChanged) && ((e.NewSize.Width > 0) && (e.NewSize.Height > 0)) && (_originalContent != null))
            {
                AssociatedObject.Content = _originalContent;
                AssociatedObject.UpdateLayout();
                CompositionTarget.Rendering += CompositionTarget_Rendering;
            }
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            AssociatedObject_Loaded(AssociatedObject, null);
        }
    }
}
