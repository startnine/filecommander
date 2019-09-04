using Microsoft.VisualBasic.FileIO;
using Start9.UI.Wpf.Windows;
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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace RibbonFileManager
{
    /// <summary>
    /// Interaction logic for WindowContent.xaml
    /// </summary>
    public partial class WindowContent : UserControl
    {
        /*public Boolean IsRenamingFiles
        {
            get => (Boolean)GetValue(IsRenamingFilesProperty);
            set => SetValue(IsRenamingFilesProperty, value);
        }

        public static readonly DependencyProperty IsRenamingFilesProperty = DependencyProperty.Register("IsRenamingFiles", typeof(Boolean), typeof(WindowContent), new PropertyMetadata(false));*/

        public MainWindow OwnerWindow
        {
            get => Window.GetWindow(this) as MainWindow;
        }

        public static event EventHandler CurrentDirectorySelectionChanged;

        CancellationTokenSource _source;

        public String CurrentFolderTitle
        {
            get
            {
                try
                {
                    return CurrentLocation.Name;
                }
                catch
                {
                    return String.Empty;
                }
            }
        }


        /*public NavigationStack<Location> NavigationStack { get; set; } = new NavigationStack<Location>();

        public RecentLocationsList<Location> RecentLocations { get; set; } = new RecentLocationsList<Location>();*/

        public NavigationManager<Location> NavManager { get; set; } = new NavigationManager<Location>();

        public IEnumerable<Location> HistoryElements => NavManager.Reverse();

        public Location CurrentLocation
        {
            get => NavManager.Current;
        }

        public String CurrentDisplayName
        {
            get => (String)GetValue(CurrentDisplayNameProperty);
            set => SetValue(CurrentDisplayNameProperty, value);
        }

        public static readonly DependencyProperty CurrentDisplayNameProperty = DependencyProperty.Register(nameof(CurrentDisplayName), typeof(String), typeof(WindowContent), new PropertyMetadata(String.Empty));

        public Double IconSize
        {
            get => (Double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(nameof(IconSize), typeof(Double), typeof(WindowContent), new PropertyMetadata((Double)48.0));

        public int IconViewLevel
        {
            get => (int)GetValue(IconViewLevelProperty);
            set => SetValue(IconViewLevelProperty, value);
        }

        public static readonly DependencyProperty IconViewLevelProperty = DependencyProperty.Register(nameof(IconViewLevel), typeof(int), typeof(WindowContent), new PropertyMetadata(208, OnIconViewLevelPropertyChanged));

        public static bool IsValidIconViewLevel(int val)
        {
            return (val >= 0) && (val <= 244);
        }

        static void OnIconViewLevelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WindowContent content)
            {
                int newIndex = (int)e.NewValue;
                if (newIndex > 0)
                {
                    if (newIndex <= 240)
                    {
                        content.CurrentView = FileBrowserView.Icons;
                        content.IconSize = 256 - newIndex;
                    }
                    else if (newIndex == 241)
                        content.CurrentView = FileBrowserView.List;
                    else if (newIndex == 242)
                        content.CurrentView = FileBrowserView.Details;
                    else if (newIndex == 243)
                        content.CurrentView = FileBrowserView.Tiles;
                    else if (newIndex == 244)
                        content.CurrentView = FileBrowserView.Content;
                }
            }
        }

        public FileBrowserView CurrentView
        {
            get => (FileBrowserView)GetValue(CurrentViewProperty);
            set => SetValue(CurrentViewProperty, value);
        }

        public static readonly DependencyProperty CurrentViewProperty = DependencyProperty.Register("CurrentView", typeof(FileBrowserView), typeof(WindowContent), new PropertyMetadata(FileBrowserView.Icons));

        public Boolean ShowItemCheckboxes
        {
            get => (Boolean)GetValue(ShowItemCheckboxesProperty);
            set => SetValue(ShowItemCheckboxesProperty, value);
        }

        public static readonly DependencyProperty ShowItemCheckboxesProperty = DependencyProperty.Register("ShowItemCheckboxes", typeof(Boolean), typeof(WindowContent), new PropertyMetadata(false));

        //String _initPath;
        Location _initLocation;

        public WindowContent(Location initLocation)
        {
            InitializeComponent();
            _initLocation = initLocation;
            Loaded += WindowContent_Loaded;
        }

        static String _shellLocationPrefix = "shell:::";

        public Location GetLocation(String path)
        {
            if (Directory.Exists(path))
                return new DirectoryQuery(path);
            else if (path.ToLowerInvariant().StartsWith(_shellLocationPrefix))
            {
                var guidString = path.Replace(_shellLocationPrefix, String.Empty);
                return new ShellLocation(Guid.Parse(guidString));
            }
            else if (Guid.TryParse(path, out Guid guid))
                return new ShellLocation(guid);
            else
                return null;
        }


        private void WindowContent_Loaded(Object sender, RoutedEventArgs e)
        {
            NavManager.Navigated += (sneder, args) => NavigateToLocationAsync(NavManager.Current, args.IsBackForwardNavigation);
            NavManager.MoveTo(_initLocation);
            //OwnerWindow.Navigate(NavManager.Current);
            Loaded -= WindowContent_Loaded;
        }

        static async IAsyncEnumerable<DiskItem> GetaDirectoryContents(String path, String query, CancellationToken token, Boolean recursive)
        {
            var entries = Directory.EnumerateFileSystemEntries(path, query, new EnumerationOptions { RecurseSubdirectories = recursive, IgnoreInaccessible = true });
            var enumer = entries.GetEnumerator();
            while (await Task.Run(enumer.MoveNext))
            {
                token.ThrowIfCancellationRequested();
                yield return await Task.Run(() => new DiskItem(enumer.Current));
                token.ThrowIfCancellationRequested();
            }
        }

        public async Task RefreshAsync(Location location, CancellationToken token = default)
        {
            //Debug.WriteLine("RefreshAsync(");
            _source?.Cancel();
            _source = new CancellationTokenSource();
            var source = CancellationTokenSource.CreateLinkedTokenSource(_source.Token, token);


            switch (location)
            {
                case DirectoryQuery l: await Navigate(l/*new SearchQuery(l.LocationPath, "*", false)*/, source); break;
                case SearchQuery s: await Navigate(s, source, false); break;
                case ShellLocation l: await Navigate(l, source); break;
            }
        }
        
        private void/*async Task */NavigateToLocationAsync(Location location, bool replaceFuture, CancellationToken token = default)
        {
            //Debug.WriteLine("NavigateToLocationAsync(");
            //NavigationStack.Add(location);
            /*if (replaceFuture)
                NavigationStack.ReplaceFuture(NavigationStack.Index + 1, location);
            else
                NavigationStack.Insert(NavigationStack.Index + 1, location);
            NavigationStack.Forward();

            RecentLocations.Navigate(location);*/
            /*await */
            RefreshAsync(location, token);
            CurrentDisplayName = location.Name;

            if (location is DirectoryQuery query)
                OwnerWindow.SetPanes(query);

            OwnerWindow.ValidateNavButtonStates();
        }

        async Task Navigate(Location l, CancellationTokenSource source, Boolean clearTextBox = true)
        {
            //Debug.WriteLine("Navigate(");
            var query = String.Empty;
            if (l is SearchQuery search)
                query = search.Query;

            //var old = CurrentDirectoryListView.ItemsSource;

            OwnerWindow.Navigate(l);

            if (clearTextBox)
                OwnerWindow.SearchTextBox.Clear();

            /*try
            {*/
                var results = new ObservableCollection<DiskItem>();
                //CurrentDirectoryListView.ItemsSource = results;
                var nextDirectoryIndex = 0;
                //Debug.WriteLine("l type: " + l.GetType().ToString());
                await foreach (var path in l.GetLocationContents(source.Token, false))
                {
                    if (path.ItemCategory == DiskItemCategory.Directory)
                    {
                        results.Insert(nextDirectoryIndex, path);
                        nextDirectoryIndex++;
                    }
                    else
                        results.Add(path);
                    source.Token.ThrowIfCancellationRequested();
                }

                CurrentDirectoryListView.ItemsSource = results;
                //Debug.WriteLine("CurrentDirectoryListView.ItemsSource = results");

                CollectionView collectionView = (CollectionView)CollectionViewSource.GetDefaultView(CurrentDirectoryListView.ItemsSource);

                if (l is ShellLocation shell)
                {
                    foreach (PropertyGroupDescription desc in shell.PropertyGroupDescriptions)
                    {
                        //Debug.WriteLine("Adding " + desc.PropertyName);
                        collectionView.GroupDescriptions.Add(desc);
                    }
                }
                else
                    collectionView.GroupDescriptions.Clear();
                /*}
                catch (OperationCanceledException) when (!String.IsNullOrEmpty(query)) // if the user canceled a search, then preserve what's been searched
                {
                }
                catch (Exception ex) // else, fall back to the previous results
                {
                    Debug.WriteLine("NAVIGATION ERROR:\nException: " + ex.ToString() + "\nStack Trace: \n" + ex.StackTrace + "\nEND ERROR INFO");
                    //CurrentDirectoryListView.ItemsSource = old;
                    MessageBox<OkActionSet>.Show(ex.ToString(), "Navigation error");
                }*/
            }

        private async void AddressBox_KeyDown(Object sender, KeyEventArgs e)
        {
            var bar = sender as TextBox;
            if (e.Key == Key.Enter)
            {
                var expText = Environment.ExpandEnvironmentVariables(bar.Text);
                if (Directory.Exists(expText))
                {
                    //await NavigateAsync(new DirectoryQuery(expText), true);
                    NavManager.MoveTo(new DirectoryQuery(expText));
                    CurrentDirectoryListView.Focus();
                    //NavBar_PathTextBox_LostFocus(null, null);
                }
                else
                {
                    var failText = expText;
                    MessageBox<OkActionSet>.Show("File Commander can't find '" + failText + "'. Check the #speeling and try again.", "File Commander"); //(this, "Ribbon File Browser can't find '" + failText + "'. Check the speeling and try again.", "Ribbon File Browser");
                }
            }
            else if (e.Key == Key.Escape)
            {
                CurrentDirectoryListView.Focus();
            }
        }

        private void RunAsAdminMenuItem_Loaded(Object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).Tag as ListViewItem;

            if (((List<DiskItem>)CurrentDirectoryListView.ItemsSource)[(item.Parent as ListView).Items.IndexOf(item)].ItemCategory == DiskItemCategory.Directory)
                (sender as MenuItem).Visibility = Visibility.Collapsed;
        }

        public void ShowPropertiesForSelection()
        {
            foreach (DiskItem i in CurrentDirectoryListView.SelectedItems)
            {
                i.ShowProperties();
            }
        }

        private async void CurrentDirectoryListView_KeyDown(Object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.C)
                    CopySelection();
                else if (e.Key == Key.X)
                    CutSelection();
                else if (e.Key == Key.V)
                    await PasteCurrentAsync();
            }
            else if (e.Key == Key.Delete)
                await DeleteSelectionAsync();
        }

        private void CurrentDirectoryListView_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            DiskItem item = null;
            if (CurrentDirectoryListView.SelectedItem != null)
            {
                item = CurrentDirectoryListView.SelectedItem as DiskItem;
                OwnerWindow.SetPanes(CurrentLocation);
            }

            OwnerWindow.ValidateCommandStates(CurrentDirectoryListView.SelectedItems.Count, item);
            OwnerWindow.SetPanes(CurrentLocation);
        }

        public async Task OpenPath(String path)
        {
            var expanded = Environment.ExpandEnvironmentVariables(path);
            if (Directory.Exists(expanded))
                NavManager.MoveTo(new DirectoryQuery(expanded)); //await NavigateAsync(new DirectoryQuery(expanded), true);
            else if (File.Exists(expanded))
                new DiskItem(expanded).Open();
            else
                MessageBox<OkActionSet>.Show("File Commander can't find '" + expanded + "'. Check the #SPEELING and try again.", "File Commander");
        }

        public async Task OpenSelectionAsync(DiskItem.OpenVerbs verb = DiskItem.OpenVerbs.Normal)
        {
            foreach (DiskItem i in CurrentDirectoryListView.SelectedItems)
            {
                if (i.ItemCategory == DiskItemCategory.Directory)
                {
                    if (CurrentDirectoryListView.SelectedItems.Count == 1)
                    {
                        if (Config.Instance.OpenFoldersInNewWindow)
                        {
                            var win = new MainWindow();
                            win.Tabs.Insert(0, new FolderTabItem(new DirectoryQuery(((DiskItem)CurrentDirectoryListView.SelectedItem).ItemPath)));
                            while (win.Tabs.Count > 1)
                                win.Tabs.RemoveAt(1);

                            win.Show();
                            win.Focus();
                            win.Activate();
                        }
                        else
                        {
                            NavManager.MoveTo(new DirectoryQuery(i.ItemPath));
                            //await NavigateAsync(new DirectoryQuery(i.ItemPath), true);
                        }
                        break;
                    }
                }
                else
                {
                    /*var info = new ProcessStartInfo()
                    {
                        FileName = i.ItemPath,
                        UseShellExecute = true
                    };
                    Process.Start(info);*/
                    i.Open();
                }
            }
        }

        public void EditSelection()
        {

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
                var d = (DiskItem)CurrentDirectoryListView.SelectedItems[i];
                paths = paths + "\"" + d.ItemPath + "\"";
                if (i < (CurrentDirectoryListView.SelectedItems.Count - 1))
                {
                    paths += "\n";
                }
            }
            Clipboard.SetText(paths);
        }

        public async Task PasteCurrentAsync()
        {
            if (!(NavManager.Current is DirectoryQuery curr)) return;

            System.Collections.Specialized.StringCollection clipboard = null;
            if (Clipboard.ContainsFileDropList())
            {
                clipboard = Clipboard.GetFileDropList();
                foreach (var s in clipboard)
                {
                    var outPath = Path.Combine(CurrentLocation.LocationPath, Path.GetFileName(s));
                    if (File.Exists(s))
                    {
                        if (s.ToLowerInvariant() == outPath.ToLowerInvariant())
                        {
                            var fixBasePath = outPath + " - Copy";
                            var fixedPath = fixBasePath;
                            var cycle = 1;
                            while (File.Exists(fixedPath))
                            {
                                fixedPath = fixBasePath + " (" + cycle.ToString() + ")";
                                cycle++;
                            }
                            FileSystem.CopyFile(s, fixedPath, UIOption.AllDialogs);
                        }
                        else if (File.Exists(outPath))
                        {
                            var fixedPath = outPath;
                            var cycle = 1;
                            while (File.Exists(fixedPath))
                            {
                                fixedPath = outPath + " (" + cycle.ToString() + ")";
                                cycle++;
                            }
                            FileSystem.CopyFile(s, fixedPath, UIOption.AllDialogs);
                        }
                        else
                            FileSystem.CopyFile(s, outPath, UIOption.AllDialogs); //File.Copy(s, outPath);
                    }
                    else if (Directory.Exists(s))
                    {
                        if (s.ToLowerInvariant() == outPath.ToLowerInvariant())
                        {
                            var fixBasePath = outPath + " - Copy";
                            var fixedPath = fixBasePath;
                            var cycle = 1;
                            while (Directory.Exists(fixedPath))
                            {
                                fixedPath = fixBasePath + " (" + cycle.ToString() + ")";
                                cycle++;
                            }
                            FileSystem.CopyDirectory(s, fixedPath, UIOption.AllDialogs);
                        }
                        else if (Directory.Exists(outPath))
                        {
                            var fixedPath = outPath;
                            var cycle = 1;
                            while (Directory.Exists(fixedPath))
                            {
                                fixedPath = outPath + " (" + cycle.ToString() + ")";
                                cycle++;
                            }
                            FileSystem.CopyDirectory(s, fixedPath, UIOption.AllDialogs);
                        }
                        else
                            FileSystem.CopyDirectory(s, outPath, UIOption.AllDialogs);
                    }
                }
            }
            else
            {
                var items = Config.PasteIn(curr.Item.ItemPath);
                var source = (ObservableCollection<DiskItem>)CurrentDirectoryListView.ItemsSource;

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

            await RefreshAsync();
        }

        public void SetClipboard()
        {
            System.Collections.Specialized.StringCollection paths = new System.Collections.Specialized.StringCollection();
            foreach (DiskItem d in CurrentDirectoryListView.SelectedItems)
                paths.Add(d.ItemPath);
            Clipboard.SetFileDropList(paths);

            OwnerWindow.UpdateClipboardButtons();

            /*Config.ClipboardContents.Clear();
            foreach (DiskItem d in CurrentDirectoryListView.SelectedItems)
            {
                Config.ClipboardContents.Add(d);
            }*/
        }

        public async Task RefreshAsync()
        {
            await RefreshAsync(CurrentLocation);
        }

        public async Task PasteShortcutAsync()
        {
            await RefreshAsync();
        }

        public async Task DeleteSelectionAsync()
        {
            for (var i = 0; i < CurrentDirectoryListView.SelectedItems.Count; i++)
            {
                var d = CurrentDirectoryListView.SelectedItems[i] as DiskItem;

                if (d.ItemCategory == DiskItemCategory.Directory)
                {
                    FileSystem.DeleteDirectory(d.ItemPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                    (CurrentDirectoryListView.ItemsSource as ObservableCollection<DiskItem>).Remove(d);
                }
                else if ((d.ItemCategory == DiskItemCategory.File) || (d.ItemCategory == DiskItemCategory.Shortcut))
                {
                    FileSystem.DeleteFile(d.ItemPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                    (CurrentDirectoryListView.ItemsSource as ObservableCollection<DiskItem>).Remove(d);
                }
            }

            //await RefreshAsync();
        }

        public void RenameSelection()
        {
            foreach (DiskItem s in CurrentDirectoryListView.SelectedItems)
                s.IsRenaming = true;
            //(CurrentDirectoryListView.ItemsSource as ObservableCollection<DiskItem>)
            //IsRenamingFiles = true;
        }

        public async Task CreateNewFolderAsync()
        {
            var path = Path.Combine(CurrentLocation.LocationPath, "New Folder");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            else
            {
                var cycle = 1;
                while (Directory.Exists(path))
                {
                    path = Path.Combine(CurrentLocation.LocationPath, @"New Folder (" + cycle.ToString() + ")");
                    MessageBox<OkActionSet>.Show(path, string.Empty);
                    cycle++;
                }
                Directory.CreateDirectory(path);
            }
            await RefreshAsync();
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
            foreach (DiskItem item in CurrentDirectoryListView.Items)
            {
                //var container = CurrentDirectoryListView.ContainerFromElement(item); //wat
                //item.IsSelected = !item.IsSelected;
            }
        }

        private void FileManagerBase_CurrentDirectorySelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            CurrentDirectorySelectionChanged?.Invoke(this, null);
        }

        private GroupStyle SelectGroupStyle(CollectionViewGroup group, Int32 level)
        {

            return (GroupStyle)FindResource("myGroupStyle");
        }


        GridViewColumnHeader _currentDirectoryListViewLastHeaderClicked = null;
        ListSortDirection _currentDirectoryListViewLastSortDirection = ListSortDirection.Ascending;

        private void CurrentDirectoryListView_GridViewColumnHeader_Click(Object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection sortDirection = ListSortDirection.Ascending;

            if (header != null)
            {
                if ((_currentDirectoryListViewLastHeaderClicked != null) && (header.Role == _currentDirectoryListViewLastHeaderClicked.Role) && (_currentDirectoryListViewLastSortDirection == ListSortDirection.Ascending))
                    sortDirection = ListSortDirection.Descending;

                var columnBinding = header.Column.DisplayMemberBinding as Binding;
                var sortBy = columnBinding?.Path.Path ?? header.Column.Header as String;

                SortCurrentDirectoryListView(sortBy, sortDirection);

                if (sortDirection == ListSortDirection.Descending)
                    header.SetResourceReference(GridViewColumnHeader.ContentTemplateProperty, "CurrentDirectoryListViewColumnHeaderDescendingTemplate");
                else
                    header.SetResourceReference(GridViewColumnHeader.ContentTemplateProperty, "CurrentDirectoryListViewColumnHeaderAscendingTemplate");

                _currentDirectoryListViewLastHeaderClicked = header;
                _currentDirectoryListViewLastSortDirection = sortDirection;
            }
        }

        private void SortCurrentDirectoryListView(String sortBy, ListSortDirection direction)
        {
            ICollectionView currentDirectoryItemsSource = CollectionViewSource.GetDefaultView(CurrentDirectoryListView.ItemsSource);

            currentDirectoryItemsSource.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            currentDirectoryItemsSource.SortDescriptions.Add(sd);
            currentDirectoryItemsSource.Refresh();
        }
        

        private void WindowContent_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                int oldIconViewLevel = IconViewLevel;
                int newIconViewLevel = IconViewLevel;
                if (e.Delta > 0) //scroll up
                    newIconViewLevel--;
                else if (e.Delta < 0) //scroll down
                    newIconViewLevel++;

                if (IsValidIconViewLevel(newIconViewLevel) && (oldIconViewLevel != newIconViewLevel))
                {
                    IconViewLevel = newIconViewLevel;
                }

                if (e.Delta != 0)
                    e.Handled = true;
            }
        }
    }

    public enum FileBrowserView
    {
        Icons,
        List,
        Details,
        Tiles,
        Content
    }
}
