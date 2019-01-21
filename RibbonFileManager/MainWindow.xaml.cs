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
using WindowsSharp.DiskItems;

namespace RibbonFileManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DecoratableWindow
    {
        public Config.InterfaceModeType InterfaceMode
        {
            get => (Config.InterfaceModeType)GetValue(InterfaceModeProperty);
            set => SetValue(InterfaceModeProperty, value);
        }

        public static readonly DependencyProperty InterfaceModeProperty =
            DependencyProperty.Register("InterfaceMode", typeof(Config.InterfaceModeType), typeof(MainWindow), new FrameworkPropertyMetadata(Config.InterfaceModeType.Ribbon, FrameworkPropertyMetadataOptions.AffectsRender, OnInterfaceModeChangedCallback));

        static void OnInterfaceModeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine("interface mode updated");
            (d as MainWindow).UpdateInterface();
        }

        void UpdateInterface()
        {
            if (InterfaceMode == Config.InterfaceModeType.CommandBar)
            {
                CommandBarControl.Visibility = Visibility.Visible;
                MenuBar.IsEnabled = true;
                Ribbon.Visibility = Visibility.Collapsed;
            }
            else if (InterfaceMode == Config.InterfaceModeType.Ribbon)
            {
                CommandBarControl.Visibility = Visibility.Collapsed;
                MenuBar.IsEnabled = false;
                MenuBar.Visibility = Visibility.Collapsed;
                Ribbon.Visibility = Visibility.Visible;
            }
        }

        /*static MainWindow()
        {
            //Debug.WriteLine("My Computer Path: " + Environment.GetFolderPath(Environment.SpecialFolder.MyComputer));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainWindow), new FrameworkPropertyMetadata(typeof(DecoratableWindow)));
        }*/

        public static RoutedCommand MenuBarCommand = new RoutedCommand();
        public static RoutedCommand RenameCommand = new RoutedCommand();
        public static RoutedCommand BackCommand = new RoutedCommand();
        public static RoutedCommand ForwardCommand = new RoutedCommand();
        public static RoutedCommand UpLevelCommand = new RoutedCommand();

        void Initialize()
        {
            InitializeComponent();

            MenuBarCommand.InputGestures.Add(new KeyGesture(Key.Z, ModifierKeys.Alt));
            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));
            BackCommand.InputGestures.Add(new KeyGesture(Key.Left, ModifierKeys.Alt));
            ForwardCommand.InputGestures.Add(new KeyGesture(Key.Right, ModifierKeys.Alt));
            UpLevelCommand.InputGestures.Add(new KeyGesture(Key.Up, ModifierKeys.Alt));

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
        }

        private void RenameCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RenameSelection();
        }

        public MainWindow()
        {
            Initialize();
            Navigate(WindowManager.WindowDefaultPath, true);
            InitialNavigate(WindowManager.WindowDefaultPath);
        }

        public MainWindow(string path)
        {
            Initialize();
            Navigate(path, true);
            InitialNavigate(path);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //var collection = Favorites;// (FavoritesTreeViewItem.ItemsSource as ObservableCollection<DiskItem>);
            //Favorites.Add(new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\Documents")));
            //Favorites.Add(new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\Pictures")));
            //Favorites.Add(new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\Downloads")));
            //UpdateInterface();

            /*Binding statusBarBinding = new Binding()
            {
                Source = Config.Instance,
                Path = new PropertyPath("InterfaceMode"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(FileManagerControl.StatusBar, VisibilityProperty, statusBarBinding);*/

            ResetFavorites();

            Config.Favorites.CollectionChanged += (sneder, args) =>
            {
                ResetFavorites();
            };

            Config.ClipboardContents.CollectionChanged += ClipboardContents_CollectionChanged;

            CurrentDirectorySelectionChanged += FileManagerBase_CurrentDirectorySelectionChanged;
            Navigated += (sneder, args) =>
            {
                //FileManagerBase_CurrentDirectorySelectionChanged(null, null);
                Navigate(CurrentPath);
                ValidateNavButtonStates();
            };
            FileManagerBase_CurrentDirectorySelectionChanged(null, null);
            ClipboardContents_CollectionChanged(null, null);
            Icon = ((ImageBrush)((new Start9.UI.Wpf.Converters.IconToImageBrushConverter()).Convert(new DiskItem(CurrentPath).ItemLargeIcon, null, "32", null))).ImageSource;

            /*List<Fluent.GalleryItem> items = new List<Fluent.GalleryItem>();
            foreach (var i in FolderViewsGallery.Items)
                items.Add(i);

            FolderViewsGallery.Items.Clear();
            foreach (var t in items)
                FolderViewsGallery.Items.Add(t);*/
        }

        private void ResetFavorites()
        {
            Config.Favorites.Clear();
            foreach (DiskItem d in Config.Favorites)
            {
                Config.Favorites.Add(d);
            }
        }

        /*public void Navigate(int index)
        {
            if ((index > 0) && (index < (HistoryList.Count - 1)))
            {
                string path = Environment.ExpandEnvironmentVariables(HistoryList[index]);
                if (HistoryList.Contains(path))
                {
                    CurrentDirectoryListView.ItemsSource = new DiskItem(currentPath).SubItems;
                    AddressBox.Text = currentPath;
                    Title = Path.GetFileName(currentPath);
                }
            }
        }*/

        /*public void Navigate(string path)
        {
            Navigate(path, false);
        }*/

        public void Navigate(string path, bool updateFileManager)
        {
            var item = new DiskItem(path);
            Title = Path.GetFileName(path);
            Icon = ((ImageBrush)((new Start9.UI.Wpf.Converters.IconToImageBrushConverter()).Convert(item.ItemLargeIcon, null, "32", null))).ImageSource;

            if (updateFileManager)
                Navigate(path);

            ValidateNavButtonStates();
            //FileManagerControl.CurrentDirectoryListView.ItemsSource = item.SubItems;
            //AddressBox.Text = currentPath;
            //BreadcrumbsBar.Path = @"Computer\" + path;
            Title = Path.GetFileName(CurrentPath);
        }

        public void ValidateNavButtonStates()
        {
            ValidateNavButtonStates((HistoryIndex == 0), (HistoryIndex >= (HistoryList.Count - 1)), (HistoryList.Count > 1), (Directory.Exists(Path.GetDirectoryName(CurrentPath))));
        }

        bool _newWindowSubmenuItemClick = false;

        private void NewWindowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == NewWindowMenuItem)
            {
                if (_newWindowSubmenuItemClick)
                    _newWindowSubmenuItemClick = false;
                else
                    WindowManager.CloneWindow(this);
            }
            else if (sender == NewWindowSubmenuItem)
            {
                _newWindowSubmenuItemClick = true;
                WindowManager.CloneWindow(this);
            }
            else
                WindowManager.CloneWindow(this);
        }

        private void NewWindowDefaultLocationButton_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.CreateWindow();
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

        private void FileManagerBase_CurrentDirectorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentDirectoryListView.SelectedItems.Count == 0)
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
                /*if (CurrentDirectoryListView.SelectedItems.Count == 1)
                {
                    SetDetailsPane(((ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource)[CurrentDirectoryListView.SelectedIndex].ItemPath);
                }*/

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


                if ((CurrentDirectoryListView.SelectedItems.Count == 1) && ((CurrentDirectoryListView.SelectedItem as DiskItem).ItemCategory == DiskItem.DiskItemCategory.Directory))
                {
                    CommandBarControl.CommandBarLayers[2].IsVisible = false;
                    CommandBarControl.CommandBarLayers[3].IsVisible = true;
                }
                else
                {
                    CommandBarControl.CommandBarLayers[2].IsVisible = true;
                    CommandBarControl.CommandBarLayers[3].IsVisible = false;
                }
            }

            ////////SelectedItemCounter.Content = CurrentDirectoryListView.SelectedItems.Count.ToString() + " items selected";
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

        private void CurrentViewGalleryItem_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void FolderViewsGallery_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FolderViewsGallery.SelectedItem == ListViewGalleryItem)
                CurrentView = FileBrowserView.List;
            else if (FolderViewsGallery.SelectedItem == DetailsViewGalleryItem)
                CurrentView = FileBrowserView.Details;
            else if (FolderViewsGallery.SelectedItem == TilesViewGalleryItem)
                CurrentView = FileBrowserView.Tiles;
            else if (FolderViewsGallery.SelectedItem == ContentViewGalleryItem)
                CurrentView = FileBrowserView.Content;
            else
                CurrentView = FileBrowserView.Icons;

            Debug.WriteLine("CurrentView: " + CurrentView.ToString());
        }

        private void CurrentDirectoryListViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            /*var item = sender as ListViewItem;

            if (((List<DiskItem>)CurrentDirectoryListView.ItemsSource)[(item.Parent as ListView).Items.IndexOf(item)].ItemCategory == DiskItem.DiskItemCategory.Directory)
            {
                foreach (MenuItem item item.ContextMenu.Items
            }*/
        }

        private void RunAsAdminMenuItem_Loaded(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).Tag as ListViewItem;

            if (((List<DiskItem>)CurrentDirectoryListView.ItemsSource)[(item.Parent as ListView).Items.IndexOf(item)].ItemCategory == DiskItem.DiskItemCategory.Directory)
                (sender as MenuItem).Visibility = Visibility.Collapsed;
        }

        private void RunAsAdminMenuItem_Initialized(object sender, EventArgs e)
        {
            
        }

        /*private void TopLevelTreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            CurrentDirectoryListView.ItemsSource = (sender as TreeView).ItemsSource;
        }*/

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSelection();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            CopySelection();
        }

        private void CopyPathButton_Click(object sender, RoutedEventArgs e)
        {
            CopyPathToSelection();
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            PasteCurrent();
        }

        private void PasteShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            PasteShortcut();
        }

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            CutSelection();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelection();
        }

        private void PropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPropertiesForSelection();
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            RenameSelection();
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            CreateNewFolder();
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            SelectAll();
        }

        private void SelectNoneButton_Click(object sender, RoutedEventArgs e)
        {
            SelectNone();
        }

        private void InvertSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            InvertSelection();
        }

        private void BackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NavigateBack();
            ValidateNavButtonStates();
        }

        private void ForwardCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NavigateForward();
            ValidateNavButtonStates();
        }

        private void UpLevelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NavigateUp();
            ValidateNavButtonStates();
        }

        private void FolderAndSearchOptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Config.SettingsWindow.Show();
            Config.SettingsWindow.Focus();
            Config.SettingsWindow.Activate();
        }

        private void MenuBarCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MenuBar.IsEnabled)
            {
                if (MenuBar.Visibility == Visibility.Visible)
                {
                    MenuBar.Visibility = Visibility.Collapsed;
                    CurrentDirectoryListView.Focus();
                }
                else
                {
                    MenuBar.Visibility = Visibility.Visible;
                    MenuBar.Focus();
                    //FocusManager.SetFocusedElement(MenuBar);
                }
            }
        }

        public string CurrentFolderTitle
        {
            get
            {
                try
                {
                    return Path.GetFileName(CurrentPath);
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public List<string> HistoryList { get; set; } = new List<string>();
        /*{
            get => (List<String>)GetValue(HistoryListProperty);
            set => SetValue(HistoryListProperty, value);
        }*/

        //public static readonly DependencyProperty HistoryListProperty = DependencyProperty.Register("HistoryList", typeof(List<String>), typeof(FileManagerBase), new PropertyMetadata(new List<String>()));

        public Int32 HistoryIndex
        {
            get => (Int32)GetValue(HistoryIndexProperty);
            set => SetValue(HistoryIndexProperty, value);
        }

        public static readonly DependencyProperty HistoryIndexProperty = DependencyProperty.Register("HistoryIndex", typeof(Int32), typeof(MainWindow), new PropertyMetadata(0, OnHistoryIndexPropertyChangedCallback));

        public bool IsRenamingFiles
        {
            get => (bool)GetValue(IsRenamingFilesProperty);
            set => SetValue(IsRenamingFilesProperty, value);
        }

        public static readonly DependencyProperty IsRenamingFilesProperty = DependencyProperty.Register("IsRenamingFiles", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        static void OnHistoryIndexPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainWindow sender = d as MainWindow;
            var oldValue = (Int32)e.OldValue;
            var newValue = (Int32)e.NewValue;

            ////////sender.ValidateNavButtonStates();

            sender.Navigate(sender.HistoryList[newValue]);
        }

        public string CurrentPath
        {
            get => HistoryList[HistoryIndex];
        }

        /*public ObservableCollection<DiskItem> Favorites
        {
            get => (ObservableCollection<DiskItem>)GetValue(FavoritesProperty);
            set => SetValue(FavoritesProperty, value);
        }

        public static readonly DependencyProperty FavoritesProperty = DependencyProperty.Register("Favorites", typeof(ObservableCollection<DiskItem>), typeof(FileManagerBase), new PropertyMetadata(new ObservableCollection<DiskItem>()));*/

        /*public ObservableCollection<DiskItem> ComputerSubfolders
        {
            get => (ObservableCollection<DiskItem>)GetValue(ComputerSubfoldersProperty);
            set => SetValue(ComputerSubfoldersProperty, value);
        }

        public static readonly DependencyProperty ComputerSubfoldersProperty = DependencyProperty.Register("ComputerSubfolders", typeof(ObservableCollection<DiskItem>), typeof(FileManagerBase), new PropertyMetadata(new ObservableCollection<DiskItem>()));*/

        public Double IconSize
        {
            get => (Double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register("IconSize", typeof(Double), typeof(MainWindow), new PropertyMetadata((Double)48.0));

        public bool ShowDetailsPane
        {
            get => (bool)GetValue(ShowDetailsPaneProperty);
            set => SetValue(ShowDetailsPaneProperty, value);
        }

        public static readonly DependencyProperty ShowDetailsPaneProperty = DependencyProperty.Register("ShowDetailsPane", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public bool ShowPreviewPane
        {
            get => (bool)GetValue(ShowPreviewPaneProperty);
            set => SetValue(ShowPreviewPaneProperty, value);
        }

        public static readonly DependencyProperty ShowPreviewPaneProperty = DependencyProperty.Register("ShowPreviewPane", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public bool ShowNavigationPane
        {
            get => (bool)GetValue(ShowNavigationPaneProperty);
            set => SetValue(ShowNavigationPaneProperty, value);
        }

        public static readonly DependencyProperty ShowNavigationPaneProperty = DependencyProperty.Register("ShowNavigationPane", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

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

        public bool ShowItemCheckboxes
        {
            get => (bool)GetValue(ShowItemCheckboxesProperty);
            set => SetValue(ShowItemCheckboxesProperty, value);
        }

        public static readonly DependencyProperty ShowItemCheckboxesProperty = DependencyProperty.Register("ShowItemCheckboxes", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        public event EventHandler<SelectionChangedEventArgs> CurrentDirectorySelectionChanged;

        public event EventHandler<EventArgs> Navigated;


        private void FileManagerBase_Loaded(object sender, RoutedEventArgs e)
        {
            /*foreach (ResourceDictionary d in Window.GetWindow(this).Resources.MergedDictionaries)
                Resources.MergedDictionaries.Add(d);*/

            Initialize();
        }

        public bool NavigateBack()
        {
            if (HistoryIndex > 0)
            {
                HistoryIndex--;
                return true;
            }
            else return false;
        }

        public bool NavigateForward()
        {
            if (HistoryIndex < (HistoryList.Count - 1))
            {
                HistoryIndex++;
                return true;
            }
            else return false;
        }

        public bool NavigateUp()
        {
            bool returnValue = false;
            try
            {
                string path = Path.GetDirectoryName(CurrentPath);
                if (Directory.Exists(path))
                {
                    Navigate(path);
                    returnValue = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return returnValue;
        }

        public void CopySelection()
        {
            Config.Cut = false;
            SetClipboard();
        }

        public void CutSelection()
        {
            Config.Cut = true;
            SetClipboard();
        }

        public void CopyPathToSelection()
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
            Clipboard.SetText(paths);
        }

        public void PasteCurrent()
        {
            var items = Config.CopyTo(HistoryList[HistoryIndex]);
            var source = ((List<DiskItem>)CurrentDirectoryListView.ItemsSource);
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

            Refresh();
        }

        public void SetClipboard()
        {
            Config.ClipboardContents.Clear();
            foreach (DiskItem d in CurrentDirectoryListView.SelectedItems)
            {
                Config.ClipboardContents.Add(d);
            }
        }

        public void Refresh()
        {
            Navigate(CurrentPath);
        }

        public void PasteShortcut()
        {
            /*foreach (DiskItem d in CurrentDirectoryListView.SelectedItems)
            {
                Shortcut.CreateShortcut(d.ItemName + " - Shortcut", null, d.ItemPath, HistoryList[HistoryIndex]);
            }*/
            Refresh();
        }

        public void DeleteSelection()
        {
            //var source = ((List<DiskItem>)CurrentDirectoryListView.ItemsSource);
            for (var i = 0; i < CurrentDirectoryListView.SelectedItems.Count; i++)
            {
                var d = (CurrentDirectoryListView.SelectedItems[i] as DiskItem);

                /*if (source.Contains(d))
                {*/
                if (d.ItemCategory == DiskItem.DiskItemCategory.Directory)
                    FileSystem.DeleteDirectory(d.ItemPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                else if ((d.ItemCategory == DiskItem.DiskItemCategory.File) || (d.ItemCategory == DiskItem.DiskItemCategory.Shortcut))
                    FileSystem.DeleteFile(d.ItemPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                //}
            }

            Refresh();
        }

        public void RenameSelection()
        {
            IsRenamingFiles = true;
        }

        public void CreateNewFolder()
        {
            string path = CurrentPath + @"\New Folder";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            else
            {
                int cycle = 1;
                while (Directory.Exists(path))
                {
                    path = CurrentPath + @"\New Folder (" + cycle.ToString() + ")";
                    MessageBox.Show(path);
                    cycle++;
                }
                Directory.CreateDirectory(path);
            }
            Refresh();
        }

        public void ShowPropertiesForSelection()
        {
            foreach (DiskItem i in CurrentDirectoryListView.SelectedItems)
            {
                /*string path = i.ItemPath;

                /*if (Directory.Exists(path))
                    Manager.CreateWindow(path);
                else*
                try
                {
                    var info = new ProcessStartInfo(path)
                    {
                        Verb = "properties",
                        UseShellExecute = true
                    };
                    Process.Start(info);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }*/
                //Debug.WriteLine("properties result: " + i.ShowProperties());
            }
        }

        public void OpenSelection()
        {
            OpenSelection(DiskItem.OpenVerbs.Normal);
        }

        public void OpenSelection(DiskItem.OpenVerbs verb)
        {
            //var source = ((List<DiskItem>)CurrentDirectoryListView.ItemsSource);
            foreach (DiskItem i in CurrentDirectoryListView.SelectedItems)
            {
                string path = i.ItemPath;

                if (Directory.Exists(path))
                {
                    if (CurrentDirectoryListView.SelectedItems.Count == 1)
                    {
                        Navigate(path);
                        break;
                    }
                    else
                    {
                        ////////Manager.CreateWindow(path);
                    }
                }
                else
                    try
                    {
                        //Process.Start(path);
                        i.Open(verb);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
            }
        }

        public void EditSelection()
        {

        }

        public void SelectAll()
        {
            CurrentDirectoryListView.SelectAll();
        }

        public void SelectNone()
        {
            CurrentDirectoryListView.SelectedItem = null;
        }

        public void InvertSelection()
        {

        }

        private void NavigationPaneTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var val = e.NewValue as DiskItem;

            if (val != null)
                Navigate(Environment.ExpandEnvironmentVariables(val.ItemPath));
        }


        public void Navigate(string targetPath)
        {
            /*if ((HistoryList.Count == 0) || (HistoryIndex >= (HistoryList.Count - 1)))
            {
                HistoryList.Add(path);
                HistoryIndex++;
            }
            else
            {
                HistoryList.Insert(HistoryIndex, path);

                while (HistoryList.Count > (HistoryIndex + 1))
                    HistoryList.RemoveAt(HistoryIndex + 1);
            }

            CurrentDirectoryListView.ItemsSource = new DiskItem(currentPath).SubItems;*/
            string path = Environment.ExpandEnvironmentVariables(targetPath);
            var item = new DiskItem(path);
            if (Directory.Exists(path))
            {
                ItemCounter.Content = item.SubItems.Count.ToString() + " items";

                if (!(HistoryList.Contains(path)))
                {
                    if (HistoryIndex < (HistoryList.Count - 1))
                    {
                        for (int i = HistoryList.Count - 1; i > HistoryIndex; i--)
                        {
                            HistoryList.RemoveAt(i);
                        }
                    }

                    HistoryList.Add(path);
                }

                HistoryIndex = HistoryList.IndexOf(path);

                SetPanes(item);

                CurrentDirectoryListView.ItemsSource = item.SubItems;
            }
            CurrentDirectoryListView_SelectionChanged(null, null);
            //Navigated?.Invoke(this, null);
            ValidateNavButtonStates();
        }

        public void InitialNavigate(string path)
        {
            HistoryList.Add(path);
            Navigate(path);
        }

        private void CurrentDirectoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentDirectoryListView.SelectedItems.Count == 0)
            {
                //SetDetailsPane(HistoryList[HistoryIndex]);
                ////////CopyButton.IsEnabled = false;
                ////////CopyPathButton.IsEnabled = false;
                ////////CutButton.IsEnabled = false;
                ////////MoveToButton.IsEnabled = false;
                ////////CopyToButton.IsEnabled = false;
                ////////DeleteButton.IsEnabled = false;
                ////////RenameButton.IsEnabled = false;
                ////////OpenButton.IsEnabled = false;
                ////////EditButton.IsEnabled = false;
                SelectedItemCounter.Visibility = Visibility.Hidden;
                SetPanes(new DiskItem(CurrentPath));
            }
            else
            {
                /*if (CurrentDirectoryListView.SelectedItems.Count == 1)
                {
                    SetDetailsPane(((ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource)[CurrentDirectoryListView.SelectedIndex].ItemPath);
                }*/

                ////////CopyButton.IsEnabled = true;
                ////////CopyPathButton.IsEnabled = true;
                ////////CutButton.IsEnabled = true;
                ////////MoveToButton.IsEnabled = true;
                ////////CopyToButton.IsEnabled = true;
                ////////DeleteButton.IsEnabled = true;
                ////////RenameButton.IsEnabled = true;
                ////////OpenButton.IsEnabled = true;
                ////////EditButton.IsEnabled = true;
                SelectedItemCounter.Visibility = Visibility.Visible;
                SetPanes((DiskItem)(CurrentDirectoryListView.SelectedItem));
            }

            SelectedItemCounter.Content = CurrentDirectoryListView.SelectedItems.Count.ToString() + " items selected";

            CurrentDirectorySelectionChanged?.Invoke(sender, e);
        }

        public void SetPanes(DiskItem item)
        {
            /*DetailsFileIconBorder.Background*/
            //Debug.WriteLine("ActualHeight: " + DetailsFileIconRectangle.ActualHeight.ToString());
            double size = DetailsFileIconRectangle.ActualHeight;
            if (size <= 0)
                size = 48;
            DetailsFileIconRectangle.Fill = (ImageBrush)((new Start9.UI.Wpf.Converters.IconToImageBrushConverter()).Convert(item.ItemJumboIcon, null, size.ToString(), null));
            if ((item.ItemCategory == DiskItem.DiskItemCategory.Directory) && (item.ItemRealName == Path.GetFileName(CurrentPath)))
                DetailsFileNameTextBlock.Text = item.SubItems.Count.ToString() + " items";
            else
                DetailsFileNameTextBlock.Text = item.ItemDisplayName;



            if (item.ItemPath.ToLowerInvariant() == CurrentPath.ToLowerInvariant())
            {
                SetPreviewPaneLayer(0);
            }
            else
            {
                string ext = Path.GetExtension(item.ItemPath).ToLowerInvariant();
                if (ext == "bmp" || ext == "png" || ext == "jpg" || ext == "jpeg")
                {
                    (PreviewPaneGrid.Children[2] as System.Windows.Shapes.Rectangle).Fill = new ImageBrush(new BitmapImage(new Uri(item.ItemPath, UriKind.RelativeOrAbsolute)));
                    SetPreviewPaneLayer(2);
                }
                else
                {
                    bool isMediaFile = true;
                    PreviewPlayer.Source = new Uri(item.ItemPath, UriKind.RelativeOrAbsolute);
                    PreviewPlayer.MediaFailed += (sneder, args) =>
                    {
                        isMediaFile = false;
                    };

                    if (isMediaFile)
                        SetPreviewPaneLayer(3);
                    else
                        SetPreviewPaneLayer(1);
                }
                /*else if (ext == "wav" || ext == "wma" || ext == "mp3" || ext == "m4a")
                {

                }
                else if (ext == "mp4" || ext == "wmv" || ext == "mp3" || ext == "m4a")
                {

                }*/
            }
        }

        void SetPreviewPaneLayer(int index)
        {
            for (int i = 0; i < PreviewPaneGrid.Children.Count; i++)
            {
                var control = PreviewPaneGrid.Children[i];
                if (i == index)
                    control.Visibility = Visibility.Visible;
                else
                    control.Visibility = Visibility.Collapsed;
            }
        }

        private void CurrentDirectoryListView_Item_MouseDoubleClick(Object sender, MouseButtonEventArgs e)
        {
            if (CurrentDirectoryListView.SelectedItems.Count == 1)
            {
                string path = ((List<DiskItem>)CurrentDirectoryListView.ItemsSource)[CurrentDirectoryListView.SelectedIndex].ItemPath;
                if (Directory.Exists(path))
                    Navigate(path);
                else
                    try
                    {
                        //Process.Start(path);
                        ((List<DiskItem>)CurrentDirectoryListView.ItemsSource)[CurrentDirectoryListView.SelectedIndex].Open();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
            }
            else if (CurrentDirectoryListView.SelectedItems.Count > 1)
            {
                //var source = (List<DiskItem>)(CurrentDirectoryListView.ItemsSource);
                foreach (DiskItem i in CurrentDirectoryListView.SelectedItems)
                {
                    string path = i.ItemPath;

                    if (Directory.Exists(path))
                    {
                        ////////WindowManager.CreateWindow(path);
                    }
                    else
                        try
                        {
                            //Process.Start(path);
                            i.Open();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                }
            }
        }

        private void DetailsViewButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentView = FileBrowserView.Details;
            IconsViewButton.IsChecked = false;
            DetailsViewButton.IsChecked = true;
        }

        private void IconsViewButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentView = FileBrowserView.Icons;
            DetailsViewButton.IsChecked = false;
            IconsViewButton.IsChecked = true;
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            /*var item = (sender as MenuItem).Tag as ListViewItem;

            string path = ((List<DiskItem>)CurrentDirectoryListView.ItemsSource)[(item.Parent as ListView).Items.IndexOf(item)].ItemPath;*/
            foreach (DiskItem i in CurrentDirectoryListView.SelectedItems)
            {
                string path = i.ItemPath;

                if (Directory.Exists(path))
                {
                    Navigate(path);
                    break;
                }
                else
                    try
                    {
                        //Process.Start(path);
                        i.Open();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
            }
        }

        private void RunAsAdminMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //var item = (sender as MenuItem).Tag as ListViewItem;
            foreach (DiskItem i in CurrentDirectoryListView.SelectedItems)
            {
                string path = i.ItemPath; //((List<DiskItem>)CurrentDirectoryListView.ItemsSource)[(item.Parent as ListView).Items.IndexOf(item)].ItemPath;
                if (File.Exists(path))
                    try
                    {
                        /*Process.Start(new ProcessStartInfo(path)
                        {
                            Verb = "runas"
                        });*/
                        i.Open(DiskItem.OpenVerbs.Admin);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
            }
        }

        private void TouchableContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            bool canRunAsAdmin = true;
            foreach (DiskItem i in CurrentDirectoryListView.SelectedItems)
            {
                if (i.ItemCategory == DiskItem.DiskItemCategory.Directory)
                {
                    canRunAsAdmin = false;
                    break;
                }
            }

            foreach (MenuItem m in (sender as ContextMenu).Items)
            {
                if (m.Name == "RunAsAdminMenuItem")
                {
                    if (canRunAsAdmin)
                        m.Visibility = Visibility.Visible;
                    else
                        m.Visibility = Visibility.Collapsed;

                    break;
                }
            }
        }

        void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CopySelection();
        }

        void CutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CutSelection();
        }

        void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelection();
        }

        void PropertiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowPropertiesForSelection();
        }

        private void CurrentDirectoryListView_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.C)
                    CopySelection();
                else if (e.Key == Key.X)
                    CutSelection();
                else if (e.Key == Key.V)
                    PasteCurrent();
            }
            else if (e.Key == Key.Delete)
                DeleteSelection();
        }

        public void ValidateNavButtonStates(bool historyIndexZero, bool IndexLessThan, bool listCount, bool dirExists)
        {
            if (historyIndexZero)
                NavBackButton.IsEnabled = false;
            else
                NavBackButton.IsEnabled = true;

            if (IndexLessThan)
                NavForwardButton.IsEnabled = false;
            else
                NavForwardButton.IsEnabled = true;

            try
            {
                if (listCount)
                    NavHistoryButton.IsEnabled = true;
                else
                    NavHistoryButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                NavHistoryButton.IsEnabled = false;
            }

            try
            {
                if (dirExists)
                    NavUpButton.IsEnabled = true;
                else
                    NavUpButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                NavUpButton.IsEnabled = false;
            }
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
            Debug.WriteLine("HISTORY: ");
            foreach (string s in HistoryList)
                Debug.WriteLine(HistoryList.IndexOf(s).ToString() + ": " + s);
        }

        private void NavUpButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateUp();
            ValidateNavButtonStates();
        }

        private void AddressBox_KeyDown(object sender, KeyEventArgs e)
        {
            var bar = (sender as TextBox);
            if (e.Key == Key.Enter)
            {
                string expText = Environment.ExpandEnvironmentVariables(bar.Text);
                if (Directory.Exists(expText))
                {
                    Navigate(expText, true);
                    CurrentDirectoryListView.Focus();
                    //NavBar_PathTextBox_LostFocus(null, null);
                }
                else
                {
                    var failText = expText;
                    Start9.UI.Wpf.Windows.MessageBox<Start9.UI.Wpf.Windows.MessageBoxEnums.OkButton>.Show("Ribbon File Browser can't find '" + failText + "'. Check the speeling and try again.", "Ribbon File Browser"); //(this, "Ribbon File Browser can't find '" + failText + "'. Check the speeling and try again.", "Ribbon File Browser");
                }
            }
            else if (e.Key == Key.Escape)
            {
                CurrentDirectoryListView.Focus();
            }
        }
    }
}
