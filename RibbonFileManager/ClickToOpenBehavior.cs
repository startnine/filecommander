using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace RibbonFileManager
{
    public class ClickToOpenBehavior : Behavior<Panel>
    {
        public ListViewItem ParentListViewItem
        {
            get => (ListViewItem)GetValue(ParentListViewItemProperty);
            set => SetValue(ParentListViewItemProperty, value);
        }

        public static readonly DependencyProperty ParentListViewItemProperty =
            DependencyProperty.Register("ParentListViewItem", typeof(ListViewItem), typeof(ClickToOpenBehavior), new PropertyMetadata(null, OnParentListViewItemChangedCallback));

        static void OnParentListViewItemChangedCallback(System.Object sender, DependencyPropertyChangedEventArgs e)
        {
            var sned = sender as ClickToOpenBehavior;
            if (e.NewValue != null)
                sned.ParentListViewItem.PreviewMouseDoubleClick += sned.ParentListViewItem_PreviewMouseDoubleClick;

            if (e.OldValue != null)
                (e.OldValue as ListViewItem).PreviewMouseDoubleClick -= sned.ParentListViewItem_PreviewMouseDoubleClick;
        }

        public WindowContent ManagerBase
        {
            get => (WindowContent)GetValue(ManagerBaseProperty);
            set => SetValue(ManagerBaseProperty, value);
        }

        public static readonly DependencyProperty ManagerBaseProperty =
            DependencyProperty.Register("ManagerBase", typeof(WindowContent), typeof(ClickToOpenBehavior), new PropertyMetadata(null));


        private async void ParentListViewItem_PreviewMouseDoubleClick(System.Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            await ManagerBase.OpenSelectionAsync();
        }
    }
}
