using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Start9.Api;
using Start9.Api.DiskItems;
using Start9.Api.Controls;
using System.IO;
using System.Globalization;
using Fluent;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Microsoft.VisualBasic.FileIO;
using static Start9.Api.SystemScaling;
using static Start9.Api.WinApi;
using static Start9.Api.Extensions;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace RibbonFileManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DecoratableWindow
    {
        //TODO: Customization
        public ObservableCollection<DiskItem> Favorites
        {
            get => (ObservableCollection<DiskItem>)GetValue(FavoritesProperty);
            set => SetValue(FavoritesProperty, value);
        }

        public static readonly DependencyProperty FavoritesProperty = DependencyProperty.Register("Favorites", typeof(ObservableCollection<DiskItem>), typeof(MainWindow), new PropertyMetadata(new ObservableCollection<DiskItem>()));

        List<String> _userFolders = new List<String>(){ @"%userprofile%\Desktop", @"%userprofile%\Documents", @"%userprofile%\Downloads", @"%userprofile%\Music", @"%userprofile%\Pictures", @"%userprofile%\Videos", @"%systemdrive%" }; //TODO: Custom locations or something

        List<String> _moveCopyToFolders = new List<String>() { @"%userprofile%\Desktop", @"%userprofile%\Documents", @"%userprofile%\Downloads", @"%userprofile%\Music", @"%userprofile%\Pictures", @"%userprofile%\Videos" };

        public ObservableCollection<DiskItem> ComputerSubfolders
        {
            get => (ObservableCollection<DiskItem>)GetValue(ComputerSubfoldersProperty);
            set => SetValue(ComputerSubfoldersProperty, value);
        }

        public static readonly DependencyProperty ComputerSubfoldersProperty = DependencyProperty.Register("ComputerSubfolders", typeof(ObservableCollection<DiskItem>), typeof(MainWindow), new PropertyMetadata());

        public enum FileBrowserView
        {
            Icons,
            List,
            Details,
            Tiles,
            Content
        }

        public FileBrowserView CurrentView
        {
            get => (FileBrowserView)GetValue(CurrentViewProperty);
            set => SetValue(CurrentViewProperty, value);
        }

        public static readonly DependencyProperty CurrentViewProperty = DependencyProperty.Register("CurrentView", typeof(FileBrowserView), typeof(MainWindow), new PropertyMetadata(FileBrowserView.Icons));

        public Double IconSize
        {
            get => (Double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register("IconSize", typeof(Double), typeof(MainWindow), new PropertyMetadata((Double)48.0));

        /*public ObservableCollection<DiskItem> CurrentFolderContents
        {
            get
            {
                if (CurrentFolder != null)
                {
                    Debug.WriteLine(":D");
                    return CurrentFolder.SubItems;
                }
                else
                {
                    Debug.WriteLine("((HECK))");
                    return new ObservableCollection<DiskItem>();
                }
            }
            set
            {
            }
        }

        public DiskItem CurrentFolder
        {
            get => (DiskItem)GetValue(CurrentFolderProperty);
            set => SetValue(CurrentFolderProperty, value);
        }

        public static readonly DependencyProperty CurrentFolderProperty = DependencyProperty.Register("CurrentFolder", typeof(DiskItem), typeof(MainWindow), new PropertyMetadata(new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\"))));*/

        public List<String> HistoryList
        {
            get => (List<String>)GetValue(HistoryListProperty);
            set => SetValue(HistoryListProperty, value);
        }

        public static readonly DependencyProperty HistoryListProperty = DependencyProperty.Register("HistoryList", typeof(List<String>), typeof(MainWindow), new PropertyMetadata(new List<String>()));

        public Int32 HistoryIndex
        {
            get => (Int32)GetValue(HistoryIndexProperty);
            set => SetValue(HistoryIndexProperty, value);
        }

        public static readonly DependencyProperty HistoryIndexProperty = DependencyProperty.Register("HistoryIndex", typeof(Int32), typeof(MainWindow), new PropertyMetadata(0, OnHistoryIndexPropertyChangedCallback));

        static void OnHistoryIndexPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainWindow sender = (d as MainWindow);
            var oldValue = (Int32)e.OldValue;
            var newValue = (Int32)e.NewValue;

            sender.ValidateNavButtonStates();

            sender.Navigate(sender.HistoryList[newValue]);
        }

        private String _startupPath;

        public MainWindow()
        {
            _startupPath = @"%userprofile%";
            Initialize();

        }

        public MainWindow(String StartupPath)
        {
            _startupPath = StartupPath;
            Initialize();
        }

        void Initialize()
        {
            InitializeComponent();
            if ((Environment.OSVersion.Version.Major < 6) | ((Environment.OSVersion.Version.Major == 6) & (Environment.OSVersion.Version.Minor == 0)))
            {
                RibbonBackstageTabs.Items.Remove(PowerShellTabItem);
            }
            //_showQuickAccessInTitlebar = RibbonControl.ShowQuickAccessToolBarAboveRibbon;
            //RibbonControl.ShowQuickAccessToolBarAboveRibbon = false;
            RibbonControl.TitleBar = RibbonTitleBar;
            //CurrentFolderContents = new dis
            Manager.OpenWindows.Add(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (Manager.OpenWindows.Contains(this))
            {
                Manager.OpenWindows.Remove(this);
            }
        }

        private void MainWindow_Loaded(Object sender, RoutedEventArgs e)
        {
            NavBar.BackButton.Click += BackButton_Click;
            NavBar.ForwardButton.Click += ForwardButton_Click;
            NavBar.PathTextBox.PreviewMouseLeftButtonDown += NavBar_PathTextBox_PreviewMouseLeftButtonDown;
            NavBar.PathTextBox.KeyDown += NavBar_PathTextBox_KeyDown;
            NavBar.PathTextBox.LostFocus += NavBar_PathTextBox_LostFocus;
            //Debug.WriteLine(Environment.ExpandEnvironmentVariables(@"%userprofile%\Documents"));
            //this.CurrentFolder = new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\Documents"));
            //CurrentDirectoryListView.ItemsSource = new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%")).SubItems;
            ComputerSubfolders = new ObservableCollection<DiskItem>();// = new DiskItem(Environment.ExpandEnvironmentVariables(@"%systemdrive%\")).SubItems;
            foreach (var s in _userFolders)
                ComputerSubfolders.Add(new DiskItem(Environment.ExpandEnvironmentVariables(s)));

            foreach (var s in new List<String>() { @"%userprofile%\Documents", @"%windir%", @"%programfiles%" })
            {
                Favorites.Add(new DiskItem(Environment.ExpandEnvironmentVariables(s)));
            }
            //ComputerSubfolders.Insert(0, new DiskItem())
            //FavoritesTreeViewItem.ItemsSource = ;
            Navigate(Environment.ExpandEnvironmentVariables(_startupPath));
            ValidateNavButtonStates();

            Manager.Clipboard.CollectionChanged += Clipboard_CollectionChanged;
            Clipboard_CollectionChanged(null, null);
            CurrentDirectoryListView_SelectionChanged(null, null);

            foreach(var s in _moveCopyToFolders)
            {
                CreateMoveToMenuItem(Environment.ExpandEnvironmentVariables(s), false);
                CreateMoveToMenuItem(Environment.ExpandEnvironmentVariables(s), true);
            }
        }

        private void CreateMoveToMenuItem(String target, Boolean copy)
        {
            System.Windows.Controls.MenuItem item = new System.Windows.Controls.MenuItem()
            {
                Header = Path.GetFileName(target),
                Tag = target
            };

            item.Click += (sneder, args) =>
            {
                ObservableCollection<DiskItem> transferredItems = new ObservableCollection<DiskItem>();
                foreach (DiskItem d in CurrentDirectoryListView.SelectedItems)
                {
                    if (copy)
                    {
                        File.Copy(d.ItemPath, target + @"\" + Path.GetFileName(d.ItemPath));
                    }
                    else
                    {
                        File.Move(d.ItemPath, target + @"\" + Path.GetFileName(d.ItemPath));
                    }
                    transferredItems.Add(d);
                }

                var items = ((ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource);
                foreach (var d in transferredItems)
                {
                    if (items.Contains(d))
                    {
                        items.Remove(d);
                    }
                }
            };

            if (copy)
            {
                MoveToButton.Items.Add(item);
            }
            else
            {
                CopyToButton.Items.Add(item);
            }
        }

        private void Navigate(String targetPath)
        {
            var path = Environment.ExpandEnvironmentVariables(targetPath);
            if (path.EndsWith("lnk"))
            {
                path = new Shortcut(path).TargetPath;
            }

            if (Directory.Exists(path))
            {
                if (!(HistoryList.Contains(path)))
                {
                    HistoryList.Add(path);
                }

                HistoryIndex = HistoryList.IndexOf(path);
                Title = Path.GetFileName(path);
                CurrentDirectoryListView.ItemsSource = new DiskItem(path).SubItems;
                SetDetailsPane(path);
                //var treeSource = ComputerSubfolders;
                List<String> pathSegments = path.Split('\\').ToList();

                //foreach (TreeViewItem r in t.Items)
                var itemsList = (NavigationPane.Items[1] as TreeViewItem).Items;
                //for (int i = 0; i < itemsList.Count; i++)
                foreach (DiskItem d in ThisPcDiskTreeViewItem.Items)
                {
                    //DiskItem d = ComputerSubfolders[i];
                    //Debug.WriteLine("Current: " + d.ItemPath + "    " + pathSegments[1]);
                    //string targetSegment = RemovePathSegments(d.ItemPath, 1, pathSegments);
                    //Debug.WriteLine(d.ItemPath + "    " + pathSegments[1]);
                    //if (d.ItemPath.EndsWith(@"\" + pathSegments[1]))
                    //ComputerSubfolders[itemsList.IndexOf(t)].ItemPath
                    if (HistoryList[HistoryIndex].StartsWith(d.ItemPath))
                    {
                        //Debug.WriteLine("TARGET FOUND! " + d.ItemPath);
                        /*var item = (ItemsControl)(NavigationPaneDiskTreeItem.ItemContainerGenerator.ContainerFromIndex(NavigationPaneDiskTreeItem.Items.IndexOf(d)));
                        item.IsSelected = true;*/
                        //NavigationPane.SelectedValuePath = 
                        //NavigationPane.SelectedIndex = t;
                        //HandleTree(pathSegments, item);
                        //break;
                    }
                }

                NavBar.BreadcrumbsStackPanel.Children.Clear();
                foreach (var s in HistoryList[HistoryIndex].Split('\\'))
                {
                    SplitButton button = new SplitButton()
                    {
                        Header = s,
                        Style = (Style)Resources["BreadcrumbStyle"]
                    };
                    button.Click += (sneder, args) =>
                    {
                        var breadcrumbPath = "";
                        foreach(SplitButton b in NavBar.BreadcrumbsStackPanel.Children)
                        {
                            breadcrumbPath = breadcrumbPath + b.Header.ToString() + @"\";
                            if (b == button)
                            {
                                break;
                            }
                        }
                        Navigate(breadcrumbPath.Substring(0, breadcrumbPath.LastIndexOf(@"\")));
                    };
                    button.MouseRightButtonUp += (sneder, args) =>
                    {
                        NavBar_PathTextBox_PreviewMouseLeftButtonDown(sneder, args);
                    };

                    button.DropDownOpened += (sneder, args) =>
                    {
                        button.Items.Clear();

                        var breadcrumbPath = "";
                        foreach (SplitButton b in NavBar.BreadcrumbsStackPanel.Children)
                        {
                            breadcrumbPath = breadcrumbPath + b.Header.ToString() + @"\";

                            if (b == button)
                                break;
                        }
                        foreach (var p in Directory.EnumerateDirectories(breadcrumbPath))
                        {
                            System.Windows.Controls.MenuItem item = new System.Windows.Controls.MenuItem()
                            {
                                Header = Path.GetFileName(p)
                            };

                            item.Click += (sneeder, rags) => { Navigate(p); };

                            button.Items.Add(item);
                        }
                    };

                    NavBar.BreadcrumbsStackPanel.Children.Add(button);
                }
            }
            else
            {
                Process.Start(path);
            }
        }

        private String RemovePathSegments(String basePath, Int32 distance, List<String> pathSegments)
        {
            var returnPath = basePath;
            for (var i = 0; i < distance; i++)
            {
                returnPath = returnPath.Substring(pathSegments[i].Length + 1, returnPath.Length - (pathSegments[i].Length + 1));
            }
            return @"\" + returnPath;
        }

        /*private void HandleTree(List<string> pathSegments, TreeViewItem treeItem)
        {
            int currentSegment = 2;
            TreeViewItem item = treeItem;
            string clippedPath = "  ";
            while ((currentSegment < pathSegments.Count) & (clippedPath != ""))
            {
                if (item == null)
                    Debug.WriteLine("item == null");
                //bool flag = false;
                //pathSegments.Remove(pathSegments[0]);
                int dIndex = 0;
                foreach (DiskItem d in item.Items)
                {
                    dIndex += 1;
                    Debug.WriteLine(dIndex.ToString() + ": " + d.ItemPath);
                    //Debug.WriteLine("Iterating... " + pathSegments[currentSegment] + "\n" + d.ItemPath);
                    clippedPath = RemovePathSegments(d.ItemPath, currentSegment, pathSegments);
                    if (clippedPath.EndsWith(@"\" + pathSegments[currentSegment]))
                    {
                        //var diskItem = (TreeViewItem)(item.Items[((ObservableCollection<DiskItem>)(item.ItemsSource)).IndexOf(d)]);
                        //foreach (((ObservableCollection<DiskItem>)(item.ItemsSource)))
                        item = (TreeViewItem)(NavigationPane.ItemContainerGenerator.ContainerFromIndex(dIndex));
                        item.IsExpanded = true;
                        Debug.WriteLine("Found TreeViewItem: " + pathSegments[currentSegment]);
                        if (currentSegment < pathSegments.Count)
                            currentSegment += 1;
                    }
                    Debug.WriteLine(currentSegment.ToString() + "    " + pathSegments.Count);
                    if (currentSegment >= pathSegments.Count)
                    {
                        Debug.WriteLine("GREATER");
                        clippedPath = "";
                        break;
                    }
                }
            }
        }*/

        private void CurrentDirectoryListView_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            if (CurrentDirectoryListView.SelectedItems.Count == 0)
            {
                SetDetailsPane(HistoryList[HistoryIndex]);
                CopyButton.IsEnabled = false;
                CopyPathButton.IsEnabled = false;
                CutButton.IsEnabled = false;
                MoveToButton.IsEnabled = false;
                CopyToButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
                RenameButton.IsEnabled = false;
                OpenButton.IsEnabled = false;
                EditButton.IsEnabled = false;
            }
            else
            {
                if (CurrentDirectoryListView.SelectedItems.Count == 1)
                {
                    SetDetailsPane(((ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource)[CurrentDirectoryListView.SelectedIndex].ItemPath);
                }

                CopyButton.IsEnabled = true;
                CopyPathButton.IsEnabled = true;
                CutButton.IsEnabled = true;
                MoveToButton.IsEnabled = true;
                CopyToButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
                RenameButton.IsEnabled = true;
                OpenButton.IsEnabled = true;
                EditButton.IsEnabled = true;
            }
        }

        private void SetDetailsPane(String path, Boolean currentDir)
        {
            SetDetailsPane(path, false);
        }

        private void SetDetailsPane(String path)
        {

            var item = new DiskItem(path);
            var conv = new DiskItemToIconImageBrushOrThumbnailConverter();
            //var wpfIcon = (ImageBrush)(conv.Convert(item, typeof(Canvas), 16, null));
            ShFileInfo shInfo = new ShFileInfo();
            SHGetFileInfo(path, 0, ref shInfo, (UInt32)Marshal.SizeOf(shInfo), (0x00000000 | 0x100));

            Icon = (Imaging.CreateBitmapSourceFromHIcon(System.Drawing.Icon.FromHandle(shInfo.hIcon).Handle, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(System.Convert.ToInt32(RealPixelsToWpfUnits(16)), System.Convert.ToInt32(RealPixelsToWpfUnits(32)))));

            if (DetailsPane.IsVisible)
            {
                DetailsPaneIconCanvas.Background = (ImageBrush)(conv.Convert(item, typeof(Canvas), DetailsPaneIconCanvas.ActualHeight, null));
                DetailsPaneFileNameTextBlock.Text = item.ItemName;
                //DetailsPaneFileTypeTextBlock.Text = new FileInfo(path).Attributes;
                //Debug.WriteLine(new FileInfo(path).Attributes);
                DetailsPaneFileTypeTextBlock.Text = item.FriendlyItemType;
                //new FileInfo(path).Length
            }
        }

        private void CurrentDirectoryListView_Item_MouseDoubleClick(Object sender, MouseButtonEventArgs e)
        {
            if (CurrentDirectoryListView.SelectedItems.Count == 1)
            {
                Navigate(((ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource)[CurrentDirectoryListView.SelectedIndex].ItemPath);
            }
        }

        private void CurrentDirectoryListView_ItemContextMenu_Open_MouseLeftButtonUp(Object sender, MouseButtonEventArgs e)
        {
            CurrentDirectoryListView_Item_MouseDoubleClick(sender, null);
        }

        private void CurrentDirectoryListView_ItemContextMenu_Cut_MouseLeftButtonUp(Object sender, MouseButtonEventArgs e)
        {
            CutButton_Click(sender, null);
        }

        private void CurrentDirectoryListView_ItemContextMenu_Copy_MouseLeftButtonUp(Object sender, MouseButtonEventArgs e)
        {
            CopyButton_Click(sender, null);
        }

        private void CurrentDirectoryListView_ItemContextMenu_CreateShortcut_MouseLeftButtonUp(Object sender, MouseButtonEventArgs e)
        {
            
        }

        private void CurrentDirectoryListView_ItemContextMenu_Delete_MouseLeftButtonUp(Object sender, MouseButtonEventArgs e)
        {
            DeleteButton_Click(sender, null);
        }

        private void CurrentDirectoryListView_ItemContextMenu_Rename_MouseLeftButtonUp(Object sender, MouseButtonEventArgs e)
        {
            RenameButton_Click(sender, null);
        }

        private void CurrentDirectoryListView_Item_MouseRightButtonUp(Object sender, MouseButtonEventArgs e)
        {
            if (CurrentDirectoryListView.SelectedItems.Count == 1)
            {
                //Navigate(((ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource)[CurrentDirectoryListView.SelectedIndex].ItemPath);

            }
        }

        private void BackButton_Click(Object sender, RoutedEventArgs e)
        {
            if (HistoryIndex > 0)
            {
                HistoryIndex--;
            }
        }

        private void ForwardButton_Click(Object sender, RoutedEventArgs e)
        {
            if (HistoryIndex < HistoryList.Count)
            {
                HistoryIndex++;
            }
        }

        public void ValidateNavButtonStates()
        {
            if (HistoryIndex == 0)
            {
                NavBar.BackButton.IsEnabled = false;
            }
            else
            {
                NavBar.BackButton.IsEnabled = true;
            }

            if (HistoryIndex >= (HistoryList.Count - 1))
            {
                NavBar.ForwardButton.IsEnabled = false;
            }
            else
            {
                NavBar.ForwardButton.IsEnabled = true;
            }
        }

        private void BackButton_MouseRightButtonUp(Object sender, MouseButtonEventArgs e)
        {

        }

        private void ForwardButton_MouseRightButtonUp(Object sender, MouseButtonEventArgs e)
        {

        }

        /*private void RibbonControl_IsMinimizedChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            if (RibbonControl.IsMinimized)
                ToolBarHeight = 50;
            else
                ToolBarHeight = RibbonControl.Height + 50;
        }*/

        private void NavigationPane_SelectedItemChanged(Object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
            if (e.NewValue != FavoritesTreeViewItem)
            {
                Navigate((NavigationPane.SelectedItem as DiskItem).ItemPath);
            }
        }

        ///https://stackoverflow.com/questions/23133527/wpf-listbox-remove-selection-by-clicking-on-a-blank-spot/23138479#23138479
        private static T GetParentOfType<T>(DependencyObject element)
        where T : DependencyObject
        {
            Type type = typeof(T);
            if (element == null) return null;
            DependencyObject parent = VisualTreeHelper.GetParent(element);
            if (parent == null && ((FrameworkElement)element).Parent is DependencyObject)
                parent = ((FrameworkElement)element).Parent;
            if (parent == null) return null;
            else if (parent.GetType() == type || parent.GetType().IsSubclassOf(type))
                return parent as T;
            return GetParentOfType<T>(parent);
        }

        private void CurrentDirectoryListView_PreviewMouseLeftButtonDown(Object sender, MouseButtonEventArgs e)
        {
            /*Timer dragSelectTimer = new Timer(1);
            Point cursorInitial = SystemScaling.CursorPosition;
            Point cursorStart = SystemScaling.CursorPosition;
            Point cursorNow = SystemScaling.CursorPosition;
            Point listPoint = CurrentDirectoryListView.PointToScreenInWpfUnits(new Point(0, 0));
            int interval = 0;
            bool isDragSelect = false;
            bool startedDragSelect = false;
            Rect borderDimensions = new Rect(0, 0, 0, 0);

            dragSelectTimer.Elapsed += (sneder, args) =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        Debug.WriteLine("TICK TOCK");
                        interval++;
                        if (interval < 100)
                        {
                            cursorStart = SystemScaling.CursorPosition;
                        }
                        else if (interval == 100)
                        {
                            if (((cursorStart.X < cursorInitial.X - 10) | (cursorStart.X > cursorInitial.X + 10)) | ((cursorStart.Y < cursorInitial.Y - 10) | (cursorStart.Y > cursorInitial.Y + 10)))
                            {
                                isDragSelect = true;
                            }
                        }
                        else if (isDragSelect)
                        {
                            if (startedDragSelect)
                            {
                                cursorNow = SystemScaling.CursorPosition;
                                if (cursorNow.X > cursorStart.X)
                                {
                                    borderDimensions.X = cursorStart.X - listPoint.X;
                                    borderDimensions.Width = cursorNow.X - cursorStart.X;
                                }
                                else
                                {
                                    borderDimensions.X = cursorNow.X - listPoint.X;
                                    borderDimensions.Width = cursorStart.X - cursorNow.X;
                                }

                                if (cursorNow.Y > cursorStart.Y)
                                {
                                    borderDimensions.Y = cursorStart.Y - listPoint.Y;
                                    borderDimensions.Height = cursorNow.Y - cursorStart.Y;
                                }
                                else
                                {
                                    borderDimensions.Y = cursorNow.Y - listPoint.Y;
                                    borderDimensions.Height = cursorStart.Y - cursorNow.Y;
                                }

                                SelectionBorder.Margin = new Thickness(borderDimensions.X, borderDimensions.Y, borderDimensions.X * -1, borderDimensions.Y * -1);
                                SelectionBorder.Width = borderDimensions.Width;
                                SelectionBorder.Height = borderDimensions.Height;
                            }
                            else
                            {
                                /*SelectionBorder.Visibility = Visibility.Hidden;
                                var source = ((ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource);

                                for (int i = 0; i < source.Count; i++)
                                {
                                    ListViewItem item = CurrentDirectoryListView.ItemContainerGenerator.ContainerFromIndex(i);
                                    Point topLeft = item.PointToScreenInWpfUnits(new Point(0, 0));
                                    Point bottomRight = new Point(topLeft.X + item.ActualWidth, topLeft.GetHashCode + item.ActualHeight);
                                    List<Point> points = new List<Point>();
                                    points.Add(topLeft);
                                    points.Add(bottomRight);
                                    points.Add(new Point(topLeft.X, bottomRight.Y));
                                    points.Add(new Point(bottomRight.X, topLeft.Y));

                                    Point selectionBorderPoint = SelectionBorder.PointToScreen(new Point(0, 0);
                                    foreach (Point p in points)
                                    {
                                        if ((p.X > selectionBorderPoint.X) & (p.X < selectionBorderPoint.X))
                                        {

                                        }
                                    }
                                }*

                                dragSelectTimer.Stop();
                            }
                        }
                        else
                        {
                            cursorStart = SystemScaling.CursorPosition;
                            SelectionBorder.Visibility = Visibility.Visible;
                            startedDragSelect = true;
                        }
                    }
                    else
                    {
                        dragSelectTimer.Stop();
                    }
                }));
            };
            dragSelectTimer.Start();*/

            SelectionBorder.Visibility = Visibility.Hidden;
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(CurrentDirectoryListView, e.GetPosition(CurrentDirectoryListView));
            if (GetParentOfType<ListViewItem>(hitTestResult.VisualHit) == null)
            {
                CurrentDirectoryListView.SelectedItem = null;
            }
        }

        private void CurrentDirectoryListView_KeyDown(Object sender, KeyEventArgs e)
        {
            foreach(var d in ((ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource))
            {
                try
                {
                    if (d.ItemName.StartsWith(e.Key.ToString()))
                    {
                        if ((((DiskItem)CurrentDirectoryListView.SelectedItem).ItemName.StartsWith(e.Key.ToString())) && CurrentDirectoryListView.SelectedIndex < (CurrentDirectoryListView.Items.Count - 1))
                        {
                            if (((DiskItem)((ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource)[CurrentDirectoryListView.SelectedIndex + 1]).ItemName.StartsWith(e.Key.ToString()))
                            {
                                CurrentDirectoryListView.SelectedIndex++;
                            }
                            else
                            {
                                foreach (var i in ((ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource))
                                {
                                    CurrentDirectoryListView.SelectedItem = i;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            CurrentDirectoryListView.SelectedItem = d;
                        }
                        break;
                    }
                }
                catch (System.NullReferenceException ex) { Debug.WriteLine(ex); }
            }
        }

        private void Clipboard_CollectionChanged(Object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Manager.Clipboard.Count == 0)
            {
                PasteButton.IsEnabled = false;
                PasteShortcutButton.IsEnabled = false;
            }
            else
            {
                PasteButton.IsEnabled = true;
                PasteShortcutButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// RIBBON BACKSTAGE FUNCTIONS START HERE
        /// </summary>

        private void NewWindowButton_Click(Object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            ResetBackstage();
        }

        private void CloseWindowButton_Click(Object sender, RoutedEventArgs e)
        {
            Close();
            ResetBackstage();
        }

        private void OpenCmdButton_Click(Object sender, RoutedEventArgs e)
        {
            Process.Start("cmd.exe");
            ResetBackstage();
        }

        private void OpenPowerShellButton_Click(Object sender, RoutedEventArgs e)
        {
            Process.Start("powershell.exe");
            ResetBackstage();
        }

        private void RibbonBackstage_IsOpenChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!RibbonBackstage.IsOpen)
            {
                ResetBackstage();
            }
        }

        private void ResetBackstage()
        {

            RibbonBackstage.IsOpen = false;
            RibbonBackstageTabs.SelectedIndex = -1;
        }

        private void NavBar_PathTextBox_PreviewMouseLeftButtonDown(Object sender, MouseButtonEventArgs e)
        {
            NavBar.BreadcrumbsStackPanel.Visibility = Visibility.Hidden;
            NavBar.PathTextBox.Text = HistoryList[HistoryIndex];
            NavBar.PathTextBox.SelectAll();
        }

        private void NavBar_PathTextBox_GotFocus(Object sender, RoutedEventArgs e)
        {
        }

        private void NavBar_PathTextBox_LostFocus(Object sender, RoutedEventArgs e)
        {
            NavBar.BreadcrumbsStackPanel.Visibility = Visibility.Visible;
            NavBar.PathTextBox.Text = "";
        }

        private void NavBar_PathTextBox_KeyDown(Object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Directory.Exists(Environment.ExpandEnvironmentVariables(NavBar.PathTextBox.Text)))
                {
                    Navigate(NavBar.PathTextBox.Text);
                    CurrentDirectoryListView.Focus();
                    NavBar_PathTextBox_LostFocus(null, null);
                }
                else
                {
                    var failText = NavBar.PathTextBox.Text;
                    Start9.Api.Plex.MessageBox.Show(null, "Ribbon File Browser can't find '" + failText + "'. Check the speeling and try again.", "Ribbon File Browser");
                }
            }
            else if (e.Key == Key.Escape)
            {
                NavBar_PathTextBox_LostFocus(null, null);
            }
        }

        /// <summary>
        /// RIBBON BUTTON FUNCTIONS START HERE
        /// </summary>

        private void CopyButton_Click(Object sender, RoutedEventArgs e)
        {
            Manager.Cut = false;
            SetClipboard();
        }

        private void CutButton_Click(Object sender, RoutedEventArgs e)
        {
            Manager.Cut = true;
            SetClipboard();
        }

        private void CopyPathButton_Click(Object sender, RoutedEventArgs e)
        {
            var paths = "";
            for (var i = 0; i < CurrentDirectoryListView.SelectedItems.Count; i++)
            {
                var d = (DiskItem)(CurrentDirectoryListView.SelectedItems[i]);
                paths = paths + "\"" + d.ItemPath + "\"";
                if (i < (CurrentDirectoryListView.SelectedItems.Count - 1))
                {
                    paths = paths + "\n";
                }
            }
            System.Windows.Clipboard.SetText(paths);
        }

        private void SetClipboard()
        {
            Manager.Clipboard.Clear();
            foreach (DiskItem d in CurrentDirectoryListView.SelectedItems)
            {
                Manager.Clipboard.Add(d);
            }
        }

        private void PasteShortcutButton_Click(Object sender, RoutedEventArgs e)
        {
            foreach (DiskItem d in CurrentDirectoryListView.SelectedItems)
            {
                Shortcut.CreateShortcut(d.ItemName + " - Shortcut", null, d.ItemPath, HistoryList[HistoryIndex]);
            }
        }

        private void PasteButton_Click(Object sender, RoutedEventArgs e)
        {
            var items = Manager.CopyTo(HistoryList[HistoryIndex]);
            var source = ((ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource);
            foreach (var d in items)
            {
                if (source.Contains(d))
                {
                    source.Remove(d);
                }
                else
                {
                    source.Add(d);
                }
            }
        }

        private void DeleteButton_Click(Object sender, RoutedEventArgs e)
        {
            var source = ((ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource);
            for (var i = 0; i < CurrentDirectoryListView.SelectedItems.Count; i++)
            {
                var d = CurrentDirectoryListView.SelectedItems[i] as DiskItem;

                if (source.Contains(d))
                {
                    if (d.ItemType == DiskItem.DiskItemType.Directory)
                        FileSystem.DeleteDirectory(d.ItemPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                    else if (d.ItemType != DiskItem.DiskItemType.App)
                        FileSystem.DeleteFile(d.ItemPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);

                    if (d.ItemType != DiskItem.DiskItemType.App)
                        source.Remove(d);
                    i--;
                }
            }
        }

        private void RenameButton_Click(Object sender, RoutedEventArgs e)
        {
            //var source = ((ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource);
            //DiskItem selected = CurrentDirectoryListView.SelectedItem as DiskItem;
            if (sender is System.Windows.Controls.MenuItem)
                (sender as System.Windows.Controls.MenuItem).IsEnabled = false;
        }

        private void NewFolderButton_Click(Object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(HistoryList[HistoryIndex] + @"\New Folder");
        }

        private void PropertiesButton_Click(Object sender, RoutedEventArgs e)
        {
            var d = CurrentDirectoryListView.SelectedItem as DiskItem;
            if ((d.ItemType == DiskItem.DiskItemType.File) | (d.ItemType == DiskItem.DiskItemType.Shortcut))
            {
                ProcessStartInfo info = new ProcessStartInfo()
                {
                    Verb = "Properties"
                };
                Process.Start(info);
            }
            else
            {

            }
        }

        private void OpenButton_Click(Object sender, RoutedEventArgs e)
        {

        }

        private void EditButton_Click(Object sender, RoutedEventArgs e)
        {

        }

        private void HistoryButton_Click(Object sender, RoutedEventArgs e)
        {

        }

        private void SelectAllButton_Click(Object sender, RoutedEventArgs e)
        {
            /*foreach (DiskItem d in ((ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource))
            {
                d.Selected = true;
            }*/
            CurrentDirectoryListView.SelectAll();
        }

        private void SelectNoneButton_Click(Object sender, RoutedEventArgs e)
        {
            /*foreach (DiskItem d in ((ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource))
            {
                d.Selected = false;
            }*/
            CurrentDirectoryListView.SelectedItem = null;
        }

        private void InvertSelectionButton_Click(Object sender, RoutedEventArgs e)
        {
            if (CurrentDirectoryListView.SelectedItem == null)
            {
                CurrentDirectoryListView.SelectAll();
            }
            else if (CurrentDirectoryListView.SelectedItems.Count == ((ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource).Count)
            {
                CurrentDirectoryListView.SelectedItem = null;
            }
            else
            {
                foreach (var d in ((ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource))
                {
                    d.Selected = (!(d.Selected));
                }
            }
        }

        private void GetModules_Click(Object sender, RoutedEventArgs e)
        {
            var count = RibbonFileManagerAddIn.Instance.Host.GetModules().Count;
            Start9.Api.Plex.MessageBox.Show(null, $"Modules successfully received - Count = {count}", "Modules received");
        }
    }
}
