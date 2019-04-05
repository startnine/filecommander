using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace RibbonFileManager
{
    public class ItemContextMenuBehavior : Behavior<ContextMenu>
    {
        public DiskItem TargetItem
        {
            get => (DiskItem)GetValue(TargetItemProperty);
            set => SetValue(TargetItemProperty, value);
        }

        public static readonly DependencyProperty TargetItemProperty =
            DependencyProperty.Register("TargetItem", typeof(DiskItem), typeof(ItemContextMenuBehavior), new PropertyMetadata(null, OnTargetItemChangedCallback));

        static void OnTargetItemChangedCallback(System.Object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var sned = sender as ItemContextMenuBehavior;
                sned.RunAsAdminMenuItem.Visibility = (e.NewValue as DiskItem).ItemCategory != DiskItemCategory.File ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public WindowContent ManagerBase
        {
            get => (WindowContent)GetValue(ManagerBaseProperty);
            set => SetValue(ManagerBaseProperty, value);
        }

        public static readonly DependencyProperty ManagerBaseProperty =
            DependencyProperty.Register("ManagerBase", typeof(WindowContent), typeof(ItemContextMenuBehavior), new PropertyMetadata(null));

        public MenuItem OpenMenuItem
        {
            get => (MenuItem)GetValue(OpenMenuItemProperty);
            set => SetValue(OpenMenuItemProperty, value);
        }

        public static readonly DependencyProperty OpenMenuItemProperty =
            DependencyProperty.Register("OpenMenuItem", typeof(MenuItem), typeof(ItemContextMenuBehavior), new PropertyMetadata(null, OnOpenMenuItemChangedCallback));

        static void OnOpenMenuItemChangedCallback(System.Object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (e.NewValue as MenuItem).Click += (sender as ItemContextMenuBehavior).OpenMenuItem_Click;
        }

        private void OpenMenuItem_Click(System.Object sender, RoutedEventArgs e)
        {
            if (TargetItem != null)
                ManagerBase.OpenPath(TargetItem.ItemPath);
        }

        public MenuItem RunAsAdminMenuItem
        {
            get => (MenuItem)GetValue(RunAsAdminMenuItemProperty);
            set => SetValue(RunAsAdminMenuItemProperty, value);
        }

        public static readonly DependencyProperty RunAsAdminMenuItemProperty =
            DependencyProperty.Register("RunAsAdminMenuItem", typeof(MenuItem), typeof(ItemContextMenuBehavior), new PropertyMetadata(null, OnRunAsAdminMenuItemChangedCallback));

        static void OnRunAsAdminMenuItemChangedCallback(System.Object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (e.NewValue as MenuItem).Click += (sender as ItemContextMenuBehavior).RunAsAdminMenuItem_Click;
        }

        private void RunAsAdminMenuItem_Click(System.Object sender, RoutedEventArgs e)
        {

        }

        public MenuItem CopyMenuItem
        {
            get => (MenuItem)GetValue(CopyMenuItemProperty);
            set => SetValue(CopyMenuItemProperty, value);
        }

        public static readonly DependencyProperty CopyMenuItemProperty =
            DependencyProperty.Register("CopyMenuItem", typeof(MenuItem), typeof(ItemContextMenuBehavior), new PropertyMetadata(null, OnCopyMenuItemChangedCallback));

        static void OnCopyMenuItemChangedCallback(System.Object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (e.NewValue as MenuItem).Click += (sender as ItemContextMenuBehavior).CopyMenuItem_Click;
        }

        private void CopyMenuItem_Click(System.Object sender, RoutedEventArgs e)
        {
            ManagerBase.CopySelection();
        }

        public MenuItem CutMenuItem
        {
            get => (MenuItem)GetValue(CutMenuItemProperty);
            set => SetValue(CutMenuItemProperty, value);
        }

        public static readonly DependencyProperty CutMenuItemProperty =
            DependencyProperty.Register("CutMenuItem", typeof(MenuItem), typeof(ItemContextMenuBehavior), new PropertyMetadata(null, OnCutMenuItemChangedCallback));

        static void OnCutMenuItemChangedCallback(System.Object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (e.NewValue as MenuItem).Click += (sender as ItemContextMenuBehavior).CutMenuItem_Click;
        }

        private void CutMenuItem_Click(System.Object sender, RoutedEventArgs e)
        {
            ManagerBase.CutSelection();
        }

        public MenuItem DeleteMenuItem
        {
            get => (MenuItem)GetValue(DeleteMenuItemProperty);
            set => SetValue(DeleteMenuItemProperty, value);
        }

        public static readonly DependencyProperty DeleteMenuItemProperty =
            DependencyProperty.Register("DeleteMenuItem", typeof(MenuItem), typeof(ItemContextMenuBehavior), new PropertyMetadata(null, OnDeleteMenuItemChangedCallback));

        static void OnDeleteMenuItemChangedCallback(System.Object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (e.NewValue as MenuItem).Click += (sender as ItemContextMenuBehavior).DeleteMenuItem_Click;
        }

        private async void DeleteMenuItem_Click(System.Object sender, RoutedEventArgs e)
        {
            await ManagerBase.DeleteSelectionAsync();
        }

        public MenuItem RenameMenuItem
        {
            get => (MenuItem)GetValue(RenameMenuItemProperty);
            set => SetValue(RenameMenuItemProperty, value);
        }

        public static readonly DependencyProperty RenameMenuItemProperty =
            DependencyProperty.Register("RenameMenuItem", typeof(MenuItem), typeof(ItemContextMenuBehavior), new PropertyMetadata(null, OnRenameMenuItemChangedCallback));

        static void OnRenameMenuItemChangedCallback(System.Object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (e.NewValue as MenuItem).Click += (sender as ItemContextMenuBehavior).RenameMenuItem_Click;
        }

        public TextBox RenameTextBox
        {
            get => (TextBox)GetValue(RenameTextBoxProperty);
            set => SetValue(RenameTextBoxProperty, value);
        }

        public static readonly DependencyProperty RenameTextBoxProperty =
            DependencyProperty.Register("RenameTextBox", typeof(TextBox), typeof(ItemContextMenuBehavior), new PropertyMetadata(null));

        private void RenameMenuItem_Click(System.Object sender, RoutedEventArgs e)
        {
            if (RenameTextBox != null)
                RenameTextBox.Visibility = Visibility.Visible;
        }

        public MenuItem PropertiesMenuItem
        {
            get => (MenuItem)GetValue(PropertiesMenuItemProperty);
            set => SetValue(PropertiesMenuItemProperty, value);
        }

        public static readonly DependencyProperty PropertiesMenuItemProperty =
            DependencyProperty.Register("PropertiesMenuItem", typeof(MenuItem), typeof(ItemContextMenuBehavior), new PropertyMetadata(null, OnPropertiesMenuItemChangedCallback));

        static void OnPropertiesMenuItemChangedCallback(System.Object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                (e.NewValue as MenuItem).Click += (sender as ItemContextMenuBehavior).PropertiesMenuItem_Click;
        }

        private void PropertiesMenuItem_Click(System.Object sender, RoutedEventArgs e)
        {

       }

        ContextMenu _menu;

        protected override void OnAttached()
        {
            base.OnAttached();
            _menu = AssociatedObject as ContextMenu;
        }
    }
}
