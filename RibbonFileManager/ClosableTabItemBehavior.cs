using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace RibbonFileManager
{
    public class ClosableTabItemBehavior : Behavior<Button>
    {
        TabItem _tabItem;
        public MainWindow OwnerWindow
        {
            get => (MainWindow)GetValue(OwnerWindowProperty);
            set => SetValue(OwnerWindowProperty, value);
        }

        public static readonly DependencyProperty OwnerWindowProperty =
            DependencyProperty.Register("OwnerWindow", typeof(MainWindow), typeof(ClosableTabItemBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();

            _tabItem = AssociatedObject.TemplatedParent as TabItem;

            AssociatedObject.Click += (sneder, args) =>
            {
                if ((_tabItem != null) && (OwnerWindow != null) && OwnerWindow.ContentTabControl.Items.Contains(_tabItem))
                    OwnerWindow.ContentTabControl.Items.Remove(_tabItem);
            };
        }
    }
}
