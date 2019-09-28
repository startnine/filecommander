using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
//using WindowsSharp.DiskItems;

namespace RibbonFileManager
{
    public class DiskItemDragBehavior : Behavior<Panel>
    {
        public DiskItem TargetItem
        {
            get => (DiskItem)GetValue(TargetItemProperty);
            set => SetValue(TargetItemProperty, value);
        }

        public static readonly DependencyProperty TargetItemProperty =
            DependencyProperty.Register("TargetItem", typeof(DiskItem), typeof(DiskItemDragBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                (AssociatedObject as Panel).MouseMove += TargetPanel_MouseMove;
                (AssociatedObject as Panel).DragEnter += DiskItemDragBehavior_DragEnter;
            }
        }

        private void DiskItemDragBehavior_DragEnter(object sender, DragEventArgs e)
        {
            var data = new DataObject(DataFormats.FileDrop, new string[] { TargetItem.ItemPath });
            /*data.SetData(DataFormats.FileDrop, new System.Collections.Specialized.StringCollection()
            {
                TargetItem.ItemPath
            });*/
            DragDrop.DoDragDrop(sender as Panel, data, DragDropEffects.Copy | DragDropEffects.Move);
        }

        Point _prevPoint = new Point(0, 0);
        private void TargetPanel_MouseMove(Object sender, System.Windows.Input.MouseEventArgs e)
        {
            if ((e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) && (_prevPoint != null) && ((e.GetPosition(sender as Panel).X != _prevPoint.X) | (e.GetPosition(sender as Panel).Y != _prevPoint.Y)) && (TargetItem != null))
            {
                var data = new DataObject(DataFormats.FileDrop, new string[] { TargetItem.ItemPath });
                /*data.SetData(DataFormats.FileDrop, new System.Collections.Specialized.StringCollection()
                {
                    TargetItem.ItemPath
                });*/
                DragDrop.DoDragDrop(sender as Panel, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
            _prevPoint = e.GetPosition(sender as Panel);
        }
    }
}
