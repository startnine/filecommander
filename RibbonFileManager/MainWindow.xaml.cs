using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.VisualBasic.FileIO;
//using NT6FileManagerBase;
using Start9.UI.Wpf.Windows;
//using WindowsSharp.DiskItems;
using System.Media;

namespace RibbonFileManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DecoratableWindow
    {
        public WindowContent ActiveContent
        {
            get => ((this.ContentTabControl.SelectedItem as TabItem).Content) as WindowContent;
        }

        public List<WindowContent> WindowContents
        {
            get
            {
                List<WindowContent> list = new List<WindowContent>();
                foreach (TabItem t in ContentTabControl.Items)
                    list.Add((t.Content) as WindowContent);

                return list;
            }
        }

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

        public MainWindow()
        {
            Initialize();
        }

        MainWindow _copyWindow = null;
        public MainWindow(MainWindow copyWindow)
        {
            Initialize();
            _copyWindow = copyWindow;
        }

        public MainWindow(string path)
        {
            Initialize();
            _firstNavigationPath = path;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Config.ClipboardContents.CollectionChanged += ClipboardContents_CollectionChanged;
            if (_copyWindow != null)
            {
                foreach (WindowContent w in _copyWindow.WindowContents)
                    AddTab(w.CurrentPath);
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
            ValidateNavButtonStates((ActiveContent.HistoryIndex == 0), (ActiveContent.HistoryIndex < (ActiveContent.HistoryList.Count - 1)) /*(ActiveContent.HistoryIndex <= (ActiveContent.HistoryList.Count - 1))*/, (ActiveContent.HistoryList.Count > 1), (Directory.Exists(Path.GetDirectoryName(ActiveContent.CurrentPath))));
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

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.OpenSelection();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.CopySelection();
        }

        private void CopyPathButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.CopyPathToSelection();
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.PasteCurrent();
        }

        private void PasteShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.PasteShortcut();
        }

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.CutSelection();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.DeleteSelection();
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

        private void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.CreateNewFolder();
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
                ActiveContent.CurrentView = WindowContent.FileBrowserView.List;
            else if (FolderViewsGallery.SelectedItem == DetailsViewGalleryItem)
                ActiveContent.CurrentView = WindowContent.FileBrowserView.Details;
            else if (FolderViewsGallery.SelectedItem == TilesViewGalleryItem)
                ActiveContent.CurrentView = WindowContent.FileBrowserView.Tiles;
            else if (FolderViewsGallery.SelectedItem == ContentViewGalleryItem)
                ActiveContent.CurrentView = WindowContent.FileBrowserView.Content;
            else
            {
                ActiveContent.CurrentView = WindowContent.FileBrowserView.Icons;

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

        public bool NavigateBack()
        {
            if (ActiveContent.HistoryIndex > 0)
            {
                ActiveContent.HistoryIndex--;
                ValidateNavButtonStates();
                return true;
            }
            else return false;
        }

        public bool NavigateForward()
        {
            if (ActiveContent.HistoryIndex < (ActiveContent.HistoryList.Count - 1))
            {
                ActiveContent.HistoryIndex++;
                ValidateNavButtonStates();
                return true;
            }
            else return false;
        }

        public bool NavigateUp()
        {
            string path = Directory.GetParent(ActiveContent.CurrentPath).ToString();
            if (Directory.Exists(path))
            {
                ActiveContent.Navigate(path);
                ValidateNavButtonStates();
                return true;
            }
            else
                return false;
        }


        public void Navigate(string targetPath)
        {
            Title = Path.GetFileName(targetPath);
            //Icon = BitmapSource.Create(new DiskItem(targetPath).ItemLargeIcon);
            UpdateStatusBar();
            UpdateNavigationBar();
            ValidateNavButtonStates();
        }

        public void UpdateStatusBar()
        {
            ItemCounter.Content = ActiveContent.CurrentDirectoryListView.Items.Count.ToString() + " items";
        }

        bool _resettingAddress = false;

        public void UpdateNavigationBar()
        {
            _resettingAddress = true;
            AddressBox.BreadcrumbPath = ActiveContent.CurrentPath;
            SearchTextBox.WatermarkText = "Search " + Path.GetFileName(ActiveContent.CurrentPath);
            _resettingAddress = false;
        }

        public void InitialNavigate(string path)
        {
            ActiveContent.HistoryList.Add(path);
            Navigate(path);
        }

        private void CurrentDirectoryListView_Item_MouseDoubleClick(Object sender, MouseButtonEventArgs e)
        {
            if (ActiveContent.CurrentDirectoryListView.SelectedItems.Count == 1)
            {
                string path = ((List<DiskItem>)ActiveContent.CurrentDirectoryListView.ItemsSource)[ActiveContent.CurrentDirectoryListView.SelectedIndex].ItemPath;
                if (Directory.Exists(path))
                    Navigate(path);
                else
                    ((List<DiskItem>)ActiveContent.CurrentDirectoryListView.ItemsSource)[ActiveContent.CurrentDirectoryListView.SelectedIndex].Open();
            }
            else if (ActiveContent.CurrentDirectoryListView.SelectedItems.Count > 1)
            {
                //var source = (List<DiskItem>)(CurrentDirectoryListView.ItemsSource);
                foreach (DiskItem i in ActiveContent.CurrentDirectoryListView.SelectedItems)
                {
                    string path = i.ItemPath;

                    if (Directory.Exists(path))
                    {
                        ////////WindowManager.CreateWindow(path);
                    }
                    else
                        i.Open();
                }
            }
        }

        private void DetailsViewButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.CurrentView = WindowContent.FileBrowserView.Details;
            IconsViewButton.IsChecked = false;
            DetailsViewButton.IsChecked = true;
        }

        private void IconsViewButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveContent.CurrentView = WindowContent.FileBrowserView.Icons;
            DetailsViewButton.IsChecked = false;
            IconsViewButton.IsChecked = true;
        }

        public void ValidateNavButtonStates(bool historyIndexZero, bool IndexLessThan, bool listCount, bool dirExists)
        {
            if (historyIndexZero)
                NavBackButton.IsEnabled = false;
            else
                NavBackButton.IsEnabled = true;

            if (IndexLessThan)
                NavForwardButton.IsEnabled = true;
            else
                NavForwardButton.IsEnabled = false;

            if (listCount)
                NavHistoryButton.IsEnabled = true;
            else
                NavHistoryButton.IsEnabled = false;

            if (dirExists)
                NavUpButton.IsEnabled = true;
            else
                NavUpButton.IsEnabled = false;
        }

        private void NavBackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateBack();
            ValidateNavButtonStates();
        }

        private void NavForwardButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateForward();
            ValidateNavButtonStates();
        }

        private void NavHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("HISTORY: ");
            foreach (string s in ActiveContent.HistoryList)
                Debug.WriteLine(ActiveContent.HistoryList.IndexOf(s).ToString() + ": " + s);
        }

        private void NavUpButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateUp();
            ValidateNavButtonStates();
        }

        private void NewTabButton_Click(object sender, RoutedEventArgs e)
        {
            AddTab(WindowManager.WindowDefaultPath);
        }

        private void AddressBox_PathUpdated(object sender, EventArgs e)
        {
            if (!_resettingAddress)
                ActiveContent.Navigate(AddressBox.BreadcrumbPath);
        }

        private void ContentTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContentTabControl.SelectedItem != null)
            {
                Navigate(ActiveContent.CurrentPath);
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

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
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
                    NavigateBack();
                    _altActionTaken = true;
                }
                else if (e.Key == Key.Right)
                {
                    NavigateForward();
                    _altActionTaken = true;
                }
                else if (e.Key == Key.Up)
                {
                    NavigateUp();
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
    }
}
