using Microsoft.UI.Xaml.Media.Animation;

namespace WaterAssessment.Views;

public sealed partial class ShellPage : Page
{
    internal static ShellPage Instance { get; private set; }

    public ShellPage()
    {
        this.InitializeComponent();
        Instance = this;
    }

    public void Navigate(Type pageType, NavigationTransitionInfo transitionInfo = null, object parameter = null)
    {
        if (transitionInfo is null)
        {
            transitionInfo = new EntranceNavigationTransitionInfo();
        }
        if (pageType is not null)
        {
            if (shellFrame?.Content?.GetType() != pageType)
            {
                shellFrame.Navigate(pageType, parameter, transitionInfo);
            }
        }
    }
}