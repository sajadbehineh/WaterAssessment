using Microsoft.UI.Xaml.Media.Animation;
using WaterAssessment.Core;

namespace WaterAssessment.Views;

public sealed partial class ShellPage : Page
{
    internal static ShellPage Instance { get; private set; }

    public ShellPage()
    {
        this.InitializeComponent();
        Instance = this;
        //this.Loaded += ShellPage_Loaded;
    }

    private void ShellPage_Loaded(object sender, RoutedEventArgs e)
    {
        // —Ê?œ«œ —« Õ–› ò‰?œ  « ›ﬁÿ ?ò »«— «Ã—« ‘Êœ
        this.Loaded -= ShellPage_Loaded;

        // œ” —”? »Â ViewModel «“ ÿ—?ﬁ ‰„Ê‰Â MainWindow
        var mainViewModel = MainWindow.Instance.ViewModel;

        // «ê— ò«—»— ·«ê?‰ ‰ò—œÂ »«‘œ° »Â ’›ÕÂ ·«ê?‰ »—Ê?œ
        if (!mainViewModel.IsLoggedIn)
        {
            Navigate(typeof(LoginPage));
        }
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

        if (pageType != typeof(LoginPage) && !AppSession.IsLoggedIn)
        {
            shellFrame.Navigate(typeof(LoginPage));
        }
    }
}