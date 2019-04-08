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

        public WindowContent WindowContent
        {
            get => (WindowContent)GetValue(WindowContentProperty);
            set => SetValue(WindowContentProperty, value);
        }

        public static readonly DependencyProperty WindowContentProperty =
            DependencyProperty.Register(nameof(WindowContent), typeof(WindowContent), typeof(ClickToOpenBehavior), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));


        private async void ParentListViewItem_PreviewMouseDoubleClick(System.Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            await WindowContent.OpenSelectionAsync();
        }
    }
}
