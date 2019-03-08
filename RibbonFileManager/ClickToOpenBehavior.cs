using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace RibbonFileManager
{
    public class ClickToOpenBehavior : Behavior<Panel>
    {
        /*public DiskItem TargetItem
        {
            get => (DiskItem)GetValue(TargetItemProperty);
            set => SetValue(TargetItemProperty, value);
        }

        public static readonly DependencyProperty TargetItemProperty =
            DependencyProperty.Register("TargetItem", typeof(DiskItem), typeof(ClickToOpenBehavior), new PropertyMetadata(null));*/

        public ListViewItem ParentListViewItem
        {
            get => (ListViewItem)GetValue(ParentListViewItemProperty);
            set => SetValue(ParentListViewItemProperty, value);
        }

        public static readonly DependencyProperty ParentListViewItemProperty =
            DependencyProperty.Register("ParentListViewItem", typeof(ListViewItem), typeof(ClickToOpenBehavior), new PropertyMetadata(null, OnParentListViewItemChangedCallback));

        static void OnParentListViewItemChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            var sned = sender as ClickToOpenBehavior;
            //Debug.WriteLine("(e.NewValue != null): " + e.NewValue != null);
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

        //ListViewItem _item;
        /*protected override void OnAttached()
        {
            base.OnAttached();
        }*/

        private void ParentListViewItem_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Debug.WriteLine("DOUBLE CLICK");
            ManagerBase.OpenSelection();
        }
    }
}
