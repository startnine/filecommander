using Start9.UI.Wpf.Statics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RibbonFileManager
{
    public class TabManager : DependencyObject
    {
        static string _groupsSetAsidePath = Environment.ExpandEnvironmentVariables(@"%appdata%\Start9\TempData\FileCommander_TabGroups");
        public static string SlashReplacement = @"((SLASH))";

        public ObservableCollection<LocationTab> OpenTabs
        {
            get => (ObservableCollection<LocationTab>)GetValue(OpenTabsProperty);
            set => SetValue(OpenTabsProperty, value);
        }

        public static readonly DependencyProperty OpenTabsProperty = DependencyProperty.Register(nameof(OpenTabs), typeof(ObservableCollection<LocationTab>), typeof(TabManager), new PropertyMetadata(new ObservableCollection<LocationTab>()));

        public ObservableCollection<TabGroup> TabsSetAside
        {
            get => (ObservableCollection<TabGroup>)GetValue(TabsSetAsideProperty);
            set => SetValue(TabsSetAsideProperty, value);
        }

        public static readonly DependencyProperty TabsSetAsideProperty = DependencyProperty.Register(nameof(TabsSetAside), typeof(ObservableCollection<TabGroup>), typeof(TabManager), new PropertyMetadata(new ObservableCollection<TabGroup>()));

        public Visibility Visibility
        {
            get => (Visibility)GetValue(VisibilityProperty);
            set => SetValue(VisibilityProperty, value);
        }

        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register(nameof(Visibility), typeof(Visibility), typeof(TabManager), new PropertyMetadata(Visibility.Visible, OnVisibilityPropertyChangedCallback));

        static void OnVisibilityPropertyChangedCallback(Object sender, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine("OnVisibilityPropertyChangedCallback");

            if (((Visibility)e.NewValue) == Visibility.Visible)
                (sender as TabManager).Populate();
        }

        public TabManager()
        {
            if (!Directory.Exists(_groupsSetAsidePath))
                Directory.CreateDirectory(_groupsSetAsidePath);
        }

        public void Populate()
        {
            OpenTabs.Clear();
            TabsSetAside.Clear();

            foreach (MainWindow win in WindowManager.OpenWindows)
            {
                foreach (TabItem t in win.ContentTabControl.Items)
                    OpenTabs.Add(new LocationTab(t));
            }

            foreach (string s in Directory.EnumerateFiles(_groupsSetAsidePath))
            {
                TabsSetAside.Add(new TabGroup(s));
            }
        }

        public static void SetTabsAside(MainWindow window)
        {
            DateTime now = DateTime.Now;
            string outName = now.Year + "-" + now.Month + "-" + now.Day + "-" + now.Hour + "-" + now.Minute;
            List<string> paths = new List<string>();

            string thumbnailDir = Path.Combine(_groupsSetAsidePath, outName);
            if (!Directory.Exists(thumbnailDir))
                Directory.CreateDirectory(thumbnailDir);
            /*else
                foreach (string s in Directory.EnumerateFiles(thumbnailDir))
                    File.Delete(s);*/

            foreach (TabItem item in window.ContentTabControl.Items)
            {
                var loc = (item.Content as WindowContent).CurrentLocation;
                string path = loc.LocationPath.Replace(@"/", SlashReplacement).Replace(@"\", SlashReplacement);
                if (loc is ShellLocation shellLoc)
                    path = shellLoc.LocationGuid.ToString();

                paths.Add(path);

                var content = item.Content as WindowContent;

                //content.CurrentLocation.LocationPath.Replace(@"\", _slashReplacement))
                using (var fileStream = new FileStream(Path.Combine(thumbnailDir, loc.Name + ".png"), FileMode.Create))
                {
                    RenderTargetBitmap bitmap = new RenderTargetBitmap((int)SystemScaling.WpfUnitsToRealPixels(content.ActualWidth), (int)SystemScaling.WpfUnitsToRealPixels(content.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(content);

                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(fileStream);
                }
            }

            File.WriteAllLines(Path.Combine(_groupsSetAsidePath, outName + ".txt"), paths.ToArray());
            window.ContentTabControl.Items.Clear();
            window.AddTab();
            window.ShowHideTabsOverview(true);
        }
    }

    public class LocationTab : DependencyObject
    {
        /*public WindowContent Content
        {
            get => (WindowContent)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content), typeof(WindowContent), typeof(LocationTab), new PropertyMetadata(null));*/

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(LocationTab), new PropertyMetadata(string.Empty));

        TabItem _item = null;
        string _path = string.Empty;
        string _group = string.Empty;

        public LocationTab(TabItem item)
        {
            _item = item;
            //Content = item.Content as WindowContent;
            var location = (item.Content as WindowContent).CurrentLocation;
            Icon = location.Icon;
            Title = location.Name;
            Thumbnail = GetThumbnail();
        }

        public LocationTab(string path, string groupPath)
        {
            _path = path.Replace(TabManager.SlashReplacement, @"\");
            _group = groupPath;
            if (groupPath.Contains(".") && groupPath.Contains(@"\") && (groupPath.LastIndexOf(".") > groupPath.LastIndexOf(@"\")))
                _group = groupPath.Substring(0, _group.LastIndexOf("."));

            if (!Guid.TryParse(_path, out Guid result))
                Icon = new DiskItem(_path).ItemLargeIcon;

            if (File.Exists(_path) || Directory.Exists(_path))
                Title = Path.GetFileNameWithoutExtension(_path);

            Thumbnail = GetThumbnail();
        }

        public ImageBrush Thumbnail
        {
            get => (ImageBrush)GetValue(ThumbnailProperty);
            set => SetValue(ThumbnailProperty, value);
        }

        ImageBrush GetThumbnail()
        {
            if (_item != null)
            {
                var content = _item.Content as WindowContent;
                RenderTargetBitmap bitmap = new RenderTargetBitmap((int)SystemScaling.WpfUnitsToRealPixels(content.ActualWidth), (int)SystemScaling.WpfUnitsToRealPixels(content.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(content);
                return new ImageBrush(bitmap)
                {
                    Stretch = Stretch.Uniform
                };
            }
            else
            {
                string name = string.Empty;
                if (Directory.Exists(_path))
                    name = new DirectoryQuery(_path).Name;
                else if (Guid.TryParse(_path, out Guid guid))
                    name = new ShellLocation(guid).Name;

                return new ImageBrush(new BitmapImage(new Uri(Path.Combine(_group, name + ".png"), UriKind.RelativeOrAbsolute)))
                {
                    Stretch = Stretch.Uniform
                };
            }
        }

        public static readonly DependencyProperty ThumbnailProperty = DependencyProperty.Register(nameof(Thumbnail), typeof(ImageBrush), typeof(LocationTab), new PropertyMetadata(new ImageBrush()));

        Icon Icon
        {
            get => (Icon)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(Icon), typeof(LocationTab), new PropertyMetadata(null));

        public void SwitchTo()
        {
            if (_item != null)
            {
                var win = Window.GetWindow(_item);
                if ((win != null) && (win is MainWindow mainWin))
                {
                    mainWin.Show();
                    mainWin.Focus();
                    mainWin.Activate();

                    mainWin.ContentTabControl.SelectedItem = _item;
                }
                else
                    Debug.WriteLine("WINDOW CONTAINING SELECTED TAB WAS NOT FOUND");
            }
            else
                throw new Exception("Cannot switch to a tab which only exists in set aside form.");
        }
    }

    public class TabGroup : DependencyObject
    {
        public ObservableCollection<LocationTab> Tabs
        {
            get => (ObservableCollection<LocationTab>)GetValue(TabsProperty);
            set => SetValue(TabsProperty, value);
        }

        public static readonly DependencyProperty TabsProperty = DependencyProperty.Register(nameof(Tabs), typeof(ObservableCollection<LocationTab>), typeof(TabGroup), new PropertyMetadata(new ObservableCollection<LocationTab>()));

        public string Time
        {
            get => (string)GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(nameof(Time), typeof(string), typeof(TabGroup), new PropertyMetadata(string.Empty));

        string _path = string.Empty;

        public TabGroup(string path)
        {
            Tabs.Clear();

            foreach (string s in File.ReadAllLines(path))
            {
                _path = path.Replace(TabManager.SlashReplacement, @"/").Replace(TabManager.SlashReplacement, @"\");
                Tabs.Add(new LocationTab(s, _path));
            }

            string[] timePartStrings = Path.GetFileNameWithoutExtension(_path).Split('-');
            int[] timeParts = new int[timePartStrings.Length];

            for (int i = 0; i < timePartStrings.Length; i++)
                timeParts[i] = int.Parse(timePartStrings[i]);

            DateTime time = new DateTime(timeParts[0], timeParts[1], timeParts[2], timeParts[3], timeParts[4], 0);

            Time = time.ToLongDateString() + " " + time.ToShortTimeString();
        }
    }
}
