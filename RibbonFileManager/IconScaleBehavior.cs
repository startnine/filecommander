using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using System.Windows.Media;

namespace RibbonFileManager
{
    public class IconScaleBehavior : Behavior<ContentPresenter>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            UpdateSize();

            AssociatedObject.LayoutUpdated += (sneder, args) => UpdateSize();
            AssociatedObject.IsVisibleChanged += (sneder, args) =>
            {
                if (AssociatedObject.IsVisible)
                    UpdateSize();
            };
        }

        void UpdateSize()
        {
            double size = AssociatedObject.Width / 48;
            (AssociatedObject.Content as FrameworkElement).LayoutTransform = new ScaleTransform(size, size);
        }
    }
}
