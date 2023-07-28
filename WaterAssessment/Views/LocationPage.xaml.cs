namespace WaterAssessment.Views;

public sealed partial class LocationPage : Page
{
    public ObservableCollection<Area> Areas { get; set; } = new ObservableCollection<Area>();
    public List<Location> Locations { get; set; } = new List<Location>();
    public ObservableCollection<LocationItem> LocationsViewModel { get; set; } = new ObservableCollection<LocationItem>();

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
            GetLocationFromDB();
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

        if (!string.IsNullOrEmpty(locationBox.Text) && cmbArea.SelectedIndex >= 0)
        {
            Location newLocation = new Location
            {
                Place = locationBox.Text,
                AreaID = (cmbArea.SelectedItem as Area).AreaID,
            };
            // مکان تکراری بر اساس حوزه اندازه گیری چک شود
            var duplicate = db.Locations.Where(l => l.Place == newLocation.Place && l.AreaID == newLocation.AreaID).FirstOrDefault();

            if (duplicate is not null)
            {
                InitInfoBar.ImplementInfoBar(locationInfoBar, InfoBarSeverity.Error,
                    true, $"مکان اندازه گیری {duplicate.Place} قبلاً ثبت شده است.");
            }
            else
            {
                if ((string)btnAdd.Content == "ویرایش" && btnAdd.DataContext is LocationItem selectedLocation)
                {
                    var place = locationBox.Text;
                    var areaID = (cmbArea.SelectedItem as Area).AreaID;

                    if (place != null && areaID >= 1)
                    {
                        Location location = db.Locations.Find(selectedLocation.LocationID);
                        location.Place = place;
                        location.AreaID = areaID;
                        db.SaveChanges();
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
                    locationsListView.ScrollIntoView(newLocation.LocationID);
                }
            }
        }

    }

    private void BtnHoverDeleteLocation_OnClick(object sender, RoutedEventArgs e)
    {
        var locationID = (sender as AppBarButton).Tag;
        using var db = new WaterAssessmentContext();
        var location = db.Locations.Find(locationID);
        if (location is not null)
        {
            db.Locations.Remove(location);
            db.SaveChanges();
            GetLocationViewModel();
            InitInfoBar.ImplementInfoBar(locationInfoBar, InfoBarSeverity.Warning, true,
                "مکان مورد نظر شما با موفقیت حذف شد.");
        }

    }

    private void BtnClearLocationBox_OnClick(object sender, RoutedEventArgs e)
    {
        cmbArea.SelectedIndex = -1;
        locationBox.Text = string.Empty;
        locationInfoBar.IsOpen = false;
        btnAddLocation.Content = "ذخیره";
        locationBox.Focus(FocusState.Pointer);
        cmbAreaInfoBar.IsOpen = false;
    }

    private void BtnHoverEditLocation_OnClick(object sender, RoutedEventArgs e)
    {
        var locationID = Convert.ToInt32((sender as AppBarButton).Tag);
        if (((locationsListView.Items).Where(l => (l is LocationItem location) && location.LocationID == locationID)
                .FirstOrDefault()) is LocationItem selectedLocation)
        {
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
        if (string.IsNullOrEmpty(areaBox.Text))
        {
            InitInfoBar.ImplementInfoBar(areaInfoBar, InfoBarSeverity.Error, true, "لطفاً نام حوزه را وارد کنید.");
        }
        else
        {
            Area newArea = new Area { AreaName = areaBox.Text };
            var duplicate = db.Areas.Where(a => a.AreaName == newArea.AreaName).FirstOrDefault();

            if (duplicate is not null)
            {
                InitInfoBar.ImplementInfoBar(areaInfoBar, InfoBarSeverity.Error, true,
                    $"حوزه {duplicate.AreaName} قبلاً ثبت شده است.");
            }
            else
            {
                if ((string)btnAdd.Content == "ویرایش" && btnAdd.DataContext is Area selectedArea)
                {
                    var areaName = areaBox.Text;

                    if (areaName != null)
                    {
                        Area area = db.Areas.Find(selectedArea.AreaID);
                        area.AreaName = areaName;
                        db.SaveChanges();
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
                    areaBox.Focus(FocusState.Pointer);
                    InitInfoBar.ImplementInfoBar(areaInfoBar, InfoBarSeverity.Success, true, "حوزه مورد نظر شما با موفقیت ثبت شد.");
                }
            }
        }
    }

    private async void BtnHoverDeleteArea_OnClick(object sender, RoutedEventArgs e)
    {
        var areaID = (sender as AppBarButton).Tag;
        await using var db = new WaterAssessmentContext();
        var area = db.Areas.Find(areaID);

        if (area is not null)
        {
            db.Areas.Remove(area);
            await db.SaveChangesAsync();
            GetAreasFromDB();
            GetLocationViewModel();
            InitInfoBar.ImplementInfoBar(areaInfoBar, InfoBarSeverity.Warning, true,
                "حوزه مورد نظر شما با موفقیت حذف شد.");
        }
    }

    private void BtnClearAreaBox_OnClick(object sender, RoutedEventArgs e)
    {
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
        var areaID = Convert.ToInt32((sender as AppBarButton).Tag);
        if (((areasListView.Items).Where(a => (a is Area area) && area.AreaID == areaID).FirstOrDefault()) is Area selectedArea)
        {
            areaBox.Text = selectedArea.AreaName;
            btnAddArea.Content = "ویرایش";
            btnAddArea.DataContext = selectedArea;
        }
    }

    #endregion
}
