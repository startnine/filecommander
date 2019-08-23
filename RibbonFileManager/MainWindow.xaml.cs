using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Start9.UI.Wpf;
using Start9.UI.Wpf.Windows;

namespace RibbonFileManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DecoratableWindow
    {
        public TabControl ContentTabControl
        {
            get
            {
                if (Config.Instance.TabsMode == Config.TabDisplayMode.Titlebar)
                    return TitlebarTabControl;
                else if (Config.Instance.TabsMode == Config.TabDisplayMode.Toolbar)
                    return ToolbarTabControl;
                else
                    return null;
            }
        }

        public ObservableCollection<FolderTabItem> Tabs
        {
            get => (ObservableCollection<FolderTabItem>)GetValue(TabsProperty);
            set => SetValue(TabsProperty, value);
        }

        public static readonly DependencyProperty TabsProperty =
            DependencyProperty.Register(nameof(Tabs), typeof(ObservableCollection<FolderTabItem>), typeof(MainWindow), new PropertyMetadata(OnTabPropertiesChangedCallback));

        public int CurrentTabIndex
        {
            get => (int)GetValue(CurrentTabIndexProperty);
            set => SetValue(CurrentTabIndexProperty, value);
        }

        public static readonly DependencyProperty CurrentTabIndexProperty =
            DependencyProperty.Register(nameof(CurrentTabIndex), typeof(int), typeof(MainWindow), new PropertyMetadata(0, OnTabPropertiesChangedCallback));

        static void OnTabPropertiesChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MainWindow win)
                win.UpdateCurrentTab();
        }

        public void UpdateCurrentTab()
        {
            if ((CurrentTabIndex >= 0) && (CurrentTabIndex < Tabs.Count))
                CurrentTab = Tabs.ElementAt(CurrentTabIndex);
            else
                CurrentTab = null;
        }

        public FolderTabItem CurrentTab
        {
            get => (FolderTabItem)GetValue(CurrentTabProperty);
            set => SetValue(CurrentTabProperty, value);
        }

        public static readonly DependencyProperty CurrentTabProperty =
            DependencyProperty.Register(nameof(CurrentTab), typeof(FolderTabItem), typeof(MainWindow), new FrameworkPropertyMetadata(null, OnCurrentTabChangedCallback));

        static void OnCurrentTabChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MainWindow win)
            {
                if ((win.CurrentTab != null) && win.CurrentTab.Content.IsLoaded)
                {
                    win.Navigate(win.CurrentTab.Content.CurrentLocation);
                    win.NavigationPaneMenuItem.IsChecked = win.CurrentTab.Content.ShowNavigationPane;
                    win.DetailsPaneToggleButton.IsChecked = win.CurrentTab.Content.ShowDetailsPane;
                    win.PreviewPaneToggleButton.IsChecked = win.CurrentTab.Content.ShowPreviewPane;
                    if (win.CurrentTab.Content.CurrentLocation is DirectoryQuery query)
                        win.ValidateCommandStates(query.Item.SubItems.Count, query.Item);
                }
            }
            /*if ((d is MainWindow win) && (win.CurrentTab != null))
            {
                win.TabContentDisplayContentControl.Content = win.CurrentTab.Content;
                Debug.WriteLine("Setting TabContentDisplayContentControl content");
            }
            else
                Debug.WriteLine("CurrentTab was null!");*/
        }

        //public IEnumerable<WindowContent> WindowContents => ContentTabControl.Items.OfType<WindowContent>();

        public Config.InterfaceModeType InterfaceMode
        {
            get => (Config.InterfaceModeType)GetValue(InterfaceModeProperty);
            set => SetValue(InterfaceModeProperty, value);
        }

        public static readonly DependencyProperty InterfaceModeProperty =
            DependencyProperty.Register("InterfaceMode", typeof(Config.InterfaceModeType), typeof(MainWindow), new FrameworkPropertyMetadata(Config.InterfaceModeType.Ribbon, FrameworkPropertyMetadataOptions.AffectsRender, OnInterfaceModeChangedCallback));

        static void OnInterfaceModeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MainWindow).UpdateInterfaceMode();
        }

        void UpdateInterfaceMode()
        {
            if (InterfaceMode == Config.InterfaceModeType.CommandBar)
            {
                CommandBarControl.Visibility = Visibility.Visible;
                MenuBar.IsEnabled = true;
                MenuBarToolBar.Visibility = Visibility.Visible;
                DefaultToolBar.Visibility = Visibility.Collapsed;
                Ribbon.Visibility = Visibility.Collapsed;
                NavigationBarGrid.Visibility = Visibility.Visible;
            }
            else if (InterfaceMode == Config.InterfaceModeType.Ribbon)
            {
                CommandBarControl.Visibility = Visibility.Collapsed;
                MenuBar.IsEnabled = false;
                MenuBarToolBar.Visibility = Visibility.Collapsed;
                DefaultToolBar.Visibility = Visibility.Collapsed;
                Ribbon.Visibility = Visibility.Visible;

                NavigationBarGrid.Visibility = Visibility.Visible;
            }
            else if (InterfaceMode == Config.InterfaceModeType.None)
            {
                CommandBarControl.Visibility = Visibility.Collapsed;
                MenuBar.IsEnabled = true;
                MenuBarToolBar.Visibility = Visibility.Visible;
                DefaultToolBar.Visibility = Visibility.Visible;
                Ribbon.Visibility = Visibility.Collapsed;
                NavigationBarGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void Config_ConfigUpdated(object sender, EventArgs e)
        {
            if (sender is Config Instance)
            {
                TitlebarTabControl.Visibility = Visibility.Collapsed;
                ToolbarTabControl.Visibility = Visibility.Collapsed;
                if (Config.Instance.InterfaceMode == Config.InterfaceModeType.Ribbon)
                    RibbonTitle.Visibility = Visibility.Visible;
                else
                    RibbonTitle.Visibility = Visibility.Collapsed;
                TabsSetAsideToggleButton.Visibility = Visibility.Collapsed;
                SetTabsAsideButton.Visibility = Visibility.Collapsed;
                NewTabButton.Visibility = Visibility.Collapsed;
                //BindingOperations.ClearBinding(TabContentDisplayContentControl, ContentProperty);
                if (Instance.TabsMode == Config.TabDisplayMode.Titlebar)
                {
                    TitlebarTabControl.Visibility = Visibility.Visible;
                    RibbonTitle.Visibility = Visibility.Collapsed;
                    TabsSetAsideToggleButton.Visibility = Visibility.Visible;
                    SetTabsAsideButton.Visibility = Visibility.Visible;
                    NewTabButton.Visibility = Visibility.Visible;
                    /*var contentBinding = new Binding()
                    {
                        Source = TitlebarTabControl,
                        Path = new PropertyPath("SelectedContent"),
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding(TabContentDisplayContentControl, ContentProperty, contentBinding);*/
                }
                else if (Instance.TabsMode == Config.TabDisplayMode.Toolbar)
                {
                    ToolbarTabControl.Visibility = Visibility.Visible;
                    /*var contentBinding = new Binding()
                    {
                        Source = ToolbarTabControl,
                        Path = new PropertyPath("SelectedContent"),
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding(TabContentDisplayContentControl, ContentProperty, contentBinding);*/
                }
            }
        }

        Location _firstNavigationPath = WindowManager.WindowDefaultLocation;

        public static BreadcrumbsPathToItemsConverter Converter { get; } = new BreadcrumbsPathToItemsConverter((path) =>
        {
            if (path.Contains("Search results in"))
            {
                return new[] { new BreadcrumbItem(path, path) };
            }

            var sb = new StringBuilder(path.Length);
            var items = new List<BreadcrumbItem>();

            foreach (var p in path.Split('\\'))
            {
                sb.Append(p);
                sb.Append("\\");
                items.Add(new BreadcrumbItem(p, sb.ToString().Trim('\\')));
            }

            return items.ToArray();
        });

        public MainWindow()
        {
            SetCurrentValue(TabsProperty, new ObservableCollection<FolderTabItem>());
            InitializeComponent();

            AddressBox.Converter = Converter;

            Activated += MainWindow_Activated;

            var interfaceModeBinding = new Binding()
            {
                Source = Config.Instance,
                Path = new PropertyPath("InterfaceMode"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(this, InterfaceModeProperty, interfaceModeBinding);
            UpdateInterfaceMode();

            Tabs.CollectionChanged += (sneder, args) => UpdateCurrentTab();

            Config_ConfigUpdated(Config.Instance, null);
            Config.ConfigUpdated += Config_ConfigUpdated;

            WindowManager.OpenWindows.Add(this);
        }

        private void MainWindow_Activated(Object sender, EventArgs e)
        {
            UpdateClipboardButtons();
        }

        public void UpdateClipboardButtons()
        {
            if (Clipboard.ContainsFileDropList())
            {
                PasteButton.IsEnabled = true;
                PasteShortcutButton.IsEnabled = true;
            }
            else
            {
                PasteButton.IsEnabled = false;
                PasteShortcutButton.IsEnabled = false;
            }
        }

        /*MainWindow _copyWindow = null;
        public MainWindow(MainWindow copyWindow)
        {
            _copyWindow = copyWindow;
            Initialize();
        }

        public MainWindow(Location path)
        {
            _firstNavigationPath = path;
            Initialize();
        }*/

        private void MainWindow_Loaded(Object sender, RoutedEventArgs e)
        {
            Config.ClipboardContents.CollectionChanged += ClipboardContents_CollectionChanged;
            /*if (_copyWindow != null)
            {
                foreach (var w in _copyWindow.Tabs)
                {
                    if (w.Content.NavManager.Current is DirectoryQuery query)
                        AddTab(query);
                    else if (w.Content.NavManager.Current is ShellLocation location)
                        AddTab(location);
                    else if (w.Content.NavManager.Current is SearchQuery search)
                    {
                        //TODO figure out what to do here lol
                    }
                }
                //AddTab((w.CurrentLocation as DirectoryQuery).Item.ItemPath);
            }
            else*/
            if (Tabs.Count == 0)
                AddTab(_firstNavigationPath);

            //TabContentDisplayContentControl.Content = CurrentTab.Content;

            ContentTabControl.SelectedIndex = 0;
            ClipboardContents_CollectionChanged(null, null);
            ValidateCommandStates(0, null);

            Debug.WriteLine("Is CurrentTab null? " + (CurrentTab == null));

            Loaded -= MainWindow_Loaded;
        }

        public FolderTabItem AddTab(Location location)
        {
            if (Config.Instance.TabsMode != Config.TabDisplayMode.Disabled)
            {
                var item = new FolderTabItem(location);
                Tabs.Add(item);
                CurrentTabIndex = Tabs.IndexOf(item);
                return item;
            }
            else
                return null;
        }

        private void CloseCurrentLocation()
        {
            if (Config.Instance.TabsMode != Config.TabDisplayMode.Disabled)
                RemoveTab(CurrentTab);
            else
                Close();
        }

        public void RemoveTab(FolderTabItem item)
        {
            if (Tabs.Count <= 1)
                Close();
            else
            {
                if (CurrentTabIndex > 0)
                    CurrentTabIndex--;
                Tabs.Remove(item);
            }
        }

        public void ValidateNavButtonStates()
        {
            NavBackButton.IsEnabled = CurrentTab.Content.NavManager.CanGoBack;
            NavForwardButton.IsEnabled = CurrentTab.Content.NavManager.CanGoForward;
            if (CurrentTab.Content.NavManager.Current is DirectoryQuery query)
                NavUpButton.IsEnabled = Directory.GetParent(query.Item.ItemPath) != null;
            else
                NavUpButton.IsEnabled = false;
        }

        public void ValidateCommandStates(Int32 selectedCount, DiskItem activeItem)
        {
            if (selectedCount == 0)
            {
                CopyButton.IsEnabled = false;
                CopyPathButton.IsEnabled = false;
                CutButton.IsEnabled = false;
                MoveToButton.IsEnabled = false;
                CopyToButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
                RenameButton.IsEnabled = false;
                OpenButton.IsEnabled = false;
                EditButton.IsEnabled = false;

                CommandBarControl.CommandBarLayers[2].IsVisible = false;
                CommandBarControl.CommandBarLayers[3].IsVisible = false;

                ArchiveToolsGroup.Visibility = Visibility.Collapsed;

                SelectedItemCounter.Visibility = Visibility.Collapsed;
            }
            else
            {
                CopyButton.IsEnabled = true;
                CopyPathButton.IsEnabled = true;
                CutButton.IsEnabled = true;
                MoveToButton.IsEnabled = true;
                CopyToButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
                RenameButton.IsEnabled = true;
                OpenButton.IsEnabled = true;
                EditButton.IsEnabled = true;

                if ((selectedCount == 1) && (activeItem.ItemCategory == DiskItemCategory.Directory))
                {
                    CommandBarControl.CommandBarLayers[2].IsVisible = false;
                    CommandBarControl.CommandBarLayers[3].IsVisible = true;
                }
                else
                {
                    CommandBarControl.CommandBarLayers[2].IsVisible = true;
                    CommandBarControl.CommandBarLayers[3].IsVisible = false;
                }

                if ((selectedCount == 1) && (Path.GetExtension(activeItem.ItemRealName).ToLowerInvariant() == ".zip"))
                    ArchiveToolsGroup.Visibility = Visibility.Visible;
                else
                    ArchiveToolsGroup.Visibility = Visibility.Collapsed;


                if ((activeItem.ItemCategory != DiskItemCategory.Directory) && (activeItem.ItemCategory != DiskItemCategory.App))
                {
                    var openWith = activeItem.GetOpenWithPrograms();
                }

                SelectedItemCounter.Visibility = Visibility.Visible;
                SelectedItemCounter.Content = selectedCount + " items selected " + activeItem.FriendlyItemSize;
            }
        }

        private void ClipboardContents_CollectionChanged(Object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Config.ClipboardContents.Count == 0)
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

        Boolean _newWindowSubmenuItemClick = false;

        private void NewWindowButton_Click(Object sender, RoutedEventArgs e)
        {
            /*if (sender == NewWindowMenuItem)
            {
                if (_newWindowSubmenuItemClick)
                    _newWindowSubmenuItemClick = false;
                else
                    WindowManager.CloneWindow(this);
            }
            else
            {
                _newWindowSubmenuItemClick = true;

                if (sender == NewWindowSubmenuItem)
                    WindowManager.CloneWindow(this);
                else if (sender == NewWindowDefaultLocationButton)
                    WindowManager.CreateWindow();
            }*/
        }

        private void CurrentViewGalleryItem_Click(Object sender, RoutedEventArgs e)
        {

        }

        private void CloseWindowButton_Click(Object sender, RoutedEventArgs e)
        {
            Close();
        }

        String cmdName = "cmd.exe";
        String psName = "powershell.exe";

        private void OpenCmdButton_Click(Object sender, RoutedEventArgs e)
        {
            if (sender == OpenCmdAdminButton)
                Process.Start(new ProcessStartInfo(cmdName)
                {
                    Verb = "runas"
                });
            else
                Process.Start(cmdName);
        }

        private void OpenPowerShellButton_Click(Object sender, RoutedEventArgs e)
        {
            if (sender == OpenPowerShellAdminButton)
                Process.Start(new ProcessStartInfo(psName)
                {
                    Verb = "runas"
                });
            else
                Process.Start(psName);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            WindowManager.OpenWindows.Remove(this);
            base.OnClosing(e);
        }

        private async void OpenButton_Click(Object sender, RoutedEventArgs e)
        {
            await CurrentTab.Content.OpenSelectionAsync();
        }

        private void CopyButton_Click(Object sender, RoutedEventArgs e)
        {
            CurrentTab.Content.CopySelection();
        }

        private void CopyPathButton_Click(Object sender, RoutedEventArgs e)
        {
            CurrentTab.Content.CopyPathToSelection();
        }

        private async void PasteButton_Click(Object sender, RoutedEventArgs e)
        {
            await CurrentTab.Content.PasteCurrentAsync();
        }

        private async void PasteShortcutButton_Click(Object sender, RoutedEventArgs e)
        {
            await CurrentTab.Content.PasteShortcutAsync();
        }

        private void CutButton_Click(Object sender, RoutedEventArgs e)
        {
            CurrentTab.Content.CutSelection();
        }

        private void EditButton_Click(Object sender, RoutedEventArgs e)
        {
            
        }

        private async void DeleteButton_Click(Object sender, RoutedEventArgs e)
        {
            await CurrentTab.Content.DeleteSelectionAsync();
        }

        private void PropertiesButton_Click(Object sender, RoutedEventArgs e)
        {
            CurrentTab.Content.ShowPropertiesForSelection();
        }

        private void RenameButton_Click(Object sender, RoutedEventArgs e)
        {
            CurrentTab.Content.RenameSelection();
        }

        private void HistoryButton_Click(Object sender, RoutedEventArgs e)
        {

        }

        private async void NewFolderButton_Click(Object sender, RoutedEventArgs e)
        {
            await CurrentTab.Content.CreateNewFolderAsync();
        }

        private void SelectAllButton_Click(Object sender, RoutedEventArgs e)
        {
            CurrentTab.Content.SelectAll();
        }

        private void SelectNoneButton_Click(Object sender, RoutedEventArgs e)
        {
            CurrentTab.Content.SelectNone();
        }

        private void InvertSelectionButton_Click(Object sender, RoutedEventArgs e)
        {
            CurrentTab.Content.InvertSelection();
        }

        private void FolderAndSearchOptionsMenuItem_Click(Object sender, RoutedEventArgs e)
        {
            Config.SettingsWindow.Show();
            Config.SettingsWindow.Focus();
            Config.SettingsWindow.Activate();
        }

        private void ToggleMenuBar()
        {
            if (MenuBar.IsEnabled)
            {
                if (MenuBar.Visibility == Visibility.Visible)
                {
                    MenuBar.Visibility = Visibility.Collapsed;
                    CurrentTab.Content.CurrentDirectoryListView.Focus();
                }
                else
                {
                    MenuBar.Visibility = Visibility.Visible;
                    MenuBar.Focus();
                }
            }
        }

        bool _syncView = false;

        private void FolderViewsGallery_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            if (!_syncView)
            {
                if (FolderViewsGallery.SelectedItem == ListViewGalleryItem)
                    CurrentTab.Content.CurrentView = FileBrowserView.List;
                else if (FolderViewsGallery.SelectedItem == DetailsViewGalleryItem)
                    CurrentTab.Content.CurrentView = FileBrowserView.Details;
                else if (FolderViewsGallery.SelectedItem == TilesViewGalleryItem)
                    CurrentTab.Content.CurrentView = FileBrowserView.Tiles;
                else if (FolderViewsGallery.SelectedItem == ContentViewGalleryItem)
                    CurrentTab.Content.CurrentView = FileBrowserView.Content;
                else
                {
                    CurrentTab.Content.CurrentView = FileBrowserView.Icons;

                    if (FolderViewsGallery.SelectedItem == ExtraLargeIconsViewGalleryItem)
                        CurrentTab.Content.IconSize = 256;
                    else CurrentTab.Content.IconSize = FolderViewsGallery.SelectedItem == LargeIconsViewGalleryItem
                        ? 96
                        : FolderViewsGallery.SelectedItem == SmallIconsViewGalleryItem ? 16 : 48;
                }
            }
        }

        /*private void FileManagerBase_Loaded(Object sender, RoutedEventArgs e)
        {
            Initialize();
        }*/

        public async Task<Boolean> NavigateBackAsync()
        {
            /*if (ActiveContent.NavigationStack.CanGoBack)
            {
                ActiveContent.NavigationStack.Back();
                ActiveContent.RecentLocations.Back();
                ValidateNavButtonStates();
                await ActiveContent.RefreshAsync();
                return true;
            }
            else return false;*/
            return CurrentTab.Content.NavManager.GoBack();
        }

        public async Task<Boolean> NavigateForwardAsync()
        {
            /*if (ActiveContent.NavigationStack.CanGoForward)
            {
                ActiveContent.NavigationStack.Forward();
                ActiveContent.RecentLocations.Forward();
                ValidateNavButtonStates();
                await ActiveContent.RefreshAsync();
                return true;
            }
            else return false;*/
            return CurrentTab.Content.NavManager.GoFoward();
        }

        public async Task<Boolean> NavigateUpAsync()
        {
            var p = ((DirectoryQuery)CurrentTab.Content.CurrentLocation).Item.ItemPath;
            var path = Directory.GetParent(p).ToString();
            if (Directory.Exists(path))
            {
                var q = new DirectoryQuery(path);
                //await ActiveContent.NavigateAsync(q, true);
                CurrentTab.Content.NavManager.MoveTo(q);
                ValidateNavButtonStates();
                return true;
            }
            else
                return false;
        }


        public void Navigate(Location location)
        {
            Title = location.Name;
            //(ContentTabControl.SelectedItem as TabItem).GetBindingExpression(HeaderedContentControl.HeaderProperty).UpdateTarget(); //activec

            if ((location is ShellLocation loc) && (loc.LocationGuid == ShellLocation.ThisPcGuid))
            {
                ComputerRibbonTabItem.Visibility = Visibility.Visible;
                HomeRibbonTabItem.Visibility = Visibility.Collapsed;
                ShareRibbonTabItem.Visibility = Visibility.Collapsed;
            }
            else
            {
                HomeRibbonTabItem.Visibility = Visibility.Visible;
                ShareRibbonTabItem.Visibility = Visibility.Visible;
                ComputerRibbonTabItem.Visibility = Visibility.Collapsed;
            }

            UpdateStatusBar();
            ValidateNavButtonStates();
        }

        public void UpdateStatusBar()
        {
            ItemCounter.Content = CurrentTab.Content.CurrentDirectoryListView.Items.Count.ToString() + " items";

            _resettingAddress = true;
            AddressBox.BreadcrumbItems = CurrentTab.Content.CurrentLocation.BreadcrumbsSegments;
            if (CurrentTab.Content.CurrentLocation is DirectoryQuery q)
                SearchTextBox.WatermarkText = "Search " + Path.GetFileName(q.Name);
            _resettingAddress = false;

            NavHistoryButton.GetBindingExpression(ItemsControl.ItemsSourceProperty).UpdateTarget();

        }

        Boolean _resettingAddress = false;

        /*public void InitialNavigate(String path)
        {
            var q = new DirectoryQuery(path);
            CurrentTab.Content.NavManager.MoveTo(q);
            Navigate(q);
        }*/

        private void DetailsViewButton_Click(Object sender, RoutedEventArgs e)
        {
            CurrentTab.Content.CurrentView = FileBrowserView.Details;
            IconsViewButton.IsChecked = false;
            DetailsViewButton.IsChecked = true;
        }

        private void IconsViewButton_Click(Object sender, RoutedEventArgs e)
        {
            CurrentTab.Content.CurrentView = FileBrowserView.Icons;
            DetailsViewButton.IsChecked = false;
            IconsViewButton.IsChecked = true;
        }

        /*public void ValidateNavButtonStates(Boolean canGoBack, Boolean canGoForward, Boolean canGoUp)
        {
            NavBackButton.IsEnabled = canGoBack;
            NavForwardButton.IsEnabled = canGoForward;
            NavHistoryButton.IsEnabled = canGoBack || canGoForward;
            NavUpButton.IsEnabled = canGoUp;
        }*/

        private async void NavBackButton_Click(Object sender, RoutedEventArgs e)
        {
            await NavigateBackAsync();
            ValidateNavButtonStates();
        }

        private async void NavForwardButton_Click(Object sender, RoutedEventArgs e)
        {
            await NavigateForwardAsync();
            ValidateNavButtonStates();
        }

        private void NavHistoryButton_Click(Object sender, RoutedEventArgs e)
        {
            /*foreach (var s in ActiveContent.NavManager)
                Debug.WriteLine(ActiveContent.NavManager.IndexOf(s).ToString() + ": " + s);*/
        }

        private async void NavUpButton_Click(Object sender, RoutedEventArgs e)
        {
            await NavigateUpAsync();
            ValidateNavButtonStates();
        }

        private void NewTabButton_Click(Object sender, RoutedEventArgs e)
        {
            AddTab(WindowManager.WindowDefaultLocation);
        }

        private void TabsSetAsideToggleButton_Click(Object sender, RoutedEventArgs e)
        {
            ShowHideTabsOverview(!(TabsOverviewContentControl.IsManipulationEnabled));
        }

        public void ShowHideTabsOverview(Boolean show)
        {
            if (Config.Instance.TabsMode != Config.TabDisplayMode.Disabled)
            {
                if (show)
                {
                    (TabsOverviewContentControl.DataContext as TabOverviewManager).Populate();

                    TabsSetAsideToggleButton.IsChecked = true;
                    TabsOverviewContentControl.IsManipulationEnabled = true;
                }
                else
                {
                    TabsSetAsideToggleButton.IsChecked = false;
                    TabsOverviewContentControl.IsManipulationEnabled = false;

                    if (ContentTabControl.Items.Count == 0)
                        Close();
                }
            }
            else
            {
                TabsSetAsideToggleButton.IsChecked = false;
                TabsOverviewContentControl.IsManipulationEnabled = false;
            }
        }

        private void SetTabsAsideButton_Click(Object sender, RoutedEventArgs e)
        {
            TabOverviewManager.SetTabsAside(this);
        }

        private async void AddressBox_PathUpdated(Object sender, EventArgs e)
        {
            if (!_resettingAddress)
                CurrentTab.Content.NavManager.MoveTo(new DirectoryQuery(AddressBox.BreadcrumbItems.Last().Path));
                //await ActiveContent.NavigateAsync(new DirectoryQuery(AddressBox.BreadcrumbItems.Last().Path), true);
        }

        /*private void ContentTabControl_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            if ((CurrentTab != null) && CurrentTab.Content.IsLoaded)
            {
                Navigate(CurrentTab.Content.CurrentLocation);
                NavigationPaneMenuItem.IsChecked = CurrentTab.Content.ShowNavigationPane;
                DetailsPaneToggleButton.IsChecked = CurrentTab.Content.ShowDetailsPane;
                PreviewPaneToggleButton.IsChecked = CurrentTab.Content.ShowPreviewPane;
                if (CurrentTab.Content.CurrentLocation is DirectoryQuery query)
                    ValidateCommandStates(query.Item.SubItems.Count, query.Item);
            }
        }*/

        private void NavigationPaneMenuItem_Click(Object sender, RoutedEventArgs e)
        {
            CurrentTab.Content.ShowNavigationPane = NavigationPaneMenuItem.IsChecked == true;
        }

        private void PreviewPaneToggleButton_Click(Object sender, RoutedEventArgs e)
        {
            CurrentTab.Content.ShowPreviewPane = PreviewPaneToggleButton.IsChecked == true;
        }

        private void DetailsPaneToggleButton_Click(Object sender, RoutedEventArgs e)
        {
            CurrentTab.Content.ShowDetailsPane = DetailsPaneToggleButton.IsChecked == true;
        }

        private void ShowItemCheckBoxesCheckBox_Click(Object sender, RoutedEventArgs e)
        {
            CurrentTab.Content.ShowItemCheckboxes = ShowItemCheckBoxesCheckBox.IsChecked == true;
        }

        Boolean _altActionTaken = true;

        private async void MainWindow_PreviewKeyDown(Object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) //((Keyboard.GetKeyStates(Key.LeftCtrl) == KeyStates.Down) || (Keyboard.GetKeyStates(Key.RightCtrl) == KeyStates.Down))
            {
                if (ContentTabControl != null)
                {
                    if (e.Key == Key.W)
                        CloseCurrentLocation();
                    else if (e.Key == Key.T)
                        AddTab(WindowManager.WindowDefaultLocation);
                    else if (e.Key == Key.PageUp)
                    {
                        if (ContentTabControl.SelectedIndex > 0)
                            ContentTabControl.SelectedIndex--;
                        else
                            ContentTabControl.SelectedIndex = ContentTabControl.Items.Count - 1;
                    }
                    else if (e.Key == Key.PageDown)
                    {
                        if (ContentTabControl.SelectedIndex < (ContentTabControl.Items.Count - 1))
                            ContentTabControl.SelectedIndex++;
                        else
                            ContentTabControl.SelectedIndex = 0;
                    }
                    else if (e.Key == Key.R)
                        CurrentTab.Content.RenameSelection();
                    else if (e.Key == Key.D1 || e.Key == Key.NumPad1)
                    {
                        if (ContentTabControl.Items.Count > 0)
                            ContentTabControl.SelectedIndex = 0;
                    }
                    else if (e.Key == Key.D2 || e.Key == Key.NumPad2)
                    {
                        if (ContentTabControl.Items.Count > 1)
                            ContentTabControl.SelectedIndex = 1;
                    }
                    else if (e.Key == Key.D3 || e.Key == Key.NumPad3)
                    {
                        if (ContentTabControl.Items.Count > 2)
                            ContentTabControl.SelectedIndex = 2;
                    }
                    else if (e.Key == Key.D4 || e.Key == Key.NumPad4)
                    {
                        if (ContentTabControl.Items.Count > 3)
                            ContentTabControl.SelectedIndex = 3;
                    }
                    else if (e.Key == Key.D5 || e.Key == Key.NumPad5)
                    {
                        if (ContentTabControl.Items.Count > 4)
                            ContentTabControl.SelectedIndex = 4;
                    }
                    else if (e.Key == Key.D6 || e.Key == Key.NumPad6)
                    {
                        if (ContentTabControl.Items.Count > 5)
                            ContentTabControl.SelectedIndex = 5;
                    }
                    else if (e.Key == Key.D7 || e.Key == Key.NumPad7)
                    {
                        if (ContentTabControl.Items.Count > 6)
                            ContentTabControl.SelectedIndex = 6;
                    }
                    else if (e.Key == Key.D8 || e.Key == Key.NumPad8)
                    {
                        if (ContentTabControl.Items.Count > 7)
                            ContentTabControl.SelectedIndex = 7;
                    }
                    else if (e.Key == Key.D9 || e.Key == Key.NumPad9)
                    {
                        if (ContentTabControl.Items.Count > 0)
                            ContentTabControl.SelectedIndex = ContentTabControl.Items.Count - 1;
                    }
                }
                else if (e.Key == Key.W)
                    Close();
            }
            
            if (e.KeyboardDevice.IsKeyDown(Key.LeftAlt) || e.KeyboardDevice.IsKeyDown(Key.RightAlt)) //((Keyboard.GetKeyStates(Key.LeftAlt) == KeyStates.Down) || (Keyboard.GetKeyStates(Key.RightAlt) == KeyStates.Down))
            {
                if (e.Key == Key.Left)
                {
                    await NavigateBackAsync();
                    _altActionTaken = true;
                }
                else if (e.Key == Key.Right)
                {
                    await NavigateForwardAsync();
                    _altActionTaken = true;
                }
                else if (e.Key == Key.Up)
                {
                    await NavigateUpAsync();
                    _altActionTaken = true;
                }
                else
                {
                    _altActionTaken = false;
                }
            }

            if (e.Key == Key.F2)
                CurrentTab.Content.RenameSelection();
        }

        public void SelectTabAt(int index)
        {
            ContentTabControl.SelectedIndex = index;
            UpdateViewToCurrentTab();
        }

        public void UpdateViewToCurrentTab()
        {
            _syncView = true;

            if (CurrentTab.Content.CurrentView == FileBrowserView.List)
                FolderViewsGallery.SelectedItem = ListViewGalleryItem;
            else if (CurrentTab.Content.CurrentView == FileBrowserView.Details)
                FolderViewsGallery.SelectedItem = DetailsViewGalleryItem;
            else if (CurrentTab.Content.CurrentView == FileBrowserView.Tiles)
                FolderViewsGallery.SelectedItem = TilesViewGalleryItem;
            else if (CurrentTab.Content.CurrentView == FileBrowserView.Content)
                FolderViewsGallery.SelectedItem = ContentViewGalleryItem;
            else
            {
                if (CurrentTab.Content.IconSize >= 256)
                    FolderViewsGallery.SelectedItem = ExtraLargeIconsViewGalleryItem;
                else if (CurrentTab.Content.IconSize >= 96)
                    FolderViewsGallery.SelectedItem = LargeIconsViewGalleryItem;
                else if (CurrentTab.Content.IconSize >= 48)
                    FolderViewsGallery.SelectedItem = MediumIconsViewGalleryItem;
                else
                    FolderViewsGallery.SelectedItem = SmallIconsViewGalleryItem;
            }

            _syncView = false;
        }

        private void MainWindow_PreviewKeyUp(Object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.System) && (!_altActionTaken))
                ToggleMenuBar();
        }

        private async void SearchTextBox_ActionSubmitted(Object sender, ActionSubmittedEventArgs e)
        {
            var cts = new CancellationTokenSource();
            var sb = (ActionBox)sender;

            void Cancellation(Object sender, RoutedEventArgs e)
            {   
                cts.Cancel();
                sb.CancelAction();
            }

            sb.ActionCanceled += Cancellation;

            var path = CurrentTab.Content.CurrentLocation switch
            {
                SearchQuery s => s.Path,
                DirectoryQuery d => d.Item.ItemPath,
                ShellLocation s => s.LocationPath,
            };

            Guid id = Guid.Empty;
            if (CurrentTab.Content.CurrentLocation is ShellLocation loc)
                id = loc.LocationGuid;

            try
            {
                if (sb.Text == "")
                {
                    if (Directory.Exists(path))
                        CurrentTab.Content.NavManager.MoveTo(new DirectoryQuery(path)); //await ActiveContent.NavigateAsync(new DirectoryQuery(path), true, cts.Token);
                    else
                        CurrentTab.Content.NavManager.MoveTo(new ShellLocation(id)); //await ActiveContent.NavigateAsync(new ShellLocation(id), true, cts.Token);
                }
                else
                {
                    CurrentTab.Content.NavManager.MoveTo(new SearchQuery(path, sb.Text)); //await ActiveContent.NavigateAsync(new SearchQuery(path, sb.Text), true, cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // do nothing, user canceled search
            }

            sb.ActionCanceled -= Cancellation;
            sb.CancelAction();
        }

        private void CurrentlyOpenTabsListView_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            if /*(TabsOverviewContentControl.IsVisible && */(CurrentlyOpenTabsListView.SelectedItem != null)//)
            {
                (CurrentlyOpenTabsListView.SelectedItem as OverviewLocationTab).SwitchTo();
                CurrentlyOpenTabsListView.SelectedItem = null;
                ShowHideTabsOverview(false);
            }
        }

        private void DecoratableWindow_Deactivated(Object sender, EventArgs e)
        {
            if (TabsOverviewContentControl.IsVisible)
                ShowHideTabsOverview(false);
        }

        private void TabPreviewsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpenControlPanelButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("control.exe");
        }
    }
}
