using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Start9.UI.Wpf.Windows;

namespace RibbonFileManager
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : DecoratableWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            base.OnClosing(e);
        }


        private void SettingsWindow_IsVisibleChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((Boolean) e.OldValue == false) && ((Boolean) e.NewValue == true))
            {
                //InterfaceModeComboBox.SelectedIndex = Config.Instance.InterfaceMode == Config.InterfaceModeType.CommandBar ? 0 : 1;
                if (Config.Instance.InterfaceMode == Config.InterfaceModeType.Ribbon)
                    InterfaceModeComboBox.SelectedIndex = 0;
                else if (Config.Instance.InterfaceMode == Config.InterfaceModeType.CommandBar)
                    InterfaceModeComboBox.SelectedIndex = 1;
                else
                    InterfaceModeComboBox.SelectedIndex = 2;

                StatusBarToggleSwitch.IsChecked = Config.Instance.ShowStatusBar;
                TitlebarTextToggleSwitch.IsChecked = Config.Instance.ShowTitlebarText;
            }
        }

        private void Start9SettingsButton_Click(Object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ApplyButton_Click(Object sender, RoutedEventArgs e)
        {
            //Config.Instance.InterfaceMode = InterfaceModeComboBox.SelectedIndex == 0 ? Config.InterfaceModeType.CommandBar : Config.InterfaceModeType.Ribbon;
            if (InterfaceModeComboBox.SelectedIndex == 0)
                Config.Instance.InterfaceMode = Config.InterfaceModeType.Ribbon;
            else if (InterfaceModeComboBox.SelectedIndex == 1)
                Config.Instance.InterfaceMode = Config.InterfaceModeType.CommandBar;
            else
                Config.Instance.InterfaceMode = Config.InterfaceModeType.None;

            Config.Instance.ShowStatusBar = StatusBarToggleSwitch.IsChecked.Value;

            Config.Instance.ShowTitlebarText = TitlebarTextToggleSwitch.IsChecked.Value;

            if (sender == OkButton)
                Close();
        }

        private void CancelButton_Click(Object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
