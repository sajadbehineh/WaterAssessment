namespace WaterAssessment.Views;

public sealed partial class Propeller_CurrentMeterPage : Page
{
    public ObservableCollection<Propeller> Propellers { get; set; } = new ObservableCollection<Propeller>();
    private bool _propellerIsEdited = false;

    public Propeller_CurrentMeterPage()
    {
        this.InitializeComponent();
        DataContext = this;
        Loaded += Propeller_CurrentMeterPage_Loaded;
    }

    private async void Propeller_CurrentMeterPage_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Run(() =>
        {
            GetPropellerFromDB();
        });
    }

    #region Propeller

    private void GetPropellerFromDB()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            Propellers?.Clear();
            using var db = new WaterAssessmentContext();
            var data = db.Propellers.ToList();
            Propellers = new(data);
            propellerListView.ItemsSource = Propellers;
        });
    }

    private void BtnAddPropeller_OnClick(object sender, RoutedEventArgs e)
    {
        var btnAdd = sender as Button;
        using var db = new WaterAssessmentContext();
        InitInfoBar.ImplementInfoBar(numBoxInfoBar, InfoBarSeverity.Error,
            String.IsNullOrWhiteSpace(propellerNumBox.Text), "وارد کردن شماره پروانه الزامی است.");
        InitInfoBar.ImplementInfoBar(aValueInfoBar, InfoBarSeverity.Error,
            String.IsNullOrWhiteSpace(aValueBox.Text), "وارد کردن ضریب (a) الزامی است.");
        InitInfoBar.ImplementInfoBar(bValueInfoBar, InfoBarSeverity.Error,
            String.IsNullOrWhiteSpace(bValueBox.Text), "وارد کردن ضریب (b) الزامی است.");

        if (!String.IsNullOrWhiteSpace(propellerNumBox.Text)
            && !String.IsNullOrWhiteSpace(aValueBox.Text)
            && !String.IsNullOrWhiteSpace(bValueBox.Text))
        {
            Propeller newPropeller = new Propeller
            {
                DeviceNumber = propellerNumBox.Text,
                AValue = Convert.ToDouble(aValueBox.Text),
                BValue = Convert.ToDouble(bValueBox.Text)
            };
            var duplicate = db.Propellers.Where(p =>
                p.DeviceNumber == newPropeller.DeviceNumber && p.AValue == newPropeller.AValue &&
                p.BValue == newPropeller.BValue).FirstOrDefault();

            if (duplicate != null)
            {
                InitInfoBar.ImplementInfoBar(numBoxInfoBar, InfoBarSeverity.Error,
                    true, $"پروانه شماره {duplicate.DeviceNumber} قبلاً ثبت شده است.");
                return;
            }

            if (_propellerIsEdited && btnAdd.DataContext is Propeller selectedPropeller)
            {
                Propeller propeller = db.Propellers.Find(selectedPropeller.PropellerID);
                if (propeller != null)
                {
                    propeller.DeviceNumber = newPropeller.DeviceNumber;
                    propeller.AValue = newPropeller.AValue;
                    propeller.BValue = newPropeller.BValue;
                    db.SaveChanges();
                    _propellerIsEdited = false;
                    GetPropellerFromDB();
                    propellerNumBox.Text = String.Empty;
                    aValueBox.Text = String.Empty;
                    bValueBox.Text = String.Empty;
                    btnAdd.Content = "ذخیره";
                    InitInfoBar.ImplementInfoBar(numBoxInfoBar, InfoBarSeverity.Success,
                        true, "پروانه مورد نظر شما با موفقیت ویرایش شد.");
                    propellerNumBox.Focus(FocusState.Pointer);
                }
            }
            else
            {
                db.Propellers.Add(newPropeller);
                db.SaveChanges();
                GetPropellerFromDB();
                propellerNumBox.Text = String.Empty;
                aValueBox.Text = String.Empty;
                bValueBox.Text = String.Empty;
                InitInfoBar.ImplementInfoBar(numBoxInfoBar, InfoBarSeverity.Success,
                    true, "پروانه مورد نظر شما با موفقیت ثبت شد.");
                propellerNumBox.Focus(FocusState.Pointer);
            }
        }
    }

    private void BtnHoverDeletePropeller_OnClick(object sender, RoutedEventArgs e)
    {
        var propellerID = (sender as AppBarButton).DataContext;
        using var db = new WaterAssessmentContext();
        var propeller = db.Propellers.Find(propellerID);
        if (propeller is not null)
        {
            db.Propellers.Remove(propeller);
            db.SaveChanges();
            GetPropellerFromDB();
            InitInfoBar.ImplementInfoBar(numBoxInfoBar, InfoBarSeverity.Warning, true,
                "پروانه مورد نظر شما با موفقیت حذف شد.");
        }
    }

    private void BtnHoverEditArea_OnClick(object sender, RoutedEventArgs e)
    {
        var propellerID = Convert.ToInt32((sender as AppBarButton).DataContext);
        if (((propellerListView.Items).Where(p => (p is Propeller propeller) && propeller.PropellerID == propellerID)
                .FirstOrDefault()) is Propeller selectedPropeller)
        {
            _propellerIsEdited = true;
            propellerNumBox.Text = selectedPropeller.DeviceNumber;
            aValueBox.Text = (selectedPropeller.AValue).ToString();
            bValueBox.Text = (selectedPropeller.BValue).ToString();
            btnAddPropeller.Content = "ویرایش";
            btnAddPropeller.DataContext = selectedPropeller;
        }
    }

    private void BtnClearTextBoxes_OnClick(object sender, RoutedEventArgs e)
    {
        if (_propellerIsEdited)
        {
            _propellerIsEdited = false;
        }
        propellerNumBox.Text = string.Empty;
        aValueBox.Text = string.Empty;
        bValueBox.Text = string.Empty;
        numBoxInfoBar.IsOpen = false;
        aValueInfoBar.IsOpen = false;
        bValueInfoBar.IsOpen = false;
        btnAddPropeller.Content = "ذخیره";
        propellerNumBox.Focus(FocusState.Pointer);
    }

    private void PropellerTextBoxes_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var currentTxt = (sender as TextBox).Text;
        btnClearPropellerBox.IsEnabled = !string.IsNullOrWhiteSpace(currentTxt);
    }

    private void propellerSwipeContainer_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse ||
            e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
        {
            VisualStateManager.GoToState(
                sender as Control, "HoverButtonsShown", true);
        }
    }

    private void propellerSwipeContainer_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(
            sender as Control, "HoverButtonsHidden", true);
    }

    #endregion
}