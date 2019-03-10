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
            get => ((ContentTabControl.SelectedItem as TabItem)?.Content) as WindowContent;
        }

        public List<WindowContent> WindowContents => ContentTabControl.Items.OfType<WindowContent>().ToList();

        public Config.InterfaceModeType InterfaceMode
        {
            get => (Config.InterfaceModeType)GetValue(InterfaceModeProperty);
            set => SetValue(InterfaceModeProperty, value);
        }

        public static readonly DependencyProperty InterfaceModeProperty =
            DependencyProperty.Register("InterfaceMode", typeof(Config.InterfaceModeType), typeof(MainWindow), new FrameworkPropertyMetadata(Config.InterfaceModeType.Ribbon, FrameworkPropertyMetadataOptions.AffectsRender, OnInterfaceModeChangedCallback));

        static void OnInterfaceModeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //Debug.WriteLine("interface mode updated");
            (d as MainWindow).UpdateInterface();
        }

        void UpdateInterface()
        {
            if (InterfaceMode == Config.InterfaceModeType.CommandBar)
            {
                CommandBarControl.Visibility = Visibility.Visible;
                MenuBar.IsEnabled = true;
                Ribbon.Visibility = Visibility.Collapsed;
                RibbonTitle.Visibility = Visibility.Collapsed;
            }
            else if (InterfaceMode == Config.InterfaceModeType.Ribbon)
            {
                CommandBarControl.Visibility = Visibility.Collapsed;
                MenuBar.IsEnabled = false;
                MenuBar.Visibility = Visibility.Collapsed;
                Ribbon.Visibility = Visibility.Visible;
                RibbonTitle.Visibility = Visibility.Visible;
            }
        }

        /*static MainWindow()
        {
            //Debug.WriteLine("My Computer Path: " + Environment.GetFolderPath(Environment.SpecialFolder.MyComputer));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainWindow), new FrameworkPropertyMetadata(typeof(DecoratableWindow)));
        }*/

        /*public static RoutedCommand CloseWindowCommand = new RoutedCommand();
        public static RoutedCommand MenuBarCommand = new RoutedCommand();

        public static RoutedCommand BackCommand = new RoutedCommand();
        public static RoutedCommand ForwardCommand = new RoutedCommand();
        public static RoutedCommand UpLevelCommand = new RoutedCommand();

        public static RoutedCommand RenameCommand = new RoutedCommand();*/

        void Initialize()
        {
            InitializeComponent();

            /*CloseWindowCommand.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));
            MenuBarCommand.InputGestures.Add(new KeyGesture(Key.Z, ModifierKeys.Alt));

            BackCommand.InputGestures.Add(new KeyGesture(Key.Left, ModifierKeys.Alt));
            ForwardCommand.InputGestures.Add(new KeyGesture(Key.Right, ModifierKeys.Alt));
            UpLevelCommand.InputGestures.Add(new KeyGesture(Key.Up, ModifierKeys.Alt));

            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));*/

            Binding interfaceModeBinding = new Binding()
            {
                Source = Config.Instance,
                Path = new PropertyPath("InterfaceMode"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(this, InterfaceModeProperty, interfaceModeBinding);
            UpdateInterface();

            /*Binding checkBoxBinding = new Binding()
            {
                Source = typeof(Config),
                Path = new PropertyPath("ShowItemCheckboxes"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(this, FileManagerBase.ShowItemCheckboxesProperty, checkBoxBinding);*/

            WindowManager.OpenWindows.Add(this);
            //FileManagerControl.Resources.MergedDictionaries.Add(Resources);

            //System.Media.SoundPlayer player = new System.Media.SoundPlayer();
        }

        string _firstNavigationPath = WindowManager.WindowDefaultPath;

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
        }

        MainWindow _copyWindow = null;
        public MainWindow(MainWindow copyWindow) : this()
        {
            _copyWindow = copyWindow;
        }

        public MainWindow(string path) : this()
        {
            _firstNavigationPath = path;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Config.ClipboardContents.CollectionChanged += ClipboardContents_CollectionChanged;
            if (_copyWindow != null)
            {
                foreach (var w in _copyWindow.WindowContents)
                    AddTab((w.CurrentLocation as DirectoryQuery).Item.ItemPath);
            }
            else
                AddTab(_firstNavigationPath);
            //AddTab(_firstNavigationPath);
            //AddTab(_firstNavigationPath);
            ContentTabControl.SelectedIndex = 0;
            ClipboardContents_CollectionChanged(null, null);
            ValidateCommandStates(0, null);
        }

        public void AddTab()
        {
            AddTab(WindowManager.WindowDefaultPath);
        }

        public void AddTab(string path)
        {
            ContentTabControl.Items.Add(CreateTab(path));
        }

        private TabItem CreateTab(string path)
        {
            if (Config.Instance.EnableTabs)
            {
                TabItem item = new TabItem()
                {
                    Content = new WindowContent(path)
                    //, Header = "AAAAA"
                };

                item.MouseDown += (sneder, args) =>
                {
                    if (args.ChangedButton == MouseButton.Middle && args.ButtonState == MouseButtonState.Pressed)
                    {
                        RemoveTab(item);
                    }
                };

                Binding tabHeaderBinding = new Binding()
                {
                    Source = item.Content,
                    Path = new PropertyPath("CurrentDirectoryName"),
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    FallbackValue = Path.GetFileName(path)
                };

                BindingOperations.SetBinding(item, TabItem.HeaderProperty, tabHeaderBinding);

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

        public void ValidateCommandStates(int selectedCount, DiskItem activeItem)
        {
            if (selectedCount == 0)
            {
                //SetDetailsPane(HistoryList[HistoryIndex]);
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
                ////////SelectedItemCounter.Visibility = Visibility.Visible;
                ////////SetPanes((DiskItem)(CurrentDirectoryListView.SelectedItem));


                if ((selectedCount == 1) && (activeItem.ItemCategory == DiskItem.DiskItemCategory.Directory))
                {
                    CommandBarControl.CommandBarLayers[2].IsVisible = false;
                    CommandBarControl.CommandBarLayers[3].IsVisible = true;
                }
                else
                {
                    CommandBarControl.CommandBarLayers[2].IsVisible = true;
                    CommandBarControl.CommandBarLayers[3].IsVisible = false;
                }


                if ((activeItem.ItemCategory != DiskItem.DiskItemCategory.Directory) && (activeItem.ItemCategory != DiskItem.DiskItemCategory.App))
                {
                    /*var assoc = activeItem.GetAssociatedProgram();
                    if (assoc != null)
                        Debug.WriteLine("ASSOCIATED PROGRAM: " + assoc.ItemPath);
                    else
                        Debug.WriteLine("COULD NOT FIND ASSOCIATED PROGRAM");*/

                    //MessageBox.Show("waiting");
                    //Debug.WriteLine("\n");

                    var openWith = activeItem.GetOpenWithPrograms();
                    foreach (DiskItem d in openWith)
                    {
                        /*if (d != null)
                            Debug.WriteLine("OPEN WITH: " + d.ItemPath);
                        else
                            Debug.WriteLine("OPEN WITH NULL");*/
                    }
                }
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

        bool _newWindowSubmenuItemClick = false;

        private void NewWindowButton_Click(object sender, RoutedEventArgs e)
        {
            //WindowManager.CloneWindow(this);
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

        private void CurrentViewGalleryItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /*private ProcessStartInfo GetAdminProcessStartOptions(string exeName)
        {
            return new ProcessStartInfo(exeName)
            {
                Verb = "runas"
            };
        }*/

        string cmdName = "cmd.exe";
        string psName = "powershell.exe";

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

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            await ActiveContent.OpenSelectionAsync();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.CopySelection();
        }

        private void CopyPathButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.CopyPathToSelection();
        }

        private async void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            await ActiveContent.PasteCurrentAsync();
        }

        private async void PasteShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            await ActiveContent.PasteShortcutAsync();
        }

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.CutSelection();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            await ActiveContent.DeleteSelectionAsync();
        }

        private void PropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.ShowPropertiesForSelection();
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.RenameSelection();
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            await ActiveContent.CreateNewFolderAsync();
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.SelectAll();
        }

        private void SelectNoneButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.SelectNone();
        }

        private void InvertSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.InvertSelection();
        }

        private void FolderAndSearchOptionsMenuItem_Click(object sender, RoutedEventArgs e)
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

        private void FolderViewsGallery_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                else if (FolderViewsGallery.SelectedItem == LargeIconsViewGalleryItem)
                    ActiveContent.IconSize = 96;
                else if (FolderViewsGallery.SelectedItem == SmallIconsViewGalleryItem)
                    ActiveContent.IconSize = 16;
                else
                    ActiveContent.IconSize = 48;
            }
        }

        private void FileManagerBase_Loaded(object sender, RoutedEventArgs e)
        {
            /*foreach (ResourceDictionary d in Window.GetWindow(this).Resources.MergedDictionaries)
                Resources.MergedDictionaries.Add(d);*/

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
            string path = Directory.GetParent(p).ToString();
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

        bool _resettingAddress = false;

        public void InitialNavigate(string path)
        {
            var q = new DirectoryQuery(path);
            ActiveContent.NavigationStack.Add(q);
            Navigate(q);
        }

        private void DetailsViewButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.CurrentView = FileBrowserView.Details;
            IconsViewButton.IsChecked = false;
            DetailsViewButton.IsChecked = true;
        }

        private void IconsViewButton_Click(object sender, RoutedEventArgs e)
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

        private async void NavBackButton_Click(object sender, RoutedEventArgs e)
        {
            await NavigateBackAsync();
            ValidateNavButtonStates();
        }

        private async void NavForwardButton_Click(object sender, RoutedEventArgs e)
        {
            await NavigateForwardAsync();
            ValidateNavButtonStates();
        }

        private void NavHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var s in ActiveContent.NavigationStack)
                Debug.WriteLine(ActiveContent.NavigationStack.IndexOf(s).ToString() + ": " + s);
        }

        private async void NavUpButton_Click(object sender, RoutedEventArgs e)
        {
            await NavigateUpAsync();
            ValidateNavButtonStates();
        }

        private void NewTabButton_Click(object sender, RoutedEventArgs e)
        {
            AddTab(WindowManager.WindowDefaultPath);
        }

        private async void AddressBox_PathUpdated(object sender, EventArgs e)
        {
            if (!_resettingAddress)
                await ActiveContent.NavigateAsync(new DirectoryQuery(AddressBox.BreadcrumbItems.Last().Path));
        }

        private void ContentTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContentTabControl.SelectedItem != null)
            {
                Navigate(ActiveContent.CurrentLocation);
                NavigationPaneMenuItem.IsChecked = ActiveContent.ShowNavigationPane;
                DetailsPaneToggleButton.IsChecked = ActiveContent.ShowDetailsPane;
                PreviewPaneToggleButton.IsChecked = ActiveContent.ShowPreviewPane;
            }
        }

        private void NavigationPaneMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.ShowNavigationPane = NavigationPaneMenuItem.IsChecked == true;
        }

        private void PreviewPaneToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.ShowPreviewPane = PreviewPaneToggleButton.IsChecked == true;
        }

        private void DetailsPaneToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.ShowDetailsPane = DetailsPaneToggleButton.IsChecked == true;
        }

        private void ShowItemCheckBoxesCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.ShowItemCheckboxes = ShowItemCheckBoxesCheckBox.IsChecked == true;
        }

        bool _altActionTaken = true;

        private async void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.GetKeyStates(Key.LeftCtrl) != KeyStates.None) || (Keyboard.GetKeyStates(Key.RightCtrl) != KeyStates.None))
            {
                if (e.Key == Key.T)
                    AddTab();
                else if (e.Key == Key.W)
                    CloseCurrentLocation();
                else if (e.Key == Key.R)
                    ActiveContent.RenameSelection();
            }
            
            if ((Keyboard.GetKeyStates(Key.LeftAlt) != KeyStates.None) || (Keyboard.GetKeyStates(Key.RightAlt) != KeyStates.None))
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
        }

        private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.System) && (!_altActionTaken))
                ToggleMenuBar();
        }

        private async void SearchTextBox_SearchSubmitted(Object sender, SearchSubmittedEventArgs e)
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
            };

            try
            {
                if (sb.Text == "")
                {
                    await ActiveContent.NavigateAsync(new DirectoryQuery(path), cts.Token);
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
    }
}
