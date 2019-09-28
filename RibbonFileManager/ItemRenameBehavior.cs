using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace RibbonFileManager
{
    public class ItemRenameBehavior : Behavior<TextBox>
    {
        TextBox _box = null;

        public DiskItem TargetItem
        {
            get => (DiskItem)GetValue(TargetItemProperty);
            set => SetValue(TargetItemProperty, value);
        }

        public static readonly DependencyProperty TargetItemProperty =
            DependencyProperty.Register("TargetItem", typeof(DiskItem), typeof(ItemRenameBehavior), new PropertyMetadata(null));

        public WindowContent WindowContent
        {
            get => (WindowContent)GetValue(WindowContentProperty);
            set => SetValue(WindowContentProperty, value);
        }

        public static readonly DependencyProperty WindowContentProperty =
            DependencyProperty.Register(nameof(WindowContent), typeof(WindowContent), typeof(ItemRenameBehavior), new PropertyMetadata(null));

        public Boolean IsRenaming
        {
            get => (Boolean) GetValue(IsRenamingProperty);
            set => SetValue(IsRenamingProperty, value);
        }

        public static readonly DependencyProperty IsRenamingProperty = DependencyProperty.Register("IsRenaming", typeof(Boolean), typeof(ItemRenameBehavior), new PropertyMetadata(false, OnIsRenamingChangedCallback));

        static void OnIsRenamingChangedCallback(Object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue)
            {
                //var sned = sender as ItemRenameBehavior;
                (sender as ItemRenameBehavior)._box.Visibility = Visibility.Visible;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            _box = AssociatedObject as TextBox;
            _box.IsVisibleChanged += TextBox_IsVisibleChanged;
            _box.KeyDown += TextBox_KeyDown;
        }

        private void TextBox_IsVisibleChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_box.IsVisible)
            {
                _box.Text = TargetItem.ItemRealName;
                _box.Focus();
                _box.SelectAll();
            }
            else
                TargetItem.IsRenaming = false;
        }

        private void TextBox_KeyDown(Object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (TargetItem != null)
                {
                    TargetItem.RenameItem(_box.Text);
                    _box.Visibility = Visibility.Collapsed;
                }
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
                _box.Visibility = Visibility.Collapsed;
        }
    }
}
