using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using WindowsSharp.DiskItems;

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
            }
        }

        Point _prevPoint = new Point(0, 0);
        private void TargetPanel_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if ((e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) && (_prevPoint != null) && ((e.GetPosition(sender as Panel).X != _prevPoint.X) | (e.GetPosition(sender as Panel).Y != _prevPoint.Y)) && (TargetItem != null))
            {
                DataObject data = new DataObject();
                data.SetData(DataFormats.FileDrop, new System.Collections.Specialized.StringCollection()
                {
                    TargetItem.ItemPath
                });
                DragDrop.DoDragDrop(sender as Panel, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
            _prevPoint = e.GetPosition(sender as Panel);
        }
    }
}
