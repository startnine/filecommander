using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using System.Windows.Media;
using System.Diagnostics;

namespace RibbonFileManager
{
    public class IconScaleBehavior : Behavior<FrameworkElement>
    {
        public FrameworkElement ScaleTarget
        {
            get => (FrameworkElement)GetValue(ScaleTargetProperty);
            set => SetValue(ScaleTargetProperty, value);
        }

        public static readonly DependencyProperty ScaleTargetProperty =
            DependencyProperty.Register(nameof(ScaleTarget), typeof(FrameworkElement), typeof(IconScaleBehavior), new PropertyMetadata(null));

        /*public double TargetSize
        {
            get => (double)GetValue(TargetSizeProperty);
            set => SetValue(TargetSizeProperty, value);
        }

        public static readonly DependencyProperty TargetSizeProperty = DependencyProperty.Register(nameof(TargetSize), typeof(double), typeof(IconScaleBehavior), new PropertyMetadata((double)-1, OnTargetSizePropertyChanged));

        static void OnTargetSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as IconScaleBehavior).UpdateSize();
        }*/

        protected override void OnAttached()
        {
            base.OnAttached();

            UpdateSize();

            //AssociatedObject.LayoutUpdated += (sneder, args) => UpdateSize();
            AssociatedObject.Loaded += (sneder, args) => UpdateSize();
            AssociatedObject.IsVisibleChanged += (sneder, args) => UpdateSize();
            AssociatedObject.SizeChanged += (sneder, args) => UpdateSize();
        }

        void UpdateSize()
        {
            if (AssociatedObject.IsVisible && (ScaleTarget != null) && double.IsNormal(AssociatedObject.ActualWidth))
            {
                double size = AssociatedObject.ActualWidth / 48.0;
                ScaleTarget.LayoutTransform = new ScaleTransform(size, size);
            }
        }
    }
}
