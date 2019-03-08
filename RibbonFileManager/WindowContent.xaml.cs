using Microsoft.VisualBasic.FileIO;
using Start9.UI.Wpf.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using WindowsSharp.DiskItems;

namespace RibbonFileManager
{
    /// <summary>
    /// Interaction logic for WindowContent.xaml
    /// </summary>
    public partial class WindowContent : UserControl
    {
        public bool IsRenamingFiles
        {
            get => (bool)GetValue(IsRenamingFilesProperty);
            set => SetValue(IsRenamingFilesProperty, value);
        }

        public static readonly DependencyProperty IsRenamingFilesProperty = DependencyProperty.Register("IsRenamingFiles", typeof(bool), typeof(WindowContent), new PropertyMetadata(false));

        public MainWindow OwnerWindow
        {
            get => Window.GetWindow(this) as MainWindow;
        }

        public static event EventHandler<EventArgs> CurrentDirectorySelectionChanged;

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

        public static readonly DependencyProperty HistoryIndexProperty = DependencyProperty.Register("HistoryIndex", typeof(Int32), typeof(WindowContent), new PropertyMetadata(0, OnHistoryIndexPropertyChangedCallback));

        static void OnHistoryIndexPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WindowContent sender = d as WindowContent;
            var oldValue = (Int32)e.OldValue;
            var newValue = (Int32)e.NewValue;
            sender.Navigate(sender.HistoryList[newValue]);
        }

        public string CurrentPath
        {
            get => HistoryList[HistoryIndex];
        }

        public string CurrentDirectoryName
        {
            get => (string)GetValue(CurrentDirectoryNameProperty);
            set => SetValue(CurrentDirectoryNameProperty, value);
        }

        public static readonly DependencyProperty CurrentDirectoryNameProperty = DependencyProperty.Register("CurrentDirectoryName", typeof(string), typeof(WindowContent), new PropertyMetadata(string.Empty));

        public Double IconSize
        {
            get => (Double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register("IconSize", typeof(Double), typeof(WindowContent), new PropertyMetadata((Double)48.0));

        public bool ShowDetailsPane
        {
            get => (bool)GetValue(ShowDetailsPaneProperty);
            set => SetValue(ShowDetailsPaneProperty, value);
        }

        public static readonly DependencyProperty ShowDetailsPaneProperty = DependencyProperty.Register("ShowDetailsPane", typeof(bool), typeof(WindowContent), new PropertyMetadata(false));

        public bool ShowPreviewPane
        {
            get => (bool)GetValue(ShowPreviewPaneProperty);
            set => SetValue(ShowPreviewPaneProperty, value);
        }

        public static readonly DependencyProperty ShowPreviewPaneProperty = DependencyProperty.Register("ShowPreviewPane", typeof(bool), typeof(WindowContent), new PropertyMetadata(false));

        public bool ShowNavigationPane
        {
            get => (bool)GetValue(ShowNavigationPaneProperty);
            set => SetValue(ShowNavigationPaneProperty, value);
        }

        public static readonly DependencyProperty ShowNavigationPaneProperty = DependencyProperty.Register("ShowNavigationPane", typeof(bool), typeof(WindowContent), new PropertyMetadata(true));

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

        public static readonly DependencyProperty CurrentViewProperty = DependencyProperty.Register("CurrentView", typeof(FileBrowserView), typeof(WindowContent), new PropertyMetadata(FileBrowserView.Icons));

        public bool ShowItemCheckboxes
        {
            get => (bool)GetValue(ShowItemCheckboxesProperty);
            set => SetValue(ShowItemCheckboxesProperty, value);
        }

        public static readonly DependencyProperty ShowItemCheckboxesProperty = DependencyProperty.Register("ShowItemCheckboxes", typeof(bool), typeof(WindowContent), new PropertyMetadata(false));

        string _initPath;
        public WindowContent(string path)
        {
            InitializeComponent();
            _initPath = path;
            if (HistoryList.Count == 0)
                HistoryList.Add(_initPath);

            Loaded += WindowContent_Loaded;
        }

        private void WindowContent_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= WindowContent_Loaded;
            Navigate(_initPath);
        }

        public void Navigate(string path)
        {
            if (Directory.Exists(path))
            {
                CurrentDirectoryListView.ItemsSource = new DiskItem(path).SubItems;
                CurrentDirectoryName = Path.GetFileName(path);

                if (!HistoryList.Contains(path))
                {
                    HistoryList.Add(path);

                    HistoryIndex = HistoryList.IndexOf(path);
                }

                OwnerWindow.Navigate(path);

                //OwnerWindow.ValidateNavButtonStates();
            }
        }

        private void AddressBox_KeyDown(object sender, KeyEventArgs e)
        {
            var bar = (sender as TextBox);
            if (e.Key == Key.Enter)
            {
                string expText = Environment.ExpandEnvironmentVariables(bar.Text);
                if (Directory.Exists(expText))
                {
                    Navigate(expText);
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

        private void RunAsAdminMenuItem_Loaded(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).Tag as ListViewItem;

            if (((List<DiskItem>)CurrentDirectoryListView.ItemsSource)[(item.Parent as ListView).Items.IndexOf(item)].ItemCategory == DiskItem.DiskItemCategory.Directory)
                (sender as MenuItem).Visibility = Visibility.Collapsed;
        }

        public void ShowPropertiesForSelection()
        {
            foreach (DiskItem i in CurrentDirectoryListView.SelectedItems)
            {
                i.ShowProperties();
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

        private void NavigationPaneTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var val = e.NewValue as DiskItem;

            if (val != null)
                Navigate(Environment.ExpandEnvironmentVariables(val.ItemPath));
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

        private void CurrentDirectoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DiskItem item = null;
            if (CurrentDirectoryListView.SelectedItem != null)
                item = CurrentDirectoryListView.SelectedItem as DiskItem;
            OwnerWindow.ValidateCommandStates(CurrentDirectoryListView.SelectedItems.Count, item);
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
                    paths += "\n";
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
            for (var i = 0; i < CurrentDirectoryListView.SelectedItems.Count; i++)
            {
                var d = (CurrentDirectoryListView.SelectedItems[i] as DiskItem);

                if (d.ItemCategory == DiskItem.DiskItemCategory.Directory)
                    FileSystem.DeleteDirectory(d.ItemPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                else if ((d.ItemCategory == DiskItem.DiskItemCategory.File) || (d.ItemCategory == DiskItem.DiskItemCategory.Shortcut))
                    FileSystem.DeleteFile(d.ItemPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
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

        private void FileManagerBase_CurrentDirectorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentDirectorySelectionChanged?.Invoke(this, null);
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


            if (item.ItemCategory != DiskItem.DiskItemCategory.Directory)
            {
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
                        /*bool isMediaFile = true;
                        PreviewPlayer.Source = new Uri(item.ItemPath, UriKind.RelativeOrAbsolute);
                        PreviewPlayer.MediaFailed += (sneder, args) =>
                        {
                            isMediaFile = false;
                        };

                        if (isMediaFile)
                            SetPreviewPaneLayer(3);
                        else //if (ext == "txt" || ext == "xml" || ext = "")
                        {*/
                        string content = File.ReadAllText(item.ItemPath);
                        if (content.Contains("\0\0"))
                            SetPreviewPaneLayer(1);
                        else
                        {
                            ((PreviewPaneGrid.Children[4] as ScrollViewer).Content as TextBlock).Text = content;
                            SetPreviewPaneLayer(4);
                        }
                        //}
                    }
                    /*else if (ext == "wav" || ext == "wma" || ext == "mp3" || ext == "m4a")
                    {

                    }
                    else if (ext == "mp4" || ext == "wmv" || ext == "mp3" || ext == "m4a")
                    {

                    }*/
                }
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
    }
}
