﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RibbonFileManager.Shale
{
    /// <summary>
    /// Interaction logic for ShaleSettingsPage.xaml
    /// </summary>
    public partial class ShaleSettingsPage : Page
    {
        ShaleSkinInfo _shaleSkinInfo = null;
        public ShaleSettingsPage(ShaleSkinInfo shaleSkinInfo)
        {
            _shaleSkinInfo = shaleSkinInfo;
            InitializeComponent();
        }

        private void LightsToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            _shaleSkinInfo.UseLightTheme = true;
        }

        private void LightsToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            _shaleSkinInfo.UseLightTheme = false;
        }
    }
}
