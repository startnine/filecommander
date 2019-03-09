using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
//using WindowsSharp.DiskItems;

namespace RibbonFileManager
{
    public class ItemRenameBehavior : Behavior<TextBox>
    {
        public DiskItem TargetItem
        {
            get => (DiskItem)GetValue(TargetItemProperty);
            set => SetValue(TargetItemProperty, value);
        }

        public static readonly DependencyProperty TargetItemProperty =
            DependencyProperty.Register("TargetItem", typeof(DiskItem), typeof(ItemRenameBehavior), new PropertyMetadata(null));

        public WindowContent ManagerBase
        {
            get => (WindowContent)GetValue(ManagerBaseProperty);
            set => SetValue(ManagerBaseProperty, value);
        }

        public static readonly DependencyProperty ManagerBaseProperty =
            DependencyProperty.Register("ManagerBase", typeof(WindowContent), typeof(ItemRenameBehavior), new PropertyMetadata(null));

        public bool IsRenaming
        {
            get => (bool)GetValue(IsRenamingProperty);
            set => SetValue(IsRenamingProperty, value);
        }

        public static readonly DependencyProperty IsRenamingProperty = DependencyProperty.Register("IsRenaming", typeof(bool), typeof(ItemRenameBehavior), new PropertyMetadata(false, OnIsRenamingChangedCallback));

        static void OnIsRenamingChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)(e.NewValue))
            {
                var sned = sender as ItemRenameBehavior;

                /*if ((sned.TargetItem != null) && (sned.ManagerBase.CurrentDirectoryListView.SelectedItem == sned.TargetItem)) //(sned.ManagerBase.CurrentDirectoryListView.SelectedItems.Contains(sned.TargetItem))
                    sned._box.Visibility = Visibility.Visible;*/
            }
        }

        TextBox _box;

        protected override void OnAttached()
        {
            base.OnAttached();
            _box = AssociatedObject as TextBox;
            _box.IsVisibleChanged += TextBox_IsVisibleChanged;
            _box.KeyDown += TextBox_KeyDown;
        }

        private void TextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_box.IsVisible)
            {
                _box.Text = TargetItem.ItemRealName;
                _box.Focus();
                _box.SelectAll();
            }
            else
                ManagerBase.IsRenamingFiles = false;
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (TargetItem != null) //((ManagerBase != null) && (TargetItem != null))
                {
                    /*bool? outVal = */
                    TargetItem.RenameItem(_box.Text);
                    _box.Visibility = Visibility.Collapsed;
                }
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
                _box.Visibility = Visibility.Collapsed;
        }
    }
}
