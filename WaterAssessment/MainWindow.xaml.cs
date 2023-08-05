using Microsoft.EntityFrameworkCore;
using WaterAssessment.Converters;
using WaterAssessment.Models;
using WaterAssessment.Views;
using Windows.Globalization;
using DevExpress.WinUI.Editors.Internal;
using Microsoft.UI.Xaml.Controls.Primitives;
using WinRT;

namespace WaterAssessment;

public sealed partial class MainWindow : Window
{
    public ObservableCollection<Area> Areas { get; set; } = new ObservableCollection<Area>();
    public List<Location> Locations { get; set; } = new List<Location>();
    public ObservableCollection<LocationItem> LocationsViewModel { get; set; } = new ObservableCollection<LocationItem>();
    public ObservableCollection<Employee> Employees { get; set; } = new ObservableCollection<Employee>();
    public ObservableCollection<Propeller> Propellers { get; set; } = new ObservableCollection<Propeller>();
    public ObservableCollection<CurrentMeter> CurrentMeters { get; set; } = new ObservableCollection<CurrentMeter>();

    internal static MainWindow Instance { get; private set; }
    public MainWindow()
    {
        this.InitializeComponent();
        Instance = this;
        this.SetWindowSize(1650, 800);
    }

    private async void InputPanelContentDialog_OnLoaded(object sender, RoutedEventArgs e)
    {
        await Task.Run(() =>
        {
            GetEmployeesFromDB();
            GetAreasFromDB();
            GetLocationViewModel();
            GetPropellerFromDB();
            GetCurrentMeterFromDB();
        });
    }

    private void GetCurrentMeterFromDB()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            CurrentMeters?.Clear();
            using var db = new WaterAssessmentContext();
            var data = db.CurrentMeters.ToList();
            CurrentMeters = new ObservableCollection<CurrentMeter>(data);
            cmbCurrentMeter.ItemsSource = CurrentMeters;
        });
    }

    private void GetPropellerFromDB()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            Propellers?.Clear();
            using var db = new WaterAssessmentContext();
            var data = db.Propellers.ToList();
            Propellers = new(data);
            cmbPropeller.ItemsSource = Propellers;
        });
    }

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
        Locations = new List<Location>(data);
    }

    private void GetLocationViewModel()
    {
        GetLocationFromDB();
        DispatcherQueue.TryEnqueue(() =>
        {
            LocationsViewModel?.Clear();
            foreach (var location in Locations)
            {
                LocationsViewModel?.Add(new LocationItem
                {
                    LocationID = location.LocationID,
                    LocationName = location.Place,
                    AreaID = location.AreaID,
                    AreaName = Areas.FirstOrDefault(a => a.AreaID == location.AreaID)?.AreaName
                });
            }

            cmbLocation.ItemsSource = LocationsViewModel;
        });
    }

    private void GetEmployeesFromDB()
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            Employees?.Clear();
            await using var db = new WaterAssessmentContext();
            var data = await db.Employees.ToListAsync();
            Employees = new ObservableCollection<Employee>(data);
            employeeList.ItemsSource = Employees;
        });
    }

    private async void NavigationViewItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var menuItem = sender as NavigationViewItem;

        if (menuItem?.Tag is not null)
        {
            switch (menuItem.Tag)
            {
                case "AssessmentForm":
                    await inputPanelContentDialog.ShowAsyncQueue();
                    break;

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

    private void DatePicker_OnLoaded(object sender, RoutedEventArgs e)
    {
        datePicker.CalendarIdentifier = CalendarIdentifiers.Persian;
        datePicker.SelectedDate = (DateTimeOffset)(System.DateTime.Today.Date);
    }

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

    private void InputPanelContentDialog_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        //if (AssessmentFormPage.Instance is not null)
        //{
        //    AssessmentFormPage.Instance = null;
        //}
        ShellPage.Instance.Navigate(typeof(AssessmentFormPage));

        AssessmentItem assessmentItem = new AssessmentItem();

        if (AssessmentFormPage.Instance != null)
        {

            var selectedEmployees = employeeList.SelectedItems;
            if (selectedEmployees.Count > 0)
            {
                assessmentItem.EmployeeID = new int[selectedEmployees.Count];

                for (int i = 0; i < (selectedEmployees.Count); i++)
                {
                    assessmentItem.EmployeeID[i] = ((selectedEmployees[i]) as Employee).EmployeeID;
                }

                assessmentItem.Employee_1 = Employees.Where(e => e.EmployeeID == assessmentItem.EmployeeID[0])
                    .FirstOrDefault().ToString();
                assessmentItem.Employee_2 = Employees.Where(e => e.EmployeeID == assessmentItem.EmployeeID[1])
                    .FirstOrDefault().ToString();
                assessmentItem.Employee_3 = Employees.Where(emp => emp.EmployeeID == assessmentItem.EmployeeID[2])
                    .FirstOrDefault().ToString();
            }
            var locationID = ((cmbLocation.SelectedItem) as LocationItem).LocationID;
            assessmentItem.LocationID = locationID;
            assessmentItem.Location = LocationsViewModel.Where(l => l.LocationID == locationID)
                .FirstOrDefault().LocationArea;

            var date = (datePicker.SelectedDate.Value.Date);
            assessmentItem.Date = date;

            var echelon = echelonBox.Text;
            assessmentItem.Echelon = echelon;

            var openness = opennessBox.Text;
            assessmentItem.Openness = openness;

            var propellerID = ((cmbPropeller.SelectedItem) as Propeller).PropellerID;
            assessmentItem.PropellerID = propellerID;
            assessmentItem.PropellerName = ((cmbPropeller.SelectedItem) as Propeller).DeviceNumber;

            var currentMeterID = ((cmbCurrentMeter.SelectedItem) as CurrentMeter).CurrentMeterID;
            assessmentItem.CurrentMeterID = currentMeterID;
            assessmentItem.CurrentMeterName = ((cmbCurrentMeter.SelectedItem) as CurrentMeter).CurrentMeterName;

            var time = (int)cmbTimer.SelectedValue;
            assessmentItem.Timer = time;

            if (rowsCountNumberBox.Value > 0)
            {
                var rows = (int)rowsCountNumberBox.Value;
                assessmentItem.RowsCount = rows;
            }
            //var rows = (int)rowsCountNumberBox.Value > 0 ? (int)rowsCountNumberBox.Value : 4;
        }
        AssessmentFormPage.Instance.Assessment = assessmentItem;
    }
}