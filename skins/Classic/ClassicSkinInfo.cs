using RibbonFileManager;
using Start9.UI.Wpf.Skinning;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Classic
{
    public class ClassicSkinInfo : ISkinInfo
    {
        public Page GetSettingsPage()
        {
            return new Page();
        }

        public ResourceDictionary OnApplySkinSettings()
        {
            ResourceDictionary dictionary = new ResourceDictionary()
            {
                Source = new Uri("/Skin;component/Classic.xaml", UriKind.Relative)
            };

            /*dictionary.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("/PresentationFramework.Classic, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL;component/themes/classic.xaml", UriKind.Relative)
            });
            dictionary.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("/Start9.UI.Wpf;component/Themes/Generic.xaml", UriKind.Relative)
            });*/

            return dictionary;
        }
    }
}
