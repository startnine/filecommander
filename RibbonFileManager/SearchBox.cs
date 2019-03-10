using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace RibbonFileManager
{
    [TemplatePart(Name = PartGoStopToggleButton, Type = typeof(ToggleButton))]
    [TemplatePart(Name = PartWatermarkTextBlock, Type = typeof(TextBlock))]
    public class SearchBox : TextBox
    {
        public const String PartGoStopToggleButton = "PART_GoStopToggleButton";
        public const String PartWatermarkTextBlock = "PART_WatermarkTextBlock";

        public String WatermarkText
        {
            get => (String) GetValue(WatermarkTextProperty);
            set => SetValue(WatermarkTextProperty, value);
        }

        public static readonly DependencyProperty WatermarkTextProperty =
            DependencyProperty.Register(nameof(WatermarkText), typeof(String), typeof(SearchBox), new PropertyMetadata(String.Empty));

        public SearchType SearchType
        {
            get => (SearchType) GetValue(SearchTypeProperty);
            set => SetValue(SearchTypeProperty, value);
        }

        public static readonly DependencyProperty SearchTypeProperty =
            DependencyProperty.Register(nameof(SearchType), typeof(SearchType), typeof(SearchBox), new PropertyMetadata(SearchType.Regular));

        public void CancelSearch()
        {
            SearchButton.IsChecked = false;
        }
        
        public static readonly RoutedEvent SearchSubmittedEvent = EventManager.RegisterRoutedEvent(
            nameof(SearchSubmitted), RoutingStrategy.Bubble, typeof(EventHandler<SearchSubmittedEventArgs>), typeof(SearchBox));

        public event EventHandler<SearchSubmittedEventArgs> SearchSubmitted
        {
            add { AddHandler(SearchSubmittedEvent, value); }
            remove { RemoveHandler(SearchSubmittedEvent, value); }
        }

        public static readonly RoutedEvent SearchCanceledEvent = EventManager.RegisterRoutedEvent(
            nameof(SearchCanceled), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SearchBox));

        public event RoutedEventHandler SearchCanceled
        {
            add { AddHandler(SearchCanceledEvent, value); }
            remove { RemoveHandler(SearchCanceledEvent, value); }
        }


        internal ToggleButton SearchButton;
        TextBlock _watermarkTextBlock;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SearchButton = GetTemplateChild(PartGoStopToggleButton) as ToggleButton;
            if (SearchButton != null)
                SearchButton.Click += GoStopToggleButton_Click;

            _watermarkTextBlock = GetTemplateChild(PartWatermarkTextBlock) as TextBlock;
        }

        private void GoStopToggleButton_Click(Object sender, RoutedEventArgs e)
        {
            if (SearchButton.IsChecked == true)
                RaiseEvent(new SearchSubmittedEventArgs(Text, SearchSubmittedEvent));
            else
                RaiseEvent(new RoutedEventArgs(SearchCanceledEvent));
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (_watermarkTextBlock != null)
            {
                _watermarkTextBlock.Visibility = String.IsNullOrWhiteSpace(Text) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (SearchType == SearchType.Instant)
            {
                SearchButton.IsChecked = !String.IsNullOrWhiteSpace(Text);
                RaiseEvent(new SearchSubmittedEventArgs(Text, SearchCanceledEvent));
                RaiseEvent(new SearchSubmittedEventArgs(Text, SearchSubmittedEvent));
            }
        }
    }

    public enum SearchType
    {
        Regular,
        Instant
    }

    public class SearchSubmittedEventArgs : RoutedEventArgs
    {
        internal SearchSubmittedEventArgs(String text, RoutedEvent @event) : base(@event)
        {
            Query = text;
        }

        public String Query { get; }

        public void CancelSearch()
        {
            ((SearchBox) Source).SearchButton.IsChecked = false;
        }
    }
}
