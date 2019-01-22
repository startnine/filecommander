using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace RibbonFileManager
{
    [TemplatePart(Name = PartGoStopToggleButton, Type = typeof(ToggleButton))]
    [TemplatePart(Name = PartWatermarkTextBlock, Type = typeof(TextBlock))]
    public class SearchBox : TextBox
    {
        const string PartGoStopToggleButton = "PART_GoStopToggleButton";
        const string PartWatermarkTextBlock = "PART_WatermarkTextBlock";

        public string WatermarkText
        {
            get => (string)GetValue(WatermarkTextProperty);
            set => SetValue(WatermarkTextProperty, value);
        }

        public static readonly DependencyProperty WatermarkTextProperty =
            DependencyProperty.Register("WatermarkText", typeof(string), typeof(SearchBox), new PropertyMetadata(string.Empty));

        public SearchBox()
        {

        }

        ToggleButton _goStopToggleButton;
        TextBlock _watermarkTextBlock;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _goStopToggleButton = GetTemplateChild(PartGoStopToggleButton) as ToggleButton;
            if (_goStopToggleButton != null)
                _goStopToggleButton.Click += GoStopToggleButton_Click;

            _watermarkTextBlock = GetTemplateChild(PartWatermarkTextBlock) as TextBlock;
        }

        private void GoStopToggleButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (_watermarkTextBlock != null)
            {
                if (string.IsNullOrWhiteSpace(Text))
                    _watermarkTextBlock.Visibility = Visibility.Visible;
                else
                    _watermarkTextBlock.Visibility = Visibility.Collapsed;
            }
        }
    }
}
