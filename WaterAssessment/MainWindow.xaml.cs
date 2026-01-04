using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Windowing;
using System.Diagnostics.Metrics;
using WaterAssessment.Models;
using WaterAssessment.Views;

namespace WaterAssessment;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new MainViewModel();

    public ObservableCollection<Area> Areas { get; set; } = new ObservableCollection<Area>();
    public List<Models.Location> Locations { get; set; } = new List<Models.Location>();
    public ObservableCollection<LocationViewModel> LocationsViewModel { get; set; } = new ObservableCollection<LocationViewModel>();
    public readonly List<Employee> Employees = new List<Employee>();
    public List<Propeller> Propellers { get; set; } = new List<Propeller>();
    public List<CurrentMeter> CurrentMeters { get; set; } = new List<CurrentMeter>();
    public ObservableCollection<Employee> SelectedEmployees { get; set; } = new();

    internal static MainWindow Instance { get; private set; }
    public MainWindow()
    {
        this.InitializeComponent();
        Instance = this;
        this.SetWindowSize(1650, 800);
        this.AppWindow.SetPresenter(AppWindowPresenterKind.Default);
    }

    private async void InputPanelContentDialog_OnLoaded(object sender, RoutedEventArgs e)
    {
        SelectedEmployees = new ObservableCollection<Employee>();
        await Task.Run(() =>
        {
            //GetEmployeesFromDB();
            GetAreasFromDB();
            //GetLocationViewModel();
            //GetPropellerFromDB();
            //GetCurrentMeterFromDB();
        });
    }

    //private void GetCurrentMeterFromDB()
    //{
    //    DispatcherQueue.TryEnqueue(() =>
    //    {
    //        CurrentMeters?.Clear();
    //        using var db = new WaterAssessmentContext();
    //        var data = db.CurrentMeters.ToList();
    //        CurrentMeters = new ObservableCollection<CurrentMeter>(data);
    //        cmbCurrentMeter.ItemsSource = CurrentMeters;
    //    });
    //}

    //private void GetPropellerFromDB()
    //{
    //    DispatcherQueue.TryEnqueue(() =>
    //    {
    //        Propellers?.Clear();
    //        using var db = new WaterAssessmentContext();
    //        var data = db.Propellers.ToList();
    //        Propellers = new(data);
    //        cmbPropeller.ItemsSource = Propellers;
    //    });
    //}

    private void GetAreasFromDB()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            Areas?.Clear();
            using var db = new WaterAssessmentContext();
            var data = db.Areas.ToList();
            Areas = new ObservableCollection<Area>(data);
        });
    }

    private void GetLocationFromDB()
    {
        Locations?.Clear();
        using var db = new WaterAssessmentContext();
        var data = db.Locations.ToList();
        Locations = new List<Models.Location>(data);
    }

    //private void GetLocationViewModel()
    //{
    //    GetLocationFromDB();
    //    DispatcherQueue.TryEnqueue(() =>
    //    {
    //        LocationsViewModel?.Clear();
    //        foreach (var location in Locations)
    //        {
    //            LocationsViewModel?.Add(new LocationViewModel
    //            {
    //                LocationID = location.LocationID,
    //                LocationName = location.LocationName,
    //                AreaID = location.AreaID,
    //                AreaName = Areas.FirstOrDefault(a => a.AreaID == location.AreaID)?.AreaName
    //            });
    //        }

    //        cmbLocation.ItemsSource = LocationsViewModel;
    //    });
    //}

    //private void GetEmployeesFromDB()
    //{
    //    DispatcherQueue.TryEnqueue(async () =>
    //    {
    //        Employees?.Clear();
    //        await using var db = new WaterAssessmentContext();
    //        var data = await db.Employees.ToListAsync();
    //        foreach (var employee in data)
    //        {
    //            Employees.Add(employee);
    //        }
    //    });
    //}

    private async void NavigationViewItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var menuItem = sender as NavigationViewItem;

        if (menuItem?.Tag is not null)
        {
            switch (menuItem.Tag)
            {
                case "AssessmentForm":
                    await ViewModel.LoadBaseDataAsync();
                    var dialog = new CreateAssessmentDialog(
                        ViewModel.Locations,
                        ViewModel.CurrentMeters,
                        ViewModel.Propellers,
                        ViewModel.Employees);
                    // XamlRoot برای WinUI 3 الزامی است
                    if (menuItem.XamlRoot != null)
                    {
                        dialog.XamlRoot = menuItem.XamlRoot;
                    }
                    else
                    {
                        // اگر به هر دلیلی نال بود، از Content پنجره استفاده کنید (روش جایگزین)
                        dialog.XamlRoot = this.Content.XamlRoot;
                    }

                    await dialog.ShowAsync();

                    // بررسی نتیجه
                    if (dialog.ViewModel.ResultAssessment != null)
                    {
                        var createdAssessment = dialog.ViewModel.ResultAssessment;
                        // رفتن به صفحه اصلی اندازه گیری با این داده‌ها
                        ShellPage.Instance.Navigate(typeof(AssessmentFormPage), null, createdAssessment);
                    }
                    break;
                //case "AssessmentForm":
                //    InputPanelContentDialog_OnLoaded(null, null);
                //    await inputPanelContentDialog.ShowAsyncQueue();
                //    break;

                case "InsertEmployees":
                    ShellPage.Instance.Navigate(typeof(EmployeePage));
                    break;

                case "Location":
                    ShellPage.Instance.Navigate(typeof(LocationPage));
                    break;

                case "Propeller_CurrentMeter":
                    ShellPage.Instance.Navigate(typeof(Propeller_CurrentMeterPage));
                    break;

                case "Settings":
                    ShellPage.Instance.Navigate(typeof(SettingsPage));
                    break;
            }
        }
    }

    //private void DatePicker_OnLoaded(object sender, RoutedEventArgs e)
    //{
    //    datePicker.CalendarIdentifier = CalendarIdentifiers.Persian;
    //    datePicker.SelectedDate = (DateTimeOffset)(System.DateTime.Today.Date);
    //}

    private void CmbTimer_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //var fiftyPanelTextBoxes = AssessmentFormPage.Instance.GetPanelTextBoxes(AssessmentFormPage.Instance.fiftyPanel);
        //var oneSecPanelTextBoxes = GetPanelTextBoxes(oneSecPanel);
        //var pointPanelTextBoxes = GetPanelTextBoxes(pointPanel);
        //var cmb = sender as ComboBox;
        //time = (int)cmb.SelectedValue;
        //for (int i = 0; i < fiftyPanelTextBoxes.Count; i++)
        //{
        //    Binding bindingFifty = new Binding()
        //    {
        //        Source = fiftyPanelTextBoxes[i],
        //        Converter = new ToOneSecConverter(),
        //        ConverterParameter = time,
        //        Path = new PropertyPath("Text"),
        //        //TargetNullValue = null,
        //        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        //    };
        //    oneSecPanelTextBoxes[i].SetBinding(TextBox.TextProperty, bindingFifty);

        //    Binding bindingOneSec = new Binding()
        //    {
        //        Source = oneSecPanelTextBoxes[i],
        //        Converter = new RadianToVelocityConverter(),
        //        ConverterParameter = propeller4,
        //        Path = new PropertyPath("Text"),
        //        //TargetNullValue = null,
        //        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        //    };
        //    pointPanelTextBoxes[i].SetBinding(TextBox.TextProperty, bindingOneSec);
    }

    //private void InputPanelContentDialog_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    //{
    //    ShellPage.Instance.Navigate(typeof(AssessmentFormPage));

    //    AssessmentItem assessmentItem = new AssessmentItem();


    //    if (AssessmentFormPage.Instance != null)
    //    {
    //        if (SelectedEmployees.Count > 0)
    //        {
    //            assessmentItem.EmployeeID = new int[SelectedEmployees.Count];
    //            for (int i = 0; i < SelectedEmployees.Count; i++)
    //            {
    //                assessmentItem.EmployeeID[i] = ((SelectedEmployees[i] as Employee).EmployeeID);
    //            }

    //            assessmentItem.Employee_1 = SelectedEmployees
    //                .Where(e => e.EmployeeID == assessmentItem.EmployeeID[0])
    //                .FirstOrDefault().ToString();
    //            assessmentItem.Employee_2 = SelectedEmployees
    //                .Where(e => e.EmployeeID == assessmentItem.EmployeeID[1])
    //                .FirstOrDefault().ToString();
    //            assessmentItem.Employee_3 = SelectedEmployees
    //                .Where(e => e.EmployeeID == assessmentItem.EmployeeID[2])
    //                .FirstOrDefault().ToString();
    //        }

    //        var locationID = ((cmbLocation.SelectedItem) as LocationViewModel).LocationID;
    //        assessmentItem.LocationID = locationID;
    //        assessmentItem.Location = LocationsViewModel
    //                .Where(l => l.LocationID == locationID)
    //                .FirstOrDefault().LocationArea;

    //        var date = (DateTime)(datePicker.SelectedDate.Value.DateTime);
    //        assessmentItem.Date = date.ToShortDateString();

    //        var echelon = echelonBox.Text;
    //        assessmentItem.Echelon = echelon;

    //        var openness = opennessBox.Text;
    //        assessmentItem.Openness = openness;

    //        var propeller = ((cmbPropeller.SelectedItem) as Propeller);
    //        assessmentItem.Propeller = propeller;
    //        assessmentItem.PropellerID = propeller.PropellerID;
    //        assessmentItem.PropellerName = propeller.DeviceNumber;

    //        var currentMeterID = ((cmbCurrentMeter.SelectedItem) as CurrentMeter).CurrentMeterID;
    //        assessmentItem.CurrentMeterID = currentMeterID;
    //        assessmentItem.CurrentMeterName = ((cmbCurrentMeter.SelectedItem) as CurrentMeter).CurrentMeterName;

    //        var time = (int)cmbTimer.SelectedValue;
    //        assessmentItem.Timer = time;

    //        if (rowsCountNumberBox.Value > 0)
    //        {
    //            var rows = (int)rowsCountNumberBox.Value;
    //            assessmentItem.RowsCount = rows;
    //        }
    //        //var rows = (int)rowsCountNumberBox.Value > 0 ? (int)rowsCountNumberBox.Value : 4;
    //    }
    //    AssessmentFormPage.Instance.Assessment = assessmentItem;
    //}

    private void TokenBox_OnTokenItemAdded(TokenizingTextBox sender, object args)
    {
        if (args is Employee employee)
        {
            SelectedEmployees.Add(employee);
        }
    }

    private void TokenBox_OnTokenItemRemoved(TokenizingTextBox sender,
        TokenItemRemovingEventArgs args)
    {
        if (args.Item is Employee employee)
        {
            SelectedEmployees.Remove(employee);
        }
    }
}