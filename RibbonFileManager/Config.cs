using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RibbonFileManager
{
    public class Config : DependencyObject
    {
        public enum InterfaceModeType
        {
            CommandBar,
            Ribbon,
            None
        }

        public enum TabDisplayMode
        {
            Titlebar,
            Toolbar,
            Disabled
        }

        string _configurationPath = string.Empty;


        //string _quickDestinationsPath = string.Empty;
        public ObservableCollection<Location> QuickDestinations
        {
            get => (ObservableCollection<Location>)GetValue(QuickDestinationsProperty);
            set => SetValue(QuickDestinationsProperty, value); //{ get; set; } = 
        }

        public static readonly DependencyProperty QuickDestinationsProperty =
            DependencyProperty.Register(nameof(QuickDestinations), typeof(ObservableCollection<Location>), typeof(Config), new FrameworkPropertyMetadata(new ObservableCollection<Location>(), FrameworkPropertyMetadataOptions.AffectsRender, OnCollectionPropertyChanged));

        /*static void OnQuickDestinationsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is Config inst) && (!string.IsNullOrWhiteSpace(inst._configurationPath)))
            {
                string[] outFileContents = new string[inst.QuickDestinations.Count];
                for (int i = 0; i < inst.QuickDestinations.Count; i++)
                    outFileContents[i] = inst.QuickDestinations[i].ToString();

                File.WriteAllLines(inst._quickDestinationsPath, outFileContents);
            }
        }*/

        //string _favoritesPath = string.Empty;
        public ObservableCollection<Location> Favorites
        {
            get => (ObservableCollection<Location>)GetValue(FavoritesProperty);
            set => SetValue(FavoritesProperty, value); //{ get; set; } = 
        }

        public static readonly DependencyProperty FavoritesProperty =
            DependencyProperty.Register(nameof(Favorites), typeof(ObservableCollection<Location>), typeof(Config), new FrameworkPropertyMetadata(new ObservableCollection<Location>(), FrameworkPropertyMetadataOptions.AffectsRender, OnCollectionPropertyChanged));


        public ObservableCollection<Location> ComputerSubFolders
        {
            get => (ObservableCollection<Location>)GetValue(ComputerSubFoldersProperty);
            set => SetValue(ComputerSubFoldersProperty, value); //{ get; set; } = 
        }

        public static readonly DependencyProperty ComputerSubFoldersProperty =
            DependencyProperty.Register(nameof(ComputerSubFolders), typeof(ObservableCollection<Location>), typeof(Config), new FrameworkPropertyMetadata(new ObservableCollection<Location>(), FrameworkPropertyMetadataOptions.AffectsRender, OnCollectionPropertyChanged));

        /*static void OnFavoritesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is Config inst) && (!string.IsNullOrWhiteSpace(inst._configurationPath)))
            {
                string[] outFileContents = new string[inst.Favorites.Count];
                for (int i = 0; i < inst.Favorites.Count; i++)
                    outFileContents[i] = inst.Favorites[i].ToString();

                File.WriteAllLines(inst._favoritesPath, outFileContents);
            }
        }*/


        //string _interfaceModePath = string.Empty;
        public InterfaceModeType InterfaceMode
        {
            get => (InterfaceModeType)GetValue(InterfaceModeProperty);
            set => SetValue(InterfaceModeProperty, value);
        }

        public static readonly DependencyProperty InterfaceModeProperty =
            DependencyProperty.Register(nameof(InterfaceMode), typeof(InterfaceModeType), typeof(Config), new FrameworkPropertyMetadata(InterfaceModeType.Ribbon, FrameworkPropertyMetadataOptions.AffectsRender, OnSinglePropertyChanged));

        //string _lockPanesPath = string.Empty;
        public Boolean LockPanes
        {
            get => (Boolean) GetValue(LockPanesProperty);
            set => SetValue(LockPanesProperty, value);
        }

        public static readonly DependencyProperty LockPanesProperty =
            DependencyProperty.Register(nameof(LockPanes), typeof(Boolean), typeof(Config), new PropertyMetadata(true, OnSinglePropertyChanged));

        #region Pane toggles

        public Boolean ShowNavigationBarPane
        {
            get => (Boolean)GetValue(ShowNavigationBarPaneProperty);
            set => SetValue(ShowNavigationBarPaneProperty, value);
        }

        public static readonly DependencyProperty ShowNavigationBarPaneProperty =
            DependencyProperty.Register(nameof(ShowNavigationBarPane), typeof(Boolean), typeof(Config), new PropertyMetadata(true, OnSinglePropertyChanged));

        public Boolean ShowNavigationTreePane
        {
            get => (Boolean)GetValue(ShowNavigationTreePaneProperty);
            set => SetValue(ShowNavigationTreePaneProperty, value);
        }

        public static readonly DependencyProperty ShowNavigationTreePaneProperty =
            DependencyProperty.Register(nameof(ShowNavigationTreePane), typeof(Boolean), typeof(Config), new PropertyMetadata(true, OnSinglePropertyChanged));

        public Boolean ShowDetailsPane
        {
            get => (Boolean)GetValue(ShowDetailsPaneProperty);
            set => SetValue(ShowDetailsPaneProperty, value);
        }

        public static readonly DependencyProperty ShowDetailsPaneProperty =
            DependencyProperty.Register(nameof(ShowDetailsPane), typeof(Boolean), typeof(Config), new PropertyMetadata(false, OnSinglePropertyChanged));

        public Boolean ShowPreviewPane
        {
            get => (Boolean)GetValue(ShowPreviewPaneProperty);
            set => SetValue(ShowPreviewPaneProperty, value);
        }

        public static readonly DependencyProperty ShowPreviewPaneProperty =
            DependencyProperty.Register(nameof(ShowPreviewPane), typeof(Boolean), typeof(Config), new PropertyMetadata(false, OnSinglePropertyChanged));

        public Boolean ShowStatusBar
        {
            get => (Boolean) GetValue(ShowStatusBarProperty);
            set => SetValue(ShowStatusBarProperty, value);
        }

        public static readonly DependencyProperty ShowStatusBarProperty =
            DependencyProperty.Register(nameof(ShowStatusBar), typeof(Boolean), typeof(Config), new PropertyMetadata(true, OnSinglePropertyChanged));

        #endregion Pane toggles

        //string _showEnhancedFolderIconsPath = string.Empty;
        public Boolean ShowEnhancedFolderIcons
        {
            get => (Boolean)GetValue(ShowEnhancedFolderIconsProperty);
            set => SetValue(ShowEnhancedFolderIconsProperty, value);
        }

        public static readonly DependencyProperty ShowEnhancedFolderIconsProperty =
            DependencyProperty.Register(nameof(ShowEnhancedFolderIcons), typeof(Boolean), typeof(Config), new PropertyMetadata(false, OnSinglePropertyChanged));


        //string _openFoldersInNewWindowPath = string.Empty;
        public Boolean OpenFoldersInNewWindow
        {
            get => (Boolean)GetValue(OpenFoldersInNewWindowProperty);
            set => SetValue(OpenFoldersInNewWindowProperty, value);
        }

        public static readonly DependencyProperty OpenFoldersInNewWindowProperty =
            DependencyProperty.Register(nameof(OpenFoldersInNewWindow), typeof(Boolean), typeof(Config), new PropertyMetadata(false, OnSinglePropertyChanged));


        //string _showItemSelectionCheckBoxes = string.Empty;
        public Boolean ShowItemSelectionCheckBoxes
        {
            get => (Boolean)GetValue(ShowItemSelectionCheckBoxesProperty);
            set => SetValue(ShowItemSelectionCheckBoxesProperty, value);
        }

        public static readonly DependencyProperty ShowItemSelectionCheckBoxesProperty =
            DependencyProperty.Register(nameof(ShowItemSelectionCheckBoxes), typeof(Boolean), typeof(Config), new PropertyMetadata(false, OnSinglePropertyChanged));


        //string _tabsModePath = string.Empty;
        public TabDisplayMode TabsMode
        {
            get => (TabDisplayMode)GetValue(TabsModeProperty);
            set => SetValue(TabsModeProperty, value);
        }

        public static readonly DependencyProperty TabsModeProperty =
            DependencyProperty.Register(nameof(TabsMode), typeof(TabDisplayMode), typeof(Config), new PropertyMetadata(TabDisplayMode.Titlebar, OnSinglePropertyChanged));


        //string _showTitlebarTextPath = string.Empty;
        public Boolean ShowTitlebarText
        {
            get => (Boolean) GetValue(ShowTitlebarTextProperty);
            set => SetValue(ShowTitlebarTextProperty, value);
        }

        public static readonly DependencyProperty ShowTitlebarTextProperty =
            DependencyProperty.Register(nameof(ShowTitlebarText), typeof(Boolean), typeof(Config), new PropertyMetadata(true, OnSinglePropertyChanged));


        //string _showTitlebarIconPath = string.Empty;
        public Boolean ShowTitlebarIcon
        {
            get => (Boolean)GetValue(ShowTitlebarIconProperty);
            set => SetValue(ShowTitlebarIconProperty, value);
        }

        public static readonly DependencyProperty ShowTitlebarIconProperty =
            DependencyProperty.Register(nameof(ShowTitlebarIcon), typeof(Boolean), typeof(Config), new PropertyMetadata(false, OnSinglePropertyChanged));

        /*public System.Windows.Controls.Dock DetailsPanePlacement
        {
            get => (System.Windows.Controls.Dock)GetValue(DetailsPanePlacementProperty);
            set => SetValue(DetailsPanePlacementProperty, value);
        }

        public static readonly DependencyProperty DetailsPanePlacementProperty =
            DependencyProperty.Register(nameof(DetailsPanePlacement), typeof(System.Windows.Controls.Dock), typeof(Config), new PropertyMetadata(System.Windows.Controls.Dock.Right));*/

        //public static Boolean ShowItemCheckboxes { get; set; } = false;

        /*static String _favoritesPath = Environment.ExpandEnvironmentVariables(@"%appdata%\Start9\TempData\" + Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location) + "_Favorites.txt");

        public static ObservableCollection<DiskItem> Favorites
        {
            get
            {
                var items = new ObservableCollection<DiskItem>();
                if (!File.Exists(_favoritesPath))
                {
                    var dir = Path.GetDirectoryName(_favoritesPath);

                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    File.WriteAllLines(_favoritesPath, new List<String>()
                    {
                        @"%userprofile%\Documents",
                        @"%userprofile%\Pictures",
                        @"%userprofile%\Downloads"
                    }.ToArray());
                }

                var lines = File.ReadAllLines(_favoritesPath);
                foreach (var l in lines)
                {
                    if (l.Contains('?'))
                    {
                        var parts = l.Split('?');
                        for (var i = 0; i < parts.Count(); i++)
                        {
                            var s = parts[i];
                        }
                        items.Add(new DiskItem(parts[0])
                        {
                            ItemDisplayName = parts[1]
                        });
                    }
                    else
                        items.Add(new DiskItem(l));
                }
                return items;
            }
            set
            {
                var paths = new List<String>();
                foreach (var d in value)
                {
                    paths.Add(d.ItemPath);
                }
                File.WriteAllLines(_favoritesPath, paths.ToArray());
            }
        }*/

        /*public static ObservableCollection<DiskItem> ComputerSubfolders
        {
            get
            {
                var collection = new ObservableCollection<DiskItem>();
                String[] _userFolders = { @"%userprofile%\Desktop", @"%userprofile%\Documents", @"%userprofile%\Downloads", @"%userprofile%\Music", @"%userprofile%\Pictures", @"%userprofile%\Videos" };
                foreach (var s in _userFolders)
                    collection.Add(new DiskItem(Environment.ExpandEnvironmentVariables(s)));

                foreach (var s in DriveInfo.GetDrives().Where(d => Directory.Exists(d.Name)))
                {
                    collection.Add(new DiskItem(Environment.ExpandEnvironmentVariables(s.Name)));
                }
                return collection;
            }
            set
            {
            }
        }*/

        public static ObservableCollection<DiskItem> ClipboardContents { get; set; } = new ObservableCollection<DiskItem>();
        public static Boolean Cut { get; set; } = false;

        public static ObservableCollection<DiskItem> PasteIn(String targetPath)
        {
            var success = new ObservableCollection<DiskItem>();
            foreach (var d in ClipboardContents)
            {
                var baseFilename = targetPath + @"\" + Path.GetFileName(d.ItemPath);
                var outFilename = baseFilename;

                var interval = 1;

                var ext = Path.GetExtension(baseFilename);
                while (File.Exists(outFilename) || Directory.Exists(outFilename))
                {
                    outFilename = baseFilename + " (" + interval.ToString() + ")";
                    if (!String.IsNullOrEmpty(ext))
                    {
                        if (!outFilename.EndsWith(ext))
                        {
                            if (outFilename.Contains(ext))
                            {
                                outFilename = outFilename.Replace(ext, "");
                                outFilename += ext;
                            }
                        }
                    }
                    interval += 1;
                }

                if (Cut)
                {
                    if (File.Exists(d.ItemPath))
                    {
                        File.Move(d.ItemPath, outFilename);
                    }
                    else
                    {
                        Directory.Move(d.ItemPath, outFilename);
                    }
                }
                else
                {
                    if (File.Exists(d.ItemPath))
                    {
                        File.Copy(d.ItemPath, outFilename);
                    }
                    else
                    {
                        FileSystem.CopyDirectory(d.ItemPath, outFilename, UIOption.AllDialogs);
                    }
                }
                success.Add(new DiskItem(outFilename));
            }
            return success;
        }

        public static SettingsWindow SettingsWindow = new SettingsWindow();

        public static Config Instance = new Config(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Start9", "FileCommander"));

        public static Config Default = new Config();

        private Config()
        {
            Favorites = new ObservableCollection<Location>(){
                new DirectoryQuery(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
                new DirectoryQuery(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)),
                new DirectoryQuery(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"))
            };
            QuickDestinations = new ObservableCollection<Location>()
            {
                new DirectoryQuery(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)), //(@"%userprofile%\Desktop")),
                new DirectoryQuery(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")), //Environment.ExpandEnvironmentVariables(@"%userprofile%\Downloads")),
                new DirectoryQuery(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)), //Environment.ExpandEnvironmentVariables(@"%userprofile%\Documents")),
                new DirectoryQuery(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)), //Environment.ExpandEnvironmentVariables(@"%userprofile%\Music")),
                new DirectoryQuery(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)), //Environment.ExpandEnvironmentVariables(@"%userprofile%\Pictures")),
                new DirectoryQuery(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)), //Environment.ExpandEnvironmentVariables(@"%userprofile%\Videos"))
            };
            ComputerSubFolders = new ObservableCollection<Location>()
            {
                new DirectoryQuery(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
                new DirectoryQuery(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)),
                new DirectoryQuery(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"))
            };

            //var thisPC = new ShellLocation(ShellLocation.ThisPcGuid).GetLocationContents(System.Threading.CancellationToken.None, false);
            //ComputerSubFolders.Add(loc);
        }

        private Config(string configurationPath) : this()
        {
            _configurationPath = configurationPath;
            if (!Directory.Exists(_configurationPath))
                Directory.CreateDirectory(_configurationPath);

            if (TryGetLocationsFromListFile(Path.Combine(_configurationPath, nameof(QuickDestinations)), out ObservableCollection<Location> resValQuickDestinations))
                QuickDestinations = resValQuickDestinations;

            if (TryGetLocationsFromListFile(Path.Combine(_configurationPath, nameof(Favorites)), out ObservableCollection<Location> resValFavorites))
                Favorites = resValFavorites;

            PopulateThisPC();

            string _interfaceModePath = Path.Combine(_configurationPath, nameof(InterfaceMode));
            if (File.Exists(_interfaceModePath) && Enum.TryParse<InterfaceModeType>(File.ReadAllText(_interfaceModePath), out InterfaceModeType resValInterfaceMode))
                InterfaceMode = resValInterfaceMode;

            string _lockPanesPath = Path.Combine(_configurationPath, nameof(LockPanes));
            if (File.Exists(_lockPanesPath) && bool.TryParse(File.ReadAllText(_lockPanesPath), out bool resValLockPanes))
                LockPanes = resValLockPanes;

            string _showEnhancedFolderIconsPath = Path.Combine(_configurationPath, nameof(ShowEnhancedFolderIcons));
            if (File.Exists(_showEnhancedFolderIconsPath) && bool.TryParse(File.ReadAllText(_showEnhancedFolderIconsPath), out bool resValShowEnhancedFolderIcons))
                ShowEnhancedFolderIcons = resValShowEnhancedFolderIcons;

            string _openFoldersInNewWindowPath = Path.Combine(_configurationPath, nameof(OpenFoldersInNewWindow));
            if (File.Exists(_openFoldersInNewWindowPath) && bool.TryParse(File.ReadAllText(_openFoldersInNewWindowPath), out bool resValOpenFoldersInNewWindow))
                OpenFoldersInNewWindow = resValOpenFoldersInNewWindow;

            string _showItemSelectionCheckBoxes = Path.Combine(_configurationPath, nameof(ShowItemSelectionCheckBoxes));
            if (File.Exists(_showItemSelectionCheckBoxes) && bool.TryParse(File.ReadAllText(_showItemSelectionCheckBoxes), out bool resValShowItemSelectionCheckBoxes))
                ShowItemSelectionCheckBoxes = resValShowItemSelectionCheckBoxes;

            string _tabsModePath = Path.Combine(_configurationPath, nameof(TabsMode));
            if (File.Exists(_interfaceModePath) && Enum.TryParse<TabDisplayMode>(File.ReadAllText(_interfaceModePath), out TabDisplayMode resValTabsMode))
                TabsMode = resValTabsMode;

            string _showTitlebarTextPath = Path.Combine(_configurationPath, nameof(ShowTitlebarText));
            if (File.Exists(_showTitlebarTextPath) && bool.TryParse(File.ReadAllText(_showTitlebarTextPath), out bool resValShowTitlebarText))
                ShowTitlebarText = resValShowTitlebarText;

            string _showTitlebarIconPath = Path.Combine(_configurationPath, nameof(ShowTitlebarIcon));
            if (File.Exists(_showTitlebarIconPath) && bool.TryParse(File.ReadAllText(_showTitlebarIconPath), out bool resValShowTitlebarIcon))
                ShowTitlebarIcon = resValShowTitlebarIcon;
        }

        async void PopulateThisPC()
        {
            await foreach (DiskItem item in new ShellLocation(ShellLocation.ThisPcGuid).GetLocationContents(System.Threading.CancellationToken.None, false))
            {
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    ComputerSubFolders.Add(Location.GetLocation(item.ItemPath));
                }));
            }
        }

        bool TryGetLocationsFromListFile(string path, out ObservableCollection<Location> collection)
        {
            if (File.Exists(path))
            {
                collection = new ObservableCollection<Location>();
                foreach (string item in File.ReadAllLines(path))
                    collection.Add(Location.GetLocation(path));

                return true;
            }
            else
            {
                collection = null;
                return false;
            }
        }

        static void OnSinglePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is Config inst) && (!string.IsNullOrWhiteSpace(inst._configurationPath)))
                File.WriteAllText(Path.Combine(inst._configurationPath, e.Property.Name), e.NewValue.ToString());
        }

        static void OnCollectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is Config inst) && (!string.IsNullOrWhiteSpace(inst._configurationPath)) && (e.NewValue is System.Collections.IList coll))
            {
                string[] outFileContents = new string[coll.Count];
                for (int i = 0; i < coll.Count; i++)
                    outFileContents[i] = coll[i].ToString();

                File.WriteAllLines(Path.Combine(inst._configurationPath, e.Property.Name), outFileContents);
            }
        }

        internal static void InvokeConfigUpdated()
        {
            ConfigUpdated?.Invoke(Instance, null);
        }

        public static event EventHandler ConfigUpdated;
    }
}
