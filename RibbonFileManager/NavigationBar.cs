using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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

        public NavigationBar()
        {

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            BackButton = GetTemplateChild(PartBackButton) as Button;
            if (BackButton == null)
            {
                Debug.WriteLine("BackButton == null");
            }
            ForwardButton = GetTemplateChild(PartForwardButton) as Button;
            HistoryToggleButton = GetTemplateChild(PartHistoryToggleButton) as ToggleButton;

            HistoryPopup = GetTemplateChild(PartHistoryPopup) as Popup;
            HistoryListView = GetTemplateChild(PartHistoryListView) as ListView;

            BreadcrumbsStackPanel = GetTemplateChild(PartBreadcrumbsStackPanel) as StackPanel;
            PathTextBox = GetTemplateChild(PartPathTextBox) as TextBox;

            SearchTextBox = GetTemplateChild(PartSearchTextBox) as TextBox;
            SearchPredictionPopup = GetTemplateChild(PartSearchPredictionPopup) as Popup;
            SearchToggleButton = GetTemplateChild(PartSearchToggleButton) as Button;
        }
    }
}
