using Start9.UI.Wpf.Skinning;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace RibbonFileManager.Shale
{
    public class ShaleSkin : ISkinInfo
    {
        ShaleSettingsPage _page = null;

        public Page GetSettingsPage()
        {
            if (_page == null)
                _page = new ShaleSettingsPage(this);

            return _page;
        }

        public ResourceDictionary OnApplySkinSettings()
        {
            ResourceDictionary dictionary = new ResourceDictionary()
            {
                Source = new Uri("/RibbonFileManager;component/Shale/Shale.xaml", UriKind.Relative)
            };

            if (UseLightTheme)
                dictionary.MergedDictionaries[5].Source = new Uri("/Start9.Wpf.Styles.Shale;component/Themes/Colors/BaseLight.xaml", UriKind.Relative);
            else
                dictionary.MergedDictionaries[5].Source = new Uri("/Start9.Wpf.Styles.Shale;component/Themes/Colors/BaseDark.xaml", UriKind.Relative);

            dictionary.MergedDictionaries[4] = Start9.Wpf.Styles.Shale.ShaleAccents.Blue.Dictionary;

            return dictionary;
        }

        public bool UseLightTheme = true;

        string ISkinInfo.SkinName
        {
            get => "Shale";
        }
    }
}
