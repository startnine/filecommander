using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using System.Windows.Input;
using Start9.UI.Wpf;
using System.Windows.Media;

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
            DependencyProperty.Register(nameof(OwnerWindow), typeof(MainWindow), typeof(ClosableTabItemBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();

            _tabItem = AssociatedObject.TemplatedParent as TabItem;

            AssociatedObject.Click += (sneder, args) =>
            {
                if ((_tabItem != null) && (OwnerWindow != null) && OwnerWindow.ContentTabControl.Items.Contains(_tabItem))
                    OwnerWindow.ContentTabControl.Items.Remove(_tabItem);
            };

            _tabItem.PreviewMouseLeftButtonDown += (sneder, args) =>
            {
                int mousePressCounter = 0;
                Timer timer = new Timer(10);
                Point initialPoint = SystemScaling.CursorPosition;
                _tabItem.RenderTransform = new TranslateTransform(0, 0);
                timer.Elapsed += (sneder, args) =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (mousePressCounter < 10)
                        {
                            if (Mouse.LeftButton == MouseButtonState.Released)
                                timer.Stop();
                            else
                                mousePressCounter++;
                        }
                        else
                        {
                            if (Mouse.LeftButton == MouseButtonState.Released)
                            {
                                timer.Stop();
                                var tabControl = _tabItem.Parent as TabControl;

                                bool after = false;
                                int index = -1;
                                foreach (TabItem item in tabControl.Items)
                                {
                                    if ((item != _tabItem) && SystemScaling.IsMouseWithin(item))
                                    {
                                        index = tabControl.Items.IndexOf(item);
                                        double xMiddle = item.PointToScreen(new Point(0, 0)).X + (item.ActualWidth / 2);
                                        if (SystemScaling.CursorPosition.X > xMiddle)
                                            after = true;

                                        break;
                                    }
                                }

                                bool animate = false;

                                if (index != -1)
                                {
                                    (_tabItem.RenderTransform as TranslateTransform).X = 0;
                                    int oldIndex = tabControl.Items.IndexOf(_tabItem);
                                    /*bool invert = ((oldIndex >= (index - 1)) && (oldIndex <= (index + 1)));

                                    if (invert)
                                        after = !after;*/

                                    if ((after) && ((index + 1) != oldIndex))
                                    {
                                        tabControl.Items.Remove(_tabItem);
                                        tabControl.Items.Insert(index + 1, _tabItem);
                                        tabControl.SelectedIndex = index + 1;
                                    }
                                    else if (index != oldIndex)
                                    {
                                        tabControl.Items.Remove(_tabItem);
                                        tabControl.Items.Insert(index, _tabItem);
                                        tabControl.SelectedIndex = index;
                                    }
                                    else
                                        animate = true;
                                }
                                
                                (_tabItem.RenderTransform as TranslateTransform).X = 0;
                            }
                            else
                            {
                                (_tabItem.RenderTransform as TranslateTransform).X = (SystemScaling.CursorPosition.X - initialPoint.X);
                                //System.Diagnostics.Debug.WriteLine("X: " + (_tabItem.RenderTransform as TranslateTransform).X);
                            }
                        }
                    }));
                };

                timer.Start();
            };
        }
    }
}
