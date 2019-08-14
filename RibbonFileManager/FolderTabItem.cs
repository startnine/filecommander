using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;

namespace RibbonFileManager
{
    public class FolderTabItem : DependencyObject
    {
        public WindowContent Content
        {
            get => (WindowContent)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(WindowContent), typeof(FolderTabItem), new FrameworkPropertyMetadata(null));

        public string Name
        {
            get => (string)GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register(nameof(Name), typeof(string), typeof(FolderTabItem), new FrameworkPropertyMetadata(string.Empty));

        public Icon Icon
        {
            get => (Icon)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(Icon), typeof(FolderTabItem), new FrameworkPropertyMetadata(null));


        public bool HasSpecialIcon
        {
            get => (bool)GetValue(HasSpecialIconProperty);
            set => SetValue(HasSpecialIconProperty, value);
        }

        public static readonly DependencyProperty HasSpecialIconProperty =
            DependencyProperty.Register(nameof(HasSpecialIcon), typeof(bool), typeof(FolderTabItem), new FrameworkPropertyMetadata(false));


        public UIElement SpecialIcon
        {
            get => (UIElement)GetValue(SpecialIconProperty);
            set => SetValue(SpecialIconProperty, value);
        }

        public static readonly DependencyProperty SpecialIconProperty =
            DependencyProperty.Register(nameof(SpecialIcon), typeof(UIElement), typeof(FolderTabItem), new FrameworkPropertyMetadata(null));

        /*public UIElement SpecialIcon
        {
            get
            {
                if (HasSpecialIcon)
                    return (Content.CurrentLocation as DirectoryQuery).Item.SpecialIcon;
                else
                    return null;
            }
        }*/

        public FolderTabItem(Location defaultlocation)
        {
            Content = new WindowContent(defaultlocation);
            Content.NavManager.Navigated += (sneder, args) =>
            {
                Name = Content.CurrentLocation.Name;
                Icon = Content.CurrentLocation.Icon;
                if (Content.CurrentLocation is DirectoryQuery query)
                    HasSpecialIcon = query.Item.HasSpecialIcon;
                else
                    HasSpecialIcon = false;

                if (HasSpecialIcon)
                    SpecialIcon = (Content.CurrentLocation as DirectoryQuery).Item.SpecialIcon;
                else
                    SpecialIcon = null;
            };
        }
    }
}
