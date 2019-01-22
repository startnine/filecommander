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


        private void SettingsWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((bool)e.OldValue == false) && ((bool)e.NewValue == true))
            {
                if (Config.Instance.InterfaceMode == Config.InterfaceModeType.CommandBar)
                    InterfaceModeComboBox.SelectedIndex = 0;
                else
                    InterfaceModeComboBox.SelectedIndex = 1;

                StatusBarToggleSwitch.IsChecked = Config.Instance.ShowStatusBar;
                TitlebarTextToggleSwitch.IsChecked = Config.Instance.ShowTitlebarText;
            }
        }

        private void Start9SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (InterfaceModeComboBox.SelectedIndex == 0)
                Config.Instance.InterfaceMode = Config.InterfaceModeType.CommandBar;
            else
                Config.Instance.InterfaceMode = Config.InterfaceModeType.Ribbon;

            if (StatusBarToggleSwitch.IsChecked.Value)
                Config.Instance.ShowStatusBar = true;
            else
                Config.Instance.ShowStatusBar = false;

            if (TitlebarTextToggleSwitch.IsChecked.Value)
                Config.Instance.ShowTitlebarText = true;
            else
                Config.Instance.ShowTitlebarText = false;

            if (sender == OkButton)
                Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
