using Microsoft.VisualBasic.FileIO;
using Start9.UI.Wpf.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public Boolean IsRenamingFiles
        {
            get => (Boolean)GetValue(IsRenamingFilesProperty);
            set => SetValue(IsRenamingFilesProperty, value);
        }

        public static readonly DependencyProperty IsRenamingFilesProperty = DependencyProperty.Register("IsRenamingFiles", typeof(Boolean), typeof(WindowContent), new PropertyMetadata(false));

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


        public NavigationStack<Location> NavigationStack { get; set; } = new NavigationStack<Location>();

        public RecentLocationsList<Location> RecentLocations { get; set; } = new RecentLocationsList<Location>();

        public IEnumerable<Location> HistoryElements => RecentLocations.Reverse();

        public Location CurrentLocation
        {
            get => NavigationStack.Current;
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

        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register("IconSize", typeof(Double), typeof(WindowContent), new PropertyMetadata((Double)48.0));

        public Boolean ShowDetailsPane
        {
            get => (Boolean)GetValue(ShowDetailsPaneProperty);
            set => SetValue(ShowDetailsPaneProperty, value);
        }

        public static readonly DependencyProperty ShowDetailsPaneProperty = DependencyProperty.Register("ShowDetailsPane", typeof(Boolean), typeof(WindowContent), new PropertyMetadata(false));

        public Boolean ShowPreviewPane
        {
            get => (Boolean)GetValue(ShowPreviewPaneProperty);
            set => SetValue(ShowPreviewPaneProperty, value);
        }

        public static readonly DependencyProperty ShowPreviewPaneProperty = DependencyProperty.Register("ShowPreviewPane", typeof(Boolean), typeof(WindowContent), new PropertyMetadata(false));

        public Boolean ShowNavigationPane
        {
            get => (Boolean)GetValue(ShowNavigationPaneProperty);
            set => SetValue(ShowNavigationPaneProperty, value);
        }

        public static readonly DependencyProperty ShowNavigationPaneProperty = DependencyProperty.Register("ShowNavigationPane", typeof(Boolean), typeof(WindowContent), new PropertyMetadata(true));

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

        public WindowContent(String path)
        {
            InitializeComponent();
            _initLocation = GetLocation(path);
            if (NavigationStack.Count == 0)
                NavigationStack.Add(_initLocation);

            Loaded += WindowContent_Loaded;
        }

        static string _shellLocationPrefix = "shell:::";

        public Location GetLocation(string path)
        {
            if (Directory.Exists(path))
                return new DirectoryQuery(path);
            else if (path.ToLowerInvariant().StartsWith(_shellLocationPrefix))
            {
                string guidString = path.Replace(_shellLocationPrefix, string.Empty);
                return new ShellLocation(Guid.Parse(guidString));
            }
            else
                return null;
        }

        private async void WindowContent_Loaded(Object sender, RoutedEventArgs e)
        {
            Loaded -= WindowContent_Loaded;
            await NavigateAsync(_initLocation);
        }

        static async IAsyncEnumerable<DiskItem> GetDirectoryContents(String path, String query, CancellationToken token, Boolean recursive)
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

        static async IAsyncEnumerable<DiskItem> GetShellLocationContents(ShellLocation location, CancellationToken token, Boolean recursive)
        {
            if (location.LocationGuid == ShellLocation.ThisPcGuid)
            {
                var entries = new List<string>()
                    {
                        Environment.ExpandEnvironmentVariables(@"%userprofile%\Desktop"),
                        Environment.ExpandEnvironmentVariables(@"%userprofile%\Documents"),
                        Environment.ExpandEnvironmentVariables(@"%userprofile%\Downloads"),
                        Environment.ExpandEnvironmentVariables(@"%userprofile%\Music"),
                        Environment.ExpandEnvironmentVariables(@"%userprofile%\Pictures"),
                        Environment.ExpandEnvironmentVariables(@"%userprofile%\Videos")
                    };
                foreach (string s in entries)
                {
                    token.ThrowIfCancellationRequested();
                    yield return await Task.Run(() => new DiskItem(s));
                    token.ThrowIfCancellationRequested();
                }

                foreach (DriveInfo d in System.IO.DriveInfo.GetDrives())
                {
                    token.ThrowIfCancellationRequested();
                    Debug.WriteLine("d.RootDirectory.FullName: " + d.RootDirectory.FullName);
                    yield return await Task.Run(() => new DiskItem(d.RootDirectory.FullName));
                    token.ThrowIfCancellationRequested();
                }
            }
            else //We don't know what this GUID is
            {
                throw new Exception("Unrecognized Shell Location GUID");
            }
        }

        public async Task RefreshAsync(Location location, CancellationToken token = default)
        {
            _source?.Cancel();
            _source = new CancellationTokenSource();
            var source = CancellationTokenSource.CreateLinkedTokenSource(_source.Token, token);


            switch (location)
            {
                case DirectoryQuery l: await Navigate(new SearchQuery(l.Item.ItemPath, "*", false), source); break;
                case SearchQuery s: await Navigate(s, source, false); break;
                case ShellLocation l: await NavigateShellLocation(l, source, false); break;
            }
        }
        
        public async Task NavigateAsync(Location location, CancellationToken token = default)
        {
            NavigationStack.Add(location);
            NavigationStack.Forward();

            RecentLocations.Navigate(location);
            await RefreshAsync(location, token);
            CurrentDisplayName = location.Name;
        }

        async Task Navigate(SearchQuery l, CancellationTokenSource source, Boolean clearTextBox = true)
        {
            var old = CurrentDirectoryListView.ItemsSource;

            OwnerWindow.Navigate(l);

            if (clearTextBox)
                OwnerWindow.SearchTextBox.Clear();

            try
            {
                var results = new ObservableCollection<DiskItem>();
                CurrentDirectoryListView.ItemsSource = results;
                int nextDirectoryIndex = 0;
                Debug.WriteLine("l type: " + l.GetType().ToString());
                await foreach (var path in GetDirectoryContents(l.Path, l.Query, source.Token, l.Recursive))
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
            }
            catch (OperationCanceledException) when (!String.IsNullOrEmpty(l.Query)) // if the user canceled a search, then preserve what's been searched
            {
            }
            catch // else, fall back to the previous results
            {
                CurrentDirectoryListView.ItemsSource = old;
            }
        }

        async Task NavigateShellLocation(ShellLocation l, CancellationTokenSource source, Boolean clearTextBox = true)
        {
            var old = CurrentDirectoryListView.ItemsSource;

            OwnerWindow.Navigate(l);

            if (clearTextBox)
                OwnerWindow.SearchTextBox.Clear();

            try
            {
                var results = new ObservableCollection<DiskItem>();
                CurrentDirectoryListView.ItemsSource = results;
                int nextDirectoryIndex = 0;
                Debug.WriteLine("l type: " + l.GetType().ToString());
                await foreach (var path in GetShellLocationContents(l, source.Token, true))
                {
                    if (path.ItemCategory == DiskItemCategory.Directory)
                    {
                        results.Insert(nextDirectoryIndex, path);
                        nextDirectoryIndex++;
                    }
                    else
                    {
                        results.Add(path);
                        source.Token.ThrowIfCancellationRequested();
                    }
                }
            }
            catch // fall back to the previous results
            {
                CurrentDirectoryListView.ItemsSource = old;
            }
        }

        private async void AddressBox_KeyDown(Object sender, KeyEventArgs e)
        {
            var bar = sender as TextBox;
            if (e.Key == Key.Enter)
            {
                var expText = Environment.ExpandEnvironmentVariables(bar.Text);
                if (Directory.Exists(expText))
                {
                    await NavigateAsync(new DirectoryQuery(expText));
                    CurrentDirectoryListView.Focus();
                    //NavBar_PathTextBox_LostFocus(null, null);
                }
                else
                {
                    var failText = expText;
                    MessageBox<MessageBoxEnums.OkButton>.Show("Ribbon File Browser can't find '" + failText + "'. Check the #speeling and try again.", "File Commander"); //(this, "Ribbon File Browser can't find '" + failText + "'. Check the speeling and try again.", "Ribbon File Browser");
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

        private async void NavigationPaneTreeView_SelectedItemChanged(Object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
            if (e.NewValue is DiskItem val)
                await NavigateAsync(new DirectoryQuery(Environment.ExpandEnvironmentVariables(val.ItemPath)));
            else
            {
                Debug.WriteLine("Tree SelectedValuePath: " + NavigationPaneTreeView.SelectedValuePath);
                /*if (NavigationPaneTreeView.SelectedValuePath == MyComputerTreeViewItem.Header)
                    await NavigateAsync(new ShellLocation(ShellLocation.ThisPcGuid));*/
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
                item = CurrentDirectoryListView.SelectedItem as DiskItem;
            OwnerWindow.ValidateCommandStates(CurrentDirectoryListView.SelectedItems.Count, item);
        }

        public async Task OpenSelectionAsync(DiskItem.OpenVerbs verb = DiskItem.OpenVerbs.Normal)
        {
            foreach (DiskItem i in CurrentDirectoryListView.SelectedItems)
            {
                if (i.ItemCategory == DiskItemCategory.Directory)
                {
                    if (CurrentDirectoryListView.SelectedItems.Count == 1) //
                    {
                        await NavigateAsync(new DirectoryQuery(i.ItemPath));
                        break;
                    }
                }
                else
                {
                    var info = new ProcessStartInfo()
                    {
                        FileName = i.ItemPath,
                        UseShellExecute = true
                    };
                    Process.Start(info);
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
            if (!(NavigationStack.Current is DirectoryQuery curr)) return;

            System.Collections.Specialized.StringCollection clipboard = null;
            if (Clipboard.ContainsFileDropList())
            {
                clipboard = Clipboard.GetFileDropList();
                foreach (string s in clipboard)
                {
                    string outPath = Path.Combine(CurrentLocation.LocationPath, Path.GetFileName(s));
                    if (File.Exists(s))
                    {
                        if (s.ToLowerInvariant() == outPath.ToLowerInvariant())
                        {
                            string fixBasePath = outPath + " - Copy";
                            string fixedPath = fixBasePath;
                            int cycle = 1;
                            while (File.Exists(fixedPath))
                            {
                                fixedPath = fixBasePath + " (" + cycle.ToString() + ")";
                                cycle++;
                            }
                            FileSystem.CopyFile(s, fixedPath, UIOption.AllDialogs);
                        }
                        else if (File.Exists(outPath))
                        {
                            string fixedPath = outPath;
                            int cycle = 1;
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
                            string fixBasePath = outPath + " - Copy";
                            string fixedPath = fixBasePath;
                            int cycle = 1;
                            while (Directory.Exists(fixedPath))
                            {
                                fixedPath = fixBasePath + " (" + cycle.ToString() + ")";
                                cycle++;
                            }
                            FileSystem.CopyDirectory(s, fixedPath, UIOption.AllDialogs);
                        }
                        else if (Directory.Exists(outPath))
                        {
                            string fixedPath = outPath;
                            int cycle = 1;
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
            IsRenamingFiles = true;
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
                    MessageBox.Show(path);
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
            foreach (ListViewItem item in CurrentDirectoryListView.Items)
                item.IsSelected = !item.IsSelected;
        }

        private void FileManagerBase_CurrentDirectorySelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            CurrentDirectorySelectionChanged?.Invoke(this, null);
        }

        public void SetPanes(DiskItem item)
        {
            var size = DetailsFileIconRectangle.ActualHeight;
            if (size <= 0)
                size = 48;
            DetailsFileIconRectangle.Fill = (ImageBrush) new Start9.UI.Wpf.Converters.IconToImageBrushConverter().Convert(item.ItemJumboIcon, null, size.ToString(), null);
            DetailsFileNameTextBlock.Text = 
                (item.ItemCategory == DiskItemCategory.Directory) && (item.ItemDisplayName == CurrentLocation.Name)
                ? item.SubItems.Count.ToString() + " items"
                : item.ItemDisplayName;


            if (item.ItemCategory != DiskItemCategory.Directory)
            {
                if (item.ItemPath.ToLowerInvariant() == CurrentLocation.Name.ToLowerInvariant())
                {
                    SetPreviewPaneLayer(0);
                }
                else
                {
                    var ext = Path.GetExtension(item.ItemPath).ToLowerInvariant();
                    if (ext == "bmp" || ext == "png" || ext == "jpg" || ext == "jpeg")
                    {
                        (PreviewPaneGrid.Children[2] as System.Windows.Shapes.Rectangle).Fill = new ImageBrush(new BitmapImage(new Uri(item.ItemPath, UriKind.RelativeOrAbsolute)));
                        SetPreviewPaneLayer(2);
                    }
                    else
                    {
                        var content = File.ReadAllText(item.ItemPath);
                        if (content.Contains("\0\0"))
                            SetPreviewPaneLayer(1);
                        else
                        {
                            ((PreviewPaneGrid.Children[4] as ScrollViewer).Content as TextBlock).Text = content;
                            SetPreviewPaneLayer(4);
                        }
                    }
                }
            }
        }

        void SetPreviewPaneLayer(Int32 index)
        {
            for (var i = 0; i < PreviewPaneGrid.Children.Count; i++)
            {
                var control = PreviewPaneGrid.Children[i];
                control.Visibility = i == index ? Visibility.Visible : Visibility.Collapsed;
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
