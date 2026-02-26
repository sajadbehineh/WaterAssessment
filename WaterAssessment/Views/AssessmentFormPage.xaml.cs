using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Dispatching;
using WaterAssessment.Services;
using DispatcherQueuePriority = Windows.System.DispatcherQueuePriority;

namespace WaterAssessment.Views;

public sealed partial class AssessmentFormPage : Page
{
    public static readonly DependencyProperty IsPageLoadingProperty = DependencyProperty.Register(
        nameof(IsPageLoading),
        typeof(bool),
        typeof(AssessmentFormPage),
        new PropertyMetadata(true));

    public bool IsPageLoading
    {
        get => (bool)GetValue(IsPageLoadingProperty);
        set => SetValue(IsPageLoadingProperty, value);
    }

    public AssessmentViewModel ViewModel { get; set; }
    public AssessmentFormPage()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// این متد وقتی اجرا می‌شود که به این صفحه نویگیت (هدایت) شوید.
    /// پارامتر e.Parameter حاوی همان آبجکت Assessment است که از دیالوگ فرستادید.
    /// </summary>
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        IsPageLoading = true;

        Assessment assessmentModel;
        var assessmentService = App.Services.GetRequiredService<IAssessmentService>();
        var formValueViewModelFactory = App.Services.GetRequiredService<IFormValueViewModelFactory>();

        try
        {
            if (e.Parameter is Assessment { AssessmentID: > 0 } receivedAssessment)
            {
                using var db = new WaterAssessmentContext();
                var fullAssessment = await assessmentService.GetAssessmentForEditAsync(receivedAssessment.AssessmentID);

                assessmentModel = fullAssessment ?? receivedAssessment;
            }
            else
            {
                assessmentModel = new Assessment();
            }

            ViewModel = new AssessmentViewModel(assessmentModel, assessmentService, formValueViewModelFactory);

            this.DataContext = ViewModel;
            Bindings.Update();

            ViewModel.RowAdded -= OnViewModelRowAdded;
            ViewModel.RowAdded += OnViewModelRowAdded;
        }
        finally
        {
            IsPageLoading = false;
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.RowAdded -= OnViewModelRowAdded;
        }

        base.OnNavigatedFrom(e);
    }

    private void OnViewModelRowAdded()
    {
        _ = DispatcherQueue.TryEnqueue((Microsoft.UI.Dispatching.DispatcherQueuePriority)DispatcherQueuePriority.Low, async () =>
        {
            await Task.Delay(25);
            HydrometryRowsScrollViewer?.ChangeView(null, HydrometryRowsScrollViewer.ScrollableHeight, null, true);
        });
    }
}
