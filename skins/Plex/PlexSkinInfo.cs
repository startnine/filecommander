using Start9.UI.Wpf.Skinning;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Plex
{
    public class PlexSkinInfo : ISkinInfo
    {
        public string SkinName => "Plex";

        public bool GetHaveSettingsChanged()
        {
            return false;
        }

        public Page GetSettingsPage()
        {
            return new Page();
        }

        public ResourceDictionary OnApplySkinSettings()
        {
            ResourceDictionary dictionary = new ResourceDictionary()
            {
                Source = new Uri("/Plex;component/Plex.xaml", UriKind.Relative)
            };

            return dictionary;
        }
    }
}
