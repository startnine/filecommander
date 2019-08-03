﻿using System.Windows;
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

            double size = AssociatedObject.Height / 48;
            (AssociatedObject.Content as FrameworkElement).LayoutTransform = new ScaleTransform(size, size);
        }
    }
}
