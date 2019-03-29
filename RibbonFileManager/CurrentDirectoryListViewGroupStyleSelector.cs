using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace RibbonFileManager
{
    public class CurrentDirectoryListViewGroupStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            return (Style)Application.Current.Resources["CurrentDirectoryListViewHeaderTemplate"];
        }
    }
}
