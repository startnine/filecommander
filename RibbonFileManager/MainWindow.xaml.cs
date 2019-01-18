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
using NT6FileManagerBase;
using Start9.UI.Wpf.Windows;
using WindowsSharp.DiskItems;

namespace RibbonFileManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DecoratableWindow
    {
        /*static MainWindow()
        {
            //Debug.WriteLine("My Computer Path: " + Environment.GetFolderPath(Environment.SpecialFolder.MyComputer));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainWindow), new FrameworkPropertyMetadata(typeof(DecoratableWindow)));
        }*/

        public static RoutedCommand RenameCommand = new RoutedCommand();
        public static RoutedCommand BackCommand = new RoutedCommand();
        public static RoutedCommand ForwardCommand = new RoutedCommand();
        public static RoutedCommand UpLevelCommand = new RoutedCommand();

        void Initialize()
        {
            InitializeComponent();

            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));
            BackCommand.InputGestures.Add(new KeyGesture(Key.Left, ModifierKeys.Alt));
            ForwardCommand.InputGestures.Add(new KeyGesture(Key.Right, ModifierKeys.Alt));
            UpLevelCommand.InputGestures.Add(new KeyGesture(Key.Up, ModifierKeys.Alt));

            WindowManager.OpenWindows.Add(this);
            //FileManagerControl.Resources.MergedDictionaries.Add(Resources);
        }

        private void RenameCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FileManagerControl.RenameSelection();
        }

        public MainWindow()
        {
            Initialize();
            Navigate(WindowManager.WindowDefaultPath, true);
            FileManagerControl.InitialNavigate(WindowManager.WindowDefaultPath);
        }

        public MainWindow(string path)
        {
            Initialize();
            Navigate(path, true);
            FileManagerControl.InitialNavigate(path);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            //var collection = Favorites;// (FavoritesTreeViewItem.ItemsSource as ObservableCollection<DiskItem>);
            //Favorites.Add(new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\Documents")));
            //Favorites.Add(new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\Pictures")));
            //Favorites.Add(new DiskItem(Environment.ExpandEnvironmentVariables(@"%userprofile%\Downloads")));
            ResetFavorites();

            Manager.Favorites.CollectionChanged += (sneder, args) =>
            {
                ResetFavorites();
            };

            Manager.ClipboardContents.CollectionChanged += ClipboardContents_CollectionChanged;

            FileManagerControl.CurrentDirectorySelectionChanged += FileManagerBase_CurrentDirectorySelectionChanged;
            FileManagerControl.Navigated += (sneder, args) =>
            {
                //FileManagerBase_CurrentDirectorySelectionChanged(null, null);
                Navigate(FileManagerControl.CurrentPath);
                ValidateNavButtonStates();
            };
            FileManagerBase_CurrentDirectorySelectionChanged(null, null);
            ClipboardContents_CollectionChanged(null, null);
            Icon = ((ImageBrush)((new Start9.UI.Wpf.Converters.IconToImageBrushConverter()).Convert(new DiskItem(FileManagerControl.CurrentPath).ItemLargeIcon, null, "32", null))).ImageSource;

            NavBar.NavBackButtonClick += NavBackButton_Click;
            NavBar.NavForwardButtonClick += NavForwardButton_Click;
            NavBar.NavUpButtonClick += NavUpButton_Click;
            NavBar.NavAddressBarKeyDown += NavAddressBar_KeyDown;
        }

        private void NavAddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            var bar = (sender as TextBox);
            if (e.Key == Key.Enter)
            {
                string expText = Environment.ExpandEnvironmentVariables(bar.Text);
                if (Directory.Exists(expText))
                {
                    Navigate(expText, true);
                    FileManagerControl.CurrentDirectoryListView.Focus();
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
                FileManagerControl.CurrentDirectoryListView.Focus();
            }
        }

        private void ResetFavorites()
        {
            Manager.Favorites.Clear();
            foreach (DiskItem d in Manager.Favorites)
            {
                Manager.Favorites.Add(d);
            }
        }

        private void NavBackButton_Click(object sender, RoutedEventArgs e)
        {
            /*if (FileManagerControl.HistoryIndex > 0)
            {
                FileManagerControl.HistoryIndex--;
                //Navigate(HistoryIndex);
            }*/
            FileManagerControl.NavigateBack();
            ValidateNavButtonStates();
        }

        private void NavForwardButton_Click(object sender, RoutedEventArgs e)
        {
            /*if (FileManagerControl.HistoryIndex < (FileManagerControl.HistoryList.Count - 1))
            {
                FileManagerControl.HistoryIndex++;
                //Navigate(HistoryIndex);
            }*/
            FileManagerControl.NavigateForward();
            ValidateNavButtonStates();
        }

        private void NavHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("HISTORY: ");
            foreach (string s in FileManagerControl.HistoryList)
                Debug.WriteLine(FileManagerControl.HistoryList.IndexOf(s).ToString() + ": " + s);
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

        public void Navigate(string path)
        {
            Navigate(path, false);
        }

        public void Navigate(string path, bool updateFileManager)
        {
            var item = new DiskItem(path);
            Title = Path.GetFileName(path);
            Icon = ((ImageBrush)((new Start9.UI.Wpf.Converters.IconToImageBrushConverter()).Convert(item.ItemLargeIcon, null, "32", null))).ImageSource;

            if (updateFileManager)
                FileManagerControl.Navigate(path);

            ValidateNavButtonStates();
            //FileManagerControl.CurrentDirectoryListView.ItemsSource = item.SubItems;
            //AddressBox.Text = currentPath;
            //BreadcrumbsBar.Path = @"Computer\" + path;
            Title = Path.GetFileName(FileManagerControl.CurrentPath);
        }

        public void ValidateNavButtonStates()
        {
            NavBar.ValidateNavButtonStates((FileManagerControl.HistoryIndex == 0), (FileManagerControl.HistoryIndex >= (FileManagerControl.HistoryList.Count - 1)), (FileManagerControl.HistoryList.Count > 1), (Directory.Exists(Path.GetDirectoryName(FileManagerControl.CurrentPath))));
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

        private void NavUpButton_Click(object sender, RoutedEventArgs e)
        {
            /*try
            {
                string path = Path.GetDirectoryName(FileManagerControl.CurrentPath);
                if (Directory.Exists(path))
                    FileManagerControl.Navigate(path);

                ValidateNavButtonStates();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }*/

            FileManagerControl.NavigateUp();
            ValidateNavButtonStates();
        }

        private void FileManagerBase_CurrentDirectorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileManagerControl.CurrentDirectoryListView.SelectedItems.Count == 0)
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
            }

            ////////SelectedItemCounter.Content = CurrentDirectoryListView.SelectedItems.Count.ToString() + " items selected";
        }

        private void ClipboardContents_CollectionChanged(Object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Manager.ClipboardContents.Count == 0)
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
                FileManagerControl.CurrentView = FileManagerBase.FileBrowserView.List;
            else if (FolderViewsGallery.SelectedItem == DetailsViewGalleryItem)
                FileManagerControl.CurrentView = FileManagerBase.FileBrowserView.Details;
            else if (FolderViewsGallery.SelectedItem == TilesViewGalleryItem)
                FileManagerControl.CurrentView = FileManagerBase.FileBrowserView.Tiles;
            else if (FolderViewsGallery.SelectedItem == ContentViewGalleryItem)
                FileManagerControl.CurrentView = FileManagerBase.FileBrowserView.Content;
            else
                FileManagerControl.CurrentView = FileManagerBase.FileBrowserView.Icons;

            Debug.WriteLine("CurrentView: " + FileManagerControl.CurrentView.ToString());
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

            if (((List<DiskItem>)FileManagerControl.CurrentDirectoryListView.ItemsSource)[(item.Parent as ListView).Items.IndexOf(item)].ItemCategory == DiskItem.DiskItemCategory.Directory)
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
            FileManagerControl.OpenSelection();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            FileManagerControl.CopySelection();
        }

        private void CopyPathButton_Click(object sender, RoutedEventArgs e)
        {
            FileManagerControl.CopyPathToSelection();
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            FileManagerControl.PasteCurrent();
        }

        private void PasteShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            FileManagerControl.PasteShortcut();
        }

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            FileManagerControl.CutSelection();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            FileManagerControl.DeleteSelection();
        }

        private void PropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            FileManagerControl.ShowPropertiesForSelection();
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            FileManagerControl.RenameSelection();
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FileManagerControl.CreateNewFolder();
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            FileManagerControl.SelectAll();
        }

        private void SelectNoneButton_Click(object sender, RoutedEventArgs e)
        {
            FileManagerControl.SelectNone();
        }

        private void InvertSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            FileManagerControl.InvertSelection();
        }

        private void BackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FileManagerControl.NavigateBack();
            ValidateNavButtonStates();
        }

        private void ForwardCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FileManagerControl.NavigateForward();
            ValidateNavButtonStates();
        }

        private void UpLevelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FileManagerControl.NavigateUp();
            ValidateNavButtonStates();
        }
    }
}
