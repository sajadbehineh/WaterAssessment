namespace WaterAssessment.Views;

public sealed partial class LocationPage : Page
{
    public ObservableCollection<Area> Areas { get; set; } = new ObservableCollection<Area>();
    public List<Location> Locations { get; set; } = new List<Location>();
    public ObservableCollection<LocationItem> LocationsViewModel { get; set; } = new ObservableCollection<LocationItem>();

    private bool _locationIsEdited = false;
    private bool _areaIsEdited = false;

    public LocationPage()
    {
        this.InitializeComponent();
        DataContext = this;
        Loaded += LocationPage_Loaded;
    }

    private async void LocationPage_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Run(() =>
        {
            GetAreasFromDB();
            GetLocationViewModel();
        });
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

            locationsListView.ItemsSource = LocationsViewModel;
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
            areasListView.ItemsSource = Areas;
            cmbArea.ItemsSource = Areas;
        });
    }

    private void GetLocationFromDB()
    {
        Locations?.Clear();
        using var db = new WaterAssessmentContext();
        var data = db.Locations.ToList();
        Locations = new List<Location>(data);
    }

    #region Location

    private void BtnAddLocation_OnClick(object sender, RoutedEventArgs e)
    {
        var btnAdd = sender as Button;
        using var db = new WaterAssessmentContext();

        InitInfoBar.ImplementInfoBar(cmbAreaInfoBar, InfoBarSeverity.Error,
                cmbArea.SelectedIndex == -1, "لطفاً حوزه مورد نظر را انتخاب کنید.");
        InitInfoBar.ImplementInfoBar(locationInfoBar, InfoBarSeverity.Error,
                    string.IsNullOrEmpty(locationBox.Text), "لطفاً مکان اندازه گیری را وارد کنید.");

        if (!string.IsNullOrWhiteSpace(locationBox.Text) && cmbArea.SelectedIndex >= 0)
        {
            Location newLocation = new Location
            {
                Place = locationBox.Text,
                AreaID = (cmbArea.SelectedItem as Area).AreaID,
            };

            var duplicate = db.Locations.Where(l => l.Place == newLocation.Place && l.AreaID == newLocation.AreaID).FirstOrDefault();

            if (duplicate is not null)
            {
                InitInfoBar.ImplementInfoBar(locationInfoBar, InfoBarSeverity.Error,
                    true, $"مکان اندازه گیری {duplicate.Place} قبلاً ثبت شده است.");
                return;
            }

            if (_locationIsEdited && btnAdd.DataContext is LocationItem selectedLocation)
            {
                Location location = db.Locations.Find(selectedLocation.LocationID);
                if (location is not null)
                {
                    location.Place = newLocation.Place;
                    location.AreaID = newLocation.AreaID;
                    db.SaveChanges();
                    _locationIsEdited = false;
                    GetLocationViewModel();
                    locationBox.Text = String.Empty;
                    cmbArea.SelectedIndex = -1;
                    btnAddLocation.Content = "ذخیره";
                    InitInfoBar.ImplementInfoBar(locationInfoBar, InfoBarSeverity.Success,
                        true, "مکان مورد نظر شما با موفقیت ویرایش شد.");
                    locationBox.Focus(FocusState.Pointer);
                }
            }
            else
            {
                db.Locations.Add(newLocation);
                db.SaveChanges();
                GetLocationViewModel();
                locationBox.Text = string.Empty;
                cmbArea.SelectedIndex = -1;
                InitInfoBar.ImplementInfoBar(locationInfoBar, InfoBarSeverity.Success,
                    true, "مکان مورد نظر شما با موفقیت ثبت شد.");
                locationBox.Focus(FocusState.Pointer);
            }
        }
    }

    private void BtnHoverDeleteLocation_OnClick(object sender, RoutedEventArgs e)
    {
        var locationID = (sender as AppBarButton).DataContext;
        using var db = new WaterAssessmentContext();
        var location = db.Locations.Find(locationID);
        if (location is not null)
        {
            db.Locations.Remove(location);
            db.SaveChanges();
            GetLocationViewModel();
            InitInfoBar.ImplementInfoBar(locationInfoBar, InfoBarSeverity.Warning,
                true, "مکان مورد نظر شما با موفقیت حذف شد.");
        }

    }

    private void BtnClearLocationBox_OnClick(object sender, RoutedEventArgs e)
    {
        if (_locationIsEdited)
        {
            _locationIsEdited = false;
        }
        cmbArea.SelectedIndex = -1;
        locationBox.Text = string.Empty;
        locationInfoBar.IsOpen = false;
        btnAddLocation.Content = "ذخیره";
        locationBox.Focus(FocusState.Pointer);
        cmbAreaInfoBar.IsOpen = false;
    }

    private void BtnHoverEditLocation_OnClick(object sender, RoutedEventArgs e)
    {
        var locationID = Convert.ToInt32((sender as AppBarButton).DataContext);
        if (((locationsListView.Items).Where(l => (l is LocationItem location) && location.LocationID == locationID)
                .FirstOrDefault()) is LocationItem selectedLocation)
        {
            _locationIsEdited = true;
            locationBox.Text = selectedLocation.LocationName;
            cmbArea.SelectedItem = (cmbArea.Items).Where(l => (l is Area area) && area.AreaID == selectedLocation.AreaID).FirstOrDefault();
            btnAddLocation.Content = "ویرایش";
            btnAddLocation.DataContext = selectedLocation;
        }
    }

    private void LocationSwipeContainer_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse ||
            e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
        {
            VisualStateManager.GoToState(
                sender as Control, "HoverButtonsShown", true);
        }
    }

    private void LocationSwipeContainer_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(
            sender as Control, "HoverButtonsHidden", true);
    }

    private void LocationBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var txtLocationBox = (sender as TextBox).Text;
        btnClearLocationBox.IsEnabled = !string.IsNullOrEmpty(txtLocationBox);
    }

    private void CmbArea_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var cmbAreas = (sender as ComboBox);
        btnClearLocationBox.IsEnabled = cmbAreas.SelectedIndex >= 0;
    }

    #endregion


    #region Area

    private void BtnAddArea_OnClick(object sender, RoutedEventArgs e)
    {
        var btnAdd = (sender as Button);
        using var db = new WaterAssessmentContext();
        InitInfoBar.ImplementInfoBar(areaInfoBar, InfoBarSeverity.Error,
            string.IsNullOrWhiteSpace(areaBox.Text), "لطفاً نام حوزه را وارد کنید.");

        if (!string.IsNullOrWhiteSpace(areaBox.Text))
        {
            Area newArea = new Area { AreaName = areaBox.Text };
            var duplicate = db.Areas.Where(a => a.AreaName == newArea.AreaName).FirstOrDefault();

            if (duplicate is not null)
            {
                InitInfoBar.ImplementInfoBar(areaInfoBar, InfoBarSeverity.Error, true,
                    $"حوزه {duplicate.AreaName} قبلاً ثبت شده است.");
                return;
            }

            if (_areaIsEdited && btnAdd.DataContext is Area selectedArea)
            {
                Area area = db.Areas.Find(selectedArea.AreaID);
                if (area != null)
                {
                    area.AreaName = newArea.AreaName;
                    db.SaveChanges();
                    _areaIsEdited = false;
                    GetAreasFromDB();
                    areaBox.Text = string.Empty;
                    btnAddArea.Content = "ذخیره";
                    InitInfoBar.ImplementInfoBar(areaInfoBar, InfoBarSeverity.Success, true,
                        "حوزه مورد نظر شما با موفقیت ویرایش شد.");
                    areaBox.Focus(FocusState.Pointer);
                }
            }
            else
            {
                db.Areas.Add(newArea);
                db.SaveChanges();
                GetAreasFromDB();
                areaBox.Text = string.Empty;
                InitInfoBar.ImplementInfoBar(areaInfoBar, InfoBarSeverity.Success,
                    true, "حوزه مورد نظر شما با موفقیت ثبت شد.");
                areaBox.Focus(FocusState.Pointer);
            }
        }
    }

    private void BtnHoverDeleteArea_OnClick(object sender, RoutedEventArgs e)
    {
        var areaID = (sender as AppBarButton).DataContext;
        using var db = new WaterAssessmentContext();
        var area = db.Areas.Find(areaID);

        if (area is not null)
        {
            db.Areas.Remove(area);
            db.SaveChanges();
            GetAreasFromDB();
            GetLocationViewModel();
            InitInfoBar.ImplementInfoBar(areaInfoBar, InfoBarSeverity.Warning, true,
                "حوزه مورد نظر شما با موفقیت حذف شد.");
        }
    }

    private void BtnClearAreaBox_OnClick(object sender, RoutedEventArgs e)
    {
        if (_areaIsEdited)
        {
            _areaIsEdited = false;
        }
        areaBox.Text = string.Empty;
        areaInfoBar.IsOpen = false;
        btnAddArea.Content = "ذخیره";
        areaBox.Focus(FocusState.Pointer);
    }

    private void ListViewSwipeContainer_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse ||
            e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
        {
            VisualStateManager.GoToState(
                sender as Control, "HoverButtonsShown", true);
        }
    }

    private void ListViewSwipeContainer_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(
            sender as Control, "HoverButtonsHidden", true);
    }

    private void AreaBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var txtAreaBox = (sender as TextBox).Text;
        btnClearAreaBox.IsEnabled = !string.IsNullOrEmpty(txtAreaBox);
    }

    private void BtnHoverEditArea_OnClick(object sender, RoutedEventArgs e)
    {
        var areaID = Convert.ToInt32((sender as AppBarButton).DataContext);
        if (((areasListView.Items).Where(a => (a is Area area) && area.AreaID == areaID).FirstOrDefault()) is Area selectedArea)
        {
            _areaIsEdited = true;
            areaBox.Text = selectedArea.AreaName;
            btnAddArea.Content = "ویرایش";
            btnAddArea.DataContext = selectedArea;
        }
    }

    #endregion
}
