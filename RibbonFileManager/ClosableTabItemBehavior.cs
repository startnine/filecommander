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
using System.Diagnostics;

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

        public TabControl OwnerControl
        {
            get => (TabControl)GetValue(OwnerControlProperty);
            set => SetValue(OwnerControlProperty, value);
        }

        public static readonly DependencyProperty OwnerControlProperty =
            DependencyProperty.Register(nameof(OwnerControl), typeof(TabControl), typeof(ClosableTabItemBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();

            _tabItem = AssociatedObject.TemplatedParent as TabItem;

            AssociatedObject.Click += (sneder, args) =>
            {
                /*if ((_tabItem != null) && (OwnerWindow != null) && OwnerWindow.ContentTabControl.Items.Contains(_tabItem))
                    OwnerWindow.ContentTabControl.Items.Remove(_tabItem);*/
                if (OwnerWindow.Tabs.Count > 0)
                    OwnerWindow.Tabs.Remove(_tabItem.DataContext as FolderTabItem);
                else
                    OwnerWindow.Close();

            };

            _tabItem.PreviewMouseDown += (sneder, args) =>
            {
                if (args.MiddleButton == MouseButtonState.Pressed)
                {
                    if (OwnerWindow.Tabs.Count > 0)
                        OwnerWindow.Tabs.Remove(_tabItem.DataContext as FolderTabItem);
                    else
                        OwnerWindow.Close();
                }
            };

            _tabItem.PreviewMouseLeftButtonDown += (sneder, args) =>
            {
                //int mousePressCounter = 0;
                Timer timer = new Timer(10);
                Point initialPoint = SystemScaling.CursorPosition;
                _tabItem.RenderTransform = new TranslateTransform(0, 0);

                bool isDragging = false;
                timer.Elapsed += (sneder, args) =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (!isDragging) //mousePressCounter < 25)
                        {
                            if (Mouse.LeftButton == MouseButtonState.Released)
                                timer.Stop();
                            else if ((SystemScaling.CursorPosition.X > initialPoint.X + 10) || (SystemScaling.CursorPosition.X < initialPoint.X - 10) || (SystemScaling.CursorPosition.Y > initialPoint.Y + 10) || (SystemScaling.CursorPosition.Y < initialPoint.Y - 10))
                                isDragging = true;
                        }
                        else
                        {
                            if (Mouse.LeftButton == MouseButtonState.Released)
                            {
                                timer.Stop();
                                var data = _tabItem.DataContext as FolderTabItem;

                                /*if (Window.GetWindow(_tabItem) is MainWindow win)
                                {*/
                                //.RemoveTab(data);
                                //TabControl.ContainerFromElement()
                                //Debug.WriteLine("_tabItem.TemplatedParent type: " + _tabItem.TemplatedParent.GetType());
                                //var tabControl = _tabItem.Parent as TabControl;
                                if (SystemScaling.IsMouseWithin(OwnerControl))
                                {
                                    bool after = false;
                                    int index = -1;
                                    int oldIndex = OwnerWindow.Tabs.IndexOf(data);
                                    foreach (FolderTabItem f in OwnerWindow.Tabs/*OwnerControl.Items*/)
                                    {
                                        TabItem item = (TabItem)OwnerControl.ItemContainerGenerator.ContainerFromItem(f); //OwnerControl.item.ContainerFromElement(f);
                                        if ((item != _tabItem) && SystemScaling.IsMouseWithin(item))
                                        {
                                            index = /*OwnerControl.Items*/OwnerWindow.Tabs.IndexOf(f);
                                            double xMiddle = item.PointToScreen(new Point(0, 0)).X + (item.ActualWidth / 2);
                                            if (SystemScaling.CursorPosition.X > xMiddle)
                                                after = true;

                                            break;
                                        }
                                    }


                                    if ((index >= 0) && ((index + 1) < OwnerWindow.Tabs.Count))
                                    {
                                        if (after && ((index + 1) != oldIndex))
                                            OwnerWindow.Tabs.Move(OwnerWindow.Tabs.IndexOf(data), index + 1);
                                        else if (index != oldIndex)
                                            OwnerWindow.Tabs.Move(OwnerWindow.Tabs.IndexOf(data), index);
                                    }
                                }
                                else if (OwnerWindow.Tabs.Count > 1)
                                {
                                    MainWindow window = new MainWindow(); //WindowManager.CreateWindow();
                                    if (OwnerWindow.WindowState == WindowState.Maximized)
                                    {
                                        window.WindowState = WindowState.Maximized;

                                        if (OwnerWindow.IsFullscreen)
                                            window.IsFullscreen = true;
                                    }
                                    else
                                    {
                                        var s = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)OwnerWindow.Left, (int)window.Top));
                                        window.Left = Math.Clamp(OwnerWindow.Left + 15, s.WorkingArea.Left, s.WorkingArea.Right - 45);
                                        window.Top = Math.Clamp(OwnerWindow.Top + 15, s.WorkingArea.Top, s.WorkingArea.Bottom - 45);
                                    }
                                    OwnerWindow.RemoveTab(data);
                                    window.Tabs.Insert(0, data);
                                    window.Show();
                                    /*window.Tabs.Insert(0, data);
                                    window.CurrentTabIndex = 0;
                                    for (int i = 1; i < window.Tabs.Count; i++)
                                        window.Tabs.RemoveAt(1);*/
                                }
                                //}

                                if (false)
                                {
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
                                }
                                (_tabItem.RenderTransform as TranslateTransform).X = 0;
                                //TODO sliding animations
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
