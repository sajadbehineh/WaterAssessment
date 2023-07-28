using WaterAssessment.Views;

namespace WaterAssessment;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        this.SetWindowSize(1650, 800);
    }

    private void NavigationViewItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var menuItem = sender as NavigationViewItem;

        if (menuItem?.Tag is not null)
        {
            switch (menuItem.Tag)
            {
                case "AssessmentForm":
                    ShellPage.Instance.Navigate(typeof(AssessmentFormPage));
                    break;

                case "InsertEmployees":
                    break;

                case "Location":
                    ShellPage.Instance.Navigate(typeof(LocationPage));
                    break;

                case "Propeller":
                    break;

                case "Settings":
                    ShellPage.Instance.Navigate(typeof(SettingsPage));
                    break;
            }
        }
    }

    //private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    //{
    //    if (args.IsSettingsSelected)
    //    {
    //        ShellPage.Instance.Navigate(typeof(SettingsPage));
    //    }
    //    else
    //    {
    //        var selectedItem = (Microsoft.UI.Xaml.Controls.NavigationViewItem)args.SelectedItem;
    //        string selectedItemTag = ((string)selectedItem.Tag);

    //        if (selectedItemTag is not null)
    //        {
    //            switch (selectedItemTag)
    //            {
    //                case "AssessmentForm":
    //                    ShellPage.Instance.Navigate(typeof(AssessmentFormPage));
    //                    break;

    //                case "InsertEmployees":
    //                    break;

    //                case "Location":
    //                    ShellPage.Instance.Navigate(typeof(LocationPage));
    //                    break;
    //                case "Propeller":
    //                    break;
    //            }
    //        }
    //    }
    //}
}