using System;
using System.Collections.Generic;
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
        public WindowContent ActiveContent
        {
            get => (ContentTabControl.SelectedItem as TabItem)?.Content as WindowContent;
        }

        public IEnumerable<WindowContent> WindowContents => ContentTabControl.Items.OfType<WindowContent>();

        public Config.InterfaceModeType InterfaceMode
        {
            get => (Config.InterfaceModeType)GetValue(InterfaceModeProperty);
            set => SetValue(InterfaceModeProperty, value);
        }

        public static readonly DependencyProperty InterfaceModeProperty =
            DependencyProperty.Register("InterfaceMode", typeof(Config.InterfaceModeType), typeof(MainWindow), new FrameworkPropertyMetadata(Config.InterfaceModeType.Ribbon, FrameworkPropertyMetadataOptions.AffectsRender, OnInterfaceModeChangedCallback));

        static void OnInterfaceModeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MainWindow).UpdateInterface();
        }

        void UpdateInterface()
        {
            if (InterfaceMode == Config.InterfaceModeType.CommandBar)
            {
                CommandBarControl.Visibility = Visibility.Visible;
                MenuBar.IsEnabled = true;
                MenuBarToolBar.Visibility = Visibility.Visible;
                DefaultToolBar.Visibility = Visibility.Collapsed;
                Ribbon.Visibility = Visibility.Collapsed;
                RibbonTitle.Visibility = Visibility.Collapsed;
                NavigationBarGrid.Visibility = Visibility.Visible;
            }
            else if (InterfaceMode == Config.InterfaceModeType.Ribbon)
            {
                CommandBarControl.Visibility = Visibility.Collapsed;
                MenuBar.IsEnabled = false;
                MenuBarToolBar.Visibility = Visibility.Collapsed;
                DefaultToolBar.Visibility = Visibility.Collapsed;
                Ribbon.Visibility = Visibility.Visible;
                RibbonTitle.Visibility = Visibility.Visible;
                NavigationBarGrid.Visibility = Visibility.Visible;
            }
            else if (InterfaceMode == Config.InterfaceModeType.None)
            {
                CommandBarControl.Visibility = Visibility.Collapsed;
                MenuBar.IsEnabled = true;
                MenuBarToolBar.Visibility = Visibility.Visible;
                DefaultToolBar.Visibility = Visibility.Visible;
                Ribbon.Visibility = Visibility.Collapsed;
                RibbonTitle.Visibility = Visibility.Collapsed;
                NavigationBarGrid.Visibility = Visibility.Collapsed;
            }
        }

        void Initialize()
        {
            InitializeComponent();

            var interfaceModeBinding = new Binding()
            {
                Source = Config.Instance,
                Path = new PropertyPath("InterfaceMode"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(this, InterfaceModeProperty, interfaceModeBinding);
            UpdateInterface();

            WindowManager.OpenWindows.Add(this);
        }

        String _firstNavigationPath = WindowManager.WindowDefaultPath;

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
            Initialize();
            AddressBox.Converter = Converter;

            Activated += MainWindow_Activated;
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

        MainWindow _copyWindow = null;
        public MainWindow(MainWindow copyWindow) : this()
        {
            _copyWindow = copyWindow;
        }

        public MainWindow(String path) : this()
        {
            _firstNavigationPath = path;
        }

        private void MainWindow_Loaded(Object sender, RoutedEventArgs e)
        {
            Config.ClipboardContents.CollectionChanged += ClipboardContents_CollectionChanged;
            if (_copyWindow != null)
            {
                foreach (var w in _copyWindow.WindowContents)
                {
                    if (w.CurrentLocation is DirectoryQuery query)
                        AddTab(query.LocationPath);
                    else if (w.CurrentLocation is ShellLocation location)
                        AddTab(location.LocationGuid.ToString());
                    else if (w.CurrentLocation is SearchQuery search)
                    {
                        //TODO figure out what to do here lol
                    }
                }
                    //AddTab((w.CurrentLocation as DirectoryQuery).Item.ItemPath);
            }
            else
                AddTab(_firstNavigationPath);
            ContentTabControl.SelectedIndex = 0;
            ClipboardContents_CollectionChanged(null, null);
            ValidateCommandStates(0, null);
        }

        public TabItem AddTab()
        {
            return AddTab(WindowManager.WindowDefaultPath);
        }

        public TabItem AddTab(String path)
        {
            var item = CreateTab(path);
            ContentTabControl.Items.Add(item);
            ContentTabControl.SelectedItem = item;
            return item;
        }

        private TabItem CreateTab(String path)
        {
            if (Config.Instance.EnableTabs)
            {
                var item = new TabItem()
                {
                    Content = new WindowContent(path)
                };

                item.MouseDown += (sneder, args) =>
                {
                    if (args.ChangedButton == MouseButton.Middle && args.ButtonState == MouseButtonState.Pressed)
                    {
                        RemoveTab(item);
                    }
                };

                var tabHeaderBinding = new Binding()
                {
                    Source = (item.Content as WindowContent).NavigationStack,
                    Path = new PropertyPath("Current"),
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    FallbackValue = Path.GetFileName(path)
                };

                BindingOperations.SetBinding(item, HeaderedContentControl.HeaderProperty, tabHeaderBinding);

                return item;
            }
            else
                return null;
        }

        private void CloseCurrentLocation()
        {
            if (Config.Instance.EnableTabs)
                RemoveTab(ContentTabControl.SelectedItem as TabItem);
            else
                Close();
        }

        private void RemoveTab(TabItem item)
        {
            ContentTabControl.Items.Remove(item);

            if (ContentTabControl.Items.Count <= 0)
                Close();
        }

        public void ValidateNavButtonStates()
        {
            Boolean canGoUp;
            try
            {
                canGoUp = Directory.GetParent(((DirectoryQuery)ActiveContent.NavigationStack.Current).Item.ItemPath) != null;
            }
            catch
            {
                canGoUp = false;
            }
            ValidateNavButtonStates(ActiveContent.NavigationStack.CanGoBack, ActiveContent.NavigationStack.CanGoForward, canGoUp);
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
            if (sender == NewWindowMenuItem)
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
            }
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
            await ActiveContent.OpenSelectionAsync();
        }

        private void CopyButton_Click(Object sender, RoutedEventArgs e)
        {
            ActiveContent.CopySelection();
        }

        private void CopyPathButton_Click(Object sender, RoutedEventArgs e)
        {
            ActiveContent.CopyPathToSelection();
        }

        private async void PasteButton_Click(Object sender, RoutedEventArgs e)
        {
            await ActiveContent.PasteCurrentAsync();
        }

        private async void PasteShortcutButton_Click(Object sender, RoutedEventArgs e)
        {
            await ActiveContent.PasteShortcutAsync();
        }

        private void CutButton_Click(Object sender, RoutedEventArgs e)
        {
            ActiveContent.CutSelection();
        }

        private void EditButton_Click(Object sender, RoutedEventArgs e)
        {
            
        }

        private async void DeleteButton_Click(Object sender, RoutedEventArgs e)
        {
            await ActiveContent.DeleteSelectionAsync();
        }

        private void PropertiesButton_Click(Object sender, RoutedEventArgs e)
        {
            ActiveContent.ShowPropertiesForSelection();
        }

        private void RenameButton_Click(Object sender, RoutedEventArgs e)
        {
            ActiveContent.RenameSelection();
        }

        private void HistoryButton_Click(Object sender, RoutedEventArgs e)
        {

        }

        private async void NewFolderButton_Click(Object sender, RoutedEventArgs e)
        {
            await ActiveContent.CreateNewFolderAsync();
        }

        private void SelectAllButton_Click(Object sender, RoutedEventArgs e)
        {
            ActiveContent.SelectAll();
        }

        private void SelectNoneButton_Click(Object sender, RoutedEventArgs e)
        {
            ActiveContent.SelectNone();
        }

        private void InvertSelectionButton_Click(Object sender, RoutedEventArgs e)
        {
            ActiveContent.InvertSelection();
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
                    ActiveContent.CurrentDirectoryListView.Focus();
                }
                else
                {
                    MenuBar.Visibility = Visibility.Visible;
                    MenuBar.Focus();
                }
            }
        }

        private void FolderViewsGallery_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            if (FolderViewsGallery.SelectedItem == ListViewGalleryItem)
                ActiveContent.CurrentView = FileBrowserView.List;
            else if (FolderViewsGallery.SelectedItem == DetailsViewGalleryItem)
                ActiveContent.CurrentView = FileBrowserView.Details;
            else if (FolderViewsGallery.SelectedItem == TilesViewGalleryItem)
                ActiveContent.CurrentView = FileBrowserView.Tiles;
            else if (FolderViewsGallery.SelectedItem == ContentViewGalleryItem)
                ActiveContent.CurrentView = FileBrowserView.Content;
            else
            {
                ActiveContent.CurrentView = FileBrowserView.Icons;

                if (FolderViewsGallery.SelectedItem == ExtraLargeIconsViewGalleryItem)
                    ActiveContent.IconSize = 256;
                else ActiveContent.IconSize = FolderViewsGallery.SelectedItem == LargeIconsViewGalleryItem
                    ? 96
                    : FolderViewsGallery.SelectedItem == SmallIconsViewGalleryItem ? 16 : 48;
            }
        }

        private void FileManagerBase_Loaded(Object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        public async Task<Boolean> NavigateBackAsync()
        {
            if (ActiveContent.NavigationStack.CanGoBack)
            {
                ActiveContent.NavigationStack.Back();
                ActiveContent.RecentLocations.Back();
                ValidateNavButtonStates();
                await ActiveContent.RefreshAsync();
                return true;
            }
            else return false;
        }

        public async Task<Boolean> NavigateForwardAsync()
        {
            if (ActiveContent.NavigationStack.CanGoForward)
            {
                ActiveContent.NavigationStack.Forward();
                ActiveContent.RecentLocations.Forward();
                ValidateNavButtonStates();
                await ActiveContent.RefreshAsync();
                return true;
            }
            else return false;
        }

        public async Task<Boolean> NavigateUpAsync()
        {
            var p = ((DirectoryQuery) ActiveContent.CurrentLocation).Item.ItemPath;
            var path = Directory.GetParent(p).ToString();
            if (Directory.Exists(path))
            {
                var q = new DirectoryQuery(path);
                await ActiveContent.NavigateAsync(q);
                ActiveContent.RecentLocations.Navigate(q);
                ValidateNavButtonStates();
                return true;
            }
            else
                return false;
        }


        public void Navigate(Location location)
        {
            Title = location.Name;
            (ContentTabControl.SelectedItem as TabItem).GetBindingExpression(HeaderedContentControl.HeaderProperty).UpdateTarget(); //activec

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
            ItemCounter.Content = ActiveContent.CurrentDirectoryListView.Items.Count.ToString() + " items";

            _resettingAddress = true;
            AddressBox.BreadcrumbItems = ActiveContent.CurrentLocation.BreadcrumbsSegments;
            if (ActiveContent.CurrentLocation is DirectoryQuery q)
                SearchTextBox.WatermarkText = "Search " + Path.GetFileName(q.Name);
            _resettingAddress = false;

            NavHistoryButton.GetBindingExpression(ItemsControl.ItemsSourceProperty).UpdateTarget();

        }

        Boolean _resettingAddress = false;

        public void InitialNavigate(String path)
        {
            var q = new DirectoryQuery(path);
            ActiveContent.NavigationStack.Add(q);
            Navigate(q);
        }

        private void DetailsViewButton_Click(Object sender, RoutedEventArgs e)
        {
            ActiveContent.CurrentView = FileBrowserView.Details;
            IconsViewButton.IsChecked = false;
            DetailsViewButton.IsChecked = true;
        }

        private void IconsViewButton_Click(Object sender, RoutedEventArgs e)
        {
            ActiveContent.CurrentView = FileBrowserView.Icons;
            DetailsViewButton.IsChecked = false;
            IconsViewButton.IsChecked = true;
        }

        public void ValidateNavButtonStates(Boolean canGoBack, Boolean canGoForward, Boolean canGoUp)
        {
            NavBackButton.IsEnabled = canGoBack;
            NavForwardButton.IsEnabled = canGoForward;
            NavHistoryButton.IsEnabled = canGoBack || canGoForward;
            NavUpButton.IsEnabled = canGoUp;
        }

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
            foreach (var s in ActiveContent.NavigationStack)
                Debug.WriteLine(ActiveContent.NavigationStack.IndexOf(s).ToString() + ": " + s);
        }

        private async void NavUpButton_Click(Object sender, RoutedEventArgs e)
        {
            await NavigateUpAsync();
            ValidateNavButtonStates();
        }

        private void NewTabButton_Click(Object sender, RoutedEventArgs e)
        {
            AddTab(WindowManager.WindowDefaultPath);
        }

        private void TabsSetAsideToggleButton_Click(Object sender, RoutedEventArgs e)
        {
            ShowHideTabsOverview(!(TabsOverviewContentControl.IsManipulationEnabled));
        }

        public void ShowHideTabsOverview(Boolean show)
        {
            if (show)
            {
                (TabsOverviewContentControl.DataContext as TabManager).Populate();

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

        private void SetTabsAsideButton_Click(Object sender, RoutedEventArgs e)
        {
            TabManager.SetTabsAside(this);
        }

        private async void AddressBox_PathUpdated(Object sender, EventArgs e)
        {   
            if (!_resettingAddress)
                await ActiveContent.NavigateAsync(new DirectoryQuery(AddressBox.BreadcrumbItems.Last().Path));
        }

        private void ContentTabControl_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            if (ContentTabControl.SelectedItem != null)
            {
                Navigate(ActiveContent.CurrentLocation);
                NavigationPaneMenuItem.IsChecked = ActiveContent.ShowNavigationPane;
                DetailsPaneToggleButton.IsChecked = ActiveContent.ShowDetailsPane;
                PreviewPaneToggleButton.IsChecked = ActiveContent.ShowPreviewPane;
                if (ActiveContent.CurrentLocation is DirectoryQuery query)
                    ValidateCommandStates(query.Item.SubItems.Count, query.Item);
            }
        }

        private void NavigationPaneMenuItem_Click(Object sender, RoutedEventArgs e)
        {
            ActiveContent.ShowNavigationPane = NavigationPaneMenuItem.IsChecked == true;
        }

        private void PreviewPaneToggleButton_Click(Object sender, RoutedEventArgs e)
        {
            ActiveContent.ShowPreviewPane = PreviewPaneToggleButton.IsChecked == true;
        }

        private void DetailsPaneToggleButton_Click(Object sender, RoutedEventArgs e)
        {
            ActiveContent.ShowDetailsPane = DetailsPaneToggleButton.IsChecked == true;
        }

        private void ShowItemCheckBoxesCheckBox_Click(Object sender, RoutedEventArgs e)
        {
            ActiveContent.ShowItemCheckboxes = ShowItemCheckBoxesCheckBox.IsChecked == true;
        }

        Boolean _altActionTaken = true;

        private async void MainWindow_PreviewKeyDown(Object sender, KeyEventArgs e)
        {
            if ((Keyboard.GetKeyStates(Key.LeftCtrl) != KeyStates.None) || (Keyboard.GetKeyStates(Key.RightCtrl) != KeyStates.None))
            {
                if (e.Key == Key.T)
                    AddTab();
                else if (e.Key == Key.W)
                    CloseCurrentLocation();
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
                    ActiveContent.RenameSelection();
            }
            
            if ((Keyboard.GetKeyStates(Key.LeftAlt) == KeyStates.Down) || (Keyboard.GetKeyStates(Key.RightAlt) == KeyStates.Down))
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
                ActiveContent.RenameSelection();
        }

        private void MainWindow_PreviewKeyUp(Object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.System) && (!_altActionTaken))
                ToggleMenuBar();
        }

        private async void SearchTextBox_ActionSubmitted(Object sender, ActionSubmittedEventArgs e)
        {
            var cts = new CancellationTokenSource();
            var sb = (SearchBox)sender;

            void Cancellation(Object sender, RoutedEventArgs e)
            {   
                cts.Cancel();
                sb.CancelSearch();
            }

            sb.SearchCanceled += Cancellation;

            var path = ActiveContent.CurrentLocation switch
            {
                SearchQuery s => s.Path,
                DirectoryQuery d => d.Item.ItemPath,
                ShellLocation s => s.LocationPath,
            };

            Guid id = Guid.Empty;
            if (ActiveContent.CurrentLocation is ShellLocation loc)
                id = loc.LocationGuid;

            try
            {
                if (sb.Text == "")
                {
                    if (Directory.Exists(path))
                        await ActiveContent.NavigateAsync(new DirectoryQuery(path), cts.Token);
                    else
                        await ActiveContent.NavigateAsync(new ShellLocation(id), cts.Token);
                }
                else
                {
                    await ActiveContent.NavigateAsync(new SearchQuery(path, sb.Text), cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // do nothing, user canceled search
            }

            sb.SearchCanceled -= Cancellation;
            sb.CancelSearch();
        }

        private void CurrentlyOpenTabsListView_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            if /*(TabsOverviewContentControl.IsVisible && */(CurrentlyOpenTabsListView.SelectedItem != null)//)
            {
                (CurrentlyOpenTabsListView.SelectedItem as LocationTab).SwitchTo();
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
    }
}
