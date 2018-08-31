using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace RibbonFileManager
{
    [TemplatePart(Name = PartBackButton, Type = typeof(Button))]
    [TemplatePart(Name = PartForwardButton, Type = typeof(Button))]
    [TemplatePart(Name = PartHistoryToggleButton, Type = typeof(ToggleButton))]

    [TemplatePart(Name = PartHistoryPopup, Type = typeof(Popup))]
    [TemplatePart(Name = PartHistoryListView, Type = typeof(ListView))]

    [TemplatePart(Name = PartBreadcrumbsStackPanel, Type = typeof(StackPanel))]
    [TemplatePart(Name = PartPathTextBox, Type = typeof(TextBox))]

    [TemplatePart(Name = PartSearchTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PartSearchPredictionPopup, Type = typeof(Popup))]
    [TemplatePart(Name = PartSearchToggleButton, Type = typeof(ToggleButton))]
    public class NavigationBar : Control
    {
        const String PartBackButton = "PART_BackButton";
        const String PartForwardButton = "PART_ForwardButton";
        const String PartHistoryToggleButton = "PART_HistoryToggleButton";

        const String PartHistoryPopup = "PART_HistoryPopup";
        const String PartHistoryListView = "PART_PartHistoryListView";

        const String PartBreadcrumbsStackPanel = "PART_BreadcrumbsStackPanel";
        const String PartPathTextBox = "PART_PathTextBox";

        const String PartSearchTextBox = "PART_SearchTextBox";
        const String PartSearchPredictionPopup = "PART_SearchPredictionPopup";
        const String PartSearchToggleButton = "PART_SearchToggleButton";


        public Button BackButton;
        public Button ForwardButton;
        public ToggleButton HistoryToggleButton;

        public Popup HistoryPopup;
        public ListView HistoryListView;

        public StackPanel BreadcrumbsStackPanel;
        public TextBox PathTextBox;

        public TextBox SearchTextBox;
        public Popup SearchPredictionPopup;
        public Button SearchToggleButton;

        public bool IsBackButtonEnabled
        {
            get => (bool)GetValue(IsBackButtonEnabledProperty);
            set => SetValue(IsBackButtonEnabledProperty, value);
        }

        public static readonly DependencyProperty IsBackButtonEnabledProperty =
            DependencyProperty.Register("IsBackButtonEnabled", typeof(bool), typeof(NavigationBar), new PropertyMetadata(true));

        public bool IsForwardButtonEnabled
        {
            get => (bool)GetValue(IsForwardButtonEnabledProperty);
            set => SetValue(IsForwardButtonEnabledProperty, value);
        }

        public static readonly DependencyProperty IsForwardButtonEnabledProperty =
            DependencyProperty.Register("IsForwardButtonEnabled", typeof(bool), typeof(NavigationBar), new PropertyMetadata(true));

        public NavigationBar()
        {

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            BackButton = GetTemplateChild(PartBackButton) as Button;
            BackButton.Click += (sneder, args) =>
            {
                BackButtonClick(BackButton, args);
            };

            ForwardButton = GetTemplateChild(PartForwardButton) as Button;
            ForwardButton.Click += (sneder, args) =>
            {
                ForwardButtonClick(ForwardButton, args);
            };

            HistoryToggleButton = GetTemplateChild(PartHistoryToggleButton) as ToggleButton;
            /*HistoryToggleButton.Click += (sneder, args) =>
            {
                HistoryToggleButtonClick(HistoryToggleButton, args);
            };*/

            HistoryPopup = GetTemplateChild(PartHistoryPopup) as Popup;
            HistoryListView = GetTemplateChild(PartHistoryListView) as ListView;

            BreadcrumbsStackPanel = GetTemplateChild(PartBreadcrumbsStackPanel) as StackPanel;
            PathTextBox = GetTemplateChild(PartPathTextBox) as TextBox;
            PathTextBox.PreviewMouseLeftButtonDown += (sneder, args) =>
            {
                PathTextBoxPreviewMouseLeftButtonDown(PathTextBox, args);
            };
            PathTextBox.KeyDown += (sneder, args) =>
            {
                PathTextBoxKeyDown(PathTextBox, args);
            };
            PathTextBox.LostFocus += (sneder, args) =>
            {
                PathTextBoxLostFocus(PathTextBox, args);
            };

            SearchTextBox = GetTemplateChild(PartSearchTextBox) as TextBox;
            SearchPredictionPopup = GetTemplateChild(PartSearchPredictionPopup) as Popup;
            SearchToggleButton = GetTemplateChild(PartSearchToggleButton) as Button;
        }

        public event RoutedEventHandler BackButtonClick;
        public event RoutedEventHandler ForwardButtonClick;
        public event RoutedEventHandler HistoryToggleButtonClick;
        public event MouseButtonEventHandler PathTextBoxPreviewMouseLeftButtonDown;
        public event KeyEventHandler PathTextBoxKeyDown;
        public event RoutedEventHandler PathTextBoxLostFocus;
    }
}
