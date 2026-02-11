using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using WaterAssessment.Services;

namespace WaterAssessment.Models.ViewModel
{
    public partial class AssessmentViewModel : ObservableObject
    {
        private readonly IAssessmentService _assessmentService;
        private readonly IFormValueViewModelFactory _formValueViewModelFactory;
        public Assessment Model { get; }

        // ==========================================================
        // لیست‌های مرجع (برای پر کردن کمبوباکس‌ها)
        // ==========================================================
        public ObservableCollection<Location> AllLocations { get; } = new();
        public ObservableCollection<Propeller> AllPropellers { get; } = new();
        public ObservableCollection<CurrentMeter> AllCurrentMeters { get; } = new();
        public ObservableCollection<Employee> AllEmployees { get; } = new();

        // لیست گشودگی دریچه‌ها (برای بایندینگ در UI)
        public ObservableCollection<AssessmentGate> GateValues { get; } = new();

        // لیست وضعیت پمپ‌ها برای بایندینگ در UI
        public ObservableCollection<AssessmentPump> PumpStates { get; } = new();

        // لیست مرجع پمپ‌های تعریف شده برای مکان انتخاب شده
        public ObservableCollection<LocationPump> AvailablePumps { get; } = new();

        // لیست ردیف‌های اندازه‌گیری (فرزندان)
        public ObservableCollection<FormValueViewModel> FormValues { get; } = new();

        public ObservableCollection<Assessment_Employee> AssessmentEmployees { get; } = new();

        // ==========================================================
        // پراپرتی‌های انتخابی (Selected Items)
        // ==========================================================

        [ObservableProperty]
        private Location _selectedLocation;
        partial void OnSelectedLocationChanged(Location value)
        {
            if (value == null) return;

            Model.LocationID = value.LocationID;
            Model.Location = value;

            // مدیریت دریچه‌ها بر اساس تعداد دریچه مکان انتخاب شده
            UpdateGateOpenings(value.GateCount ?? 0);
            // مدیریت پمپ‌ها
            UpdatePumpStates(value);
        }

        [ObservableProperty]
        private Propeller _selectedPropeller;
        partial void OnSelectedPropellerChanged(Propeller value)
        {
            if (value == null) return;
            Model.PropellerID = value.PropellerID;
            Model.Propeller = value;

            // باید پروانه جدید را به تمام سطرهای فرزند هم اطلاع دهیم تا سرعت را با فرمول جدید حساب کنند
            foreach (var row in FormValues)
            {
                row.Propeller = value;
            }
        }

        [ObservableProperty]
        private CurrentMeter _selectedCurrentMeter;
        partial void OnSelectedCurrentMeterChanged(CurrentMeter value)
        {
            if (value == null) return;
            Model.CurrentMeterID = value.CurrentMeterID;
            Model.CurrentMeter = value;
        }

        [ObservableProperty] private Employee _selectedEmployeeToAdd;



        // ==========================================================
        // وضعیت UI (مشغولی، پیام‌ها)
        // ==========================================================
        [ObservableProperty] private bool _isBusy;
        [ObservableProperty] private string _infoMessage;
        [ObservableProperty] private InfoBarSeverity _infoSeverity = InfoBarSeverity.Informational;
        [ObservableProperty] private bool _isInfoOpen;

        // 1. تشخیص حالت ویرایش: اگر ID بزرگتر از 0 باشد یعنی رکورد قبلاً در دیتابیس بوده
        public bool IsEditMode => Model.AssessmentID > 0;

        // 2. تعیین متن دکمه بر اساس حالت
        // این پراپرتی به خاصیت Content دکمه در XAML وصل می‌شود
        public string SaveButtonContent => IsEditMode ? "ذخیره تغییرات (ویرایش)" : "ذخیره اندازه گیری";

        [ObservableProperty]
        private int _timer;
        partial void OnTimerChanged(int value)
        {
            Model.Timer = value;

            foreach (var row in FormValues) row.MeasureTime = value;
        }

        [ObservableProperty] private DateTime _date;
        partial void OnDateChanged(DateTime value) => Model.Date = value;

        [ObservableProperty] private double? _echelon;
        partial void OnEchelonChanged(double? value) => Model.Echelon = value;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalFlowDisplay))]
        private double _totalFlow;
        public string TotalFlowDisplay => TotalFlow.ToString("N3"); // نمایش با 3 رقم اعشار

        // نمایش وضعیت پمپ‌ها در لیبل نهایی
        public string PumpStatesDisplay => PumpStates.Any()
            ? string.Join(" | ", PumpStates.Select(p => $"{p.LocationPump?.PumpName}: {(p.IsRunning ? "روشن" : "خاموش")}"))
            : "بدون پمپ";

        public AssessmentViewModel(Assessment model, IAssessmentService assessmentService, IFormValueViewModelFactory formValueViewModelFactory)
        {
            _assessmentService = assessmentService;
            _formValueViewModelFactory = formValueViewModelFactory;
            Model = model;
            SelectedPropeller = model.Propeller;

            // مقداردهی اولیه فیلد‌ها از مدل
            Timer = model.Timer == 0 ? 50 : model.Timer; // پیش‌فرض 50
            Date = model.Date == default ? DateTime.Now : model.Date;
            Echelon = model.Echelon;
            TotalFlow = model.TotalFlow;

            // لود کردن کارمندان انتخاب شده از دیالوگ
            foreach (var emp in model.AssessmentEmployees) AssessmentEmployees.Add(emp);

            foreach (var gateVal in model.GateOpenings)
            {
                GateValues.Add(gateVal);
            }

            if (model.FormValues != null && model.FormValues.Any())
            {
                // حالت ویرایش: لود کردن سطرهای موجود
                foreach (var fv in model.FormValues)
                {
                    var vm = _formValueViewModelFactory.Create(fv, SelectedPropeller);
                    SubscribeToChildEvents(vm); // اشتراک در رویدادها
                    FormValues.Add(vm);
                }
                // یک بار محاسبات کلی را انجام بده (برای اطمینان از صحت مقادیر لود شده)
                RecalculateGeometryAndFlow();
            }
            else
            {
                // +++ حالت جدید: ایجاد ۶ سطر پیش‌فرض +++
                InitializeDefaultRows();
            }

            // لود کردن داده‌های مرجع (مکان‌ها و...) از دیتابیس
            _ = LoadReferenceDataAsync();
        }

        private async Task LoadReferenceDataAsync()
        {
            var referenceData = await _assessmentService.GetReferenceDataAsync();
            var locations = referenceData.Locations;
            var propellers = referenceData.Propellers;
            var meters = referenceData.CurrentMeters;
            var employees = referenceData.Employees;
            //var locations = await _assessmentService.GetAllLocationsAsync();
            //var propellers = await _assessmentService.GetAllPropellersAsync();
            //var meters = await _assessmentService.GetAllCurrentMetersAsync();
            //var employees = await _assessmentService.GetAllEmployeesAsync();

            AllLocations.Clear();
            foreach (var item in locations) AllLocations.Add(item);

            AllPropellers.Clear();
            foreach (var item in propellers) AllPropellers.Add(item);

            AllCurrentMeters.Clear();
            foreach (var item in meters) AllCurrentMeters.Add(item);

            AllEmployees.Clear();
            foreach (var item in employees) AllEmployees.Add(item);

            // 2. ست کردن مقادیر انتخاب شده (اگر ویرایش است یا مدل پر شده است)
            if (Model.LocationID > 0)
                SelectedLocation = AllLocations.FirstOrDefault(x => x.LocationID == Model.LocationID);

            if (Model.PropellerID > 0)
                SelectedPropeller = AllPropellers.FirstOrDefault(x => x.PropellerID == Model.PropellerID);

            if (Model.CurrentMeterID > 0)
                SelectedCurrentMeter = AllCurrentMeters.FirstOrDefault(x => x.CurrentMeterID == Model.CurrentMeterID);

            // 3. لود کردن دریچه‌ها
            // اگر ویرایش است، دریچه‌های ذخیره شده را می‌آوریم
            //if (Model.GateOpenings != null && Model.GateOpenings.Any())
            //{
            //    foreach (var g in Model.GateOpenings) GateValues.Add(g);
            //}
            else if (SelectedLocation != null)
            {
                // اگر جدید است، بر اساس مکان پیش‌فرض بساز
                UpdateGateOpenings(SelectedLocation.GateCount ?? 0);
            }
        }

        // ==========================================================
        // متدهای مدیریت تجهیزات بالادست
        // ==========================================================

        private void UpdateGateOpenings(int count)
        {
            // اگر تعداد فعلی با تعداد درخواستی یکی است، دست نزن (تا مقادیر وارد شده نپرد)
            if (GateValues.Count == count) return;

            GateValues.Clear();

            // همگام‌سازی با مدل
            if (Model.GateOpenings == null) Model.GateOpenings = new List<AssessmentGate>();
            Model.GateOpenings.Clear();

            for (int i = 1; i <= count; i++)
            {
                var newVal = new AssessmentGate { GateNumber = i, Value = 0, AssessmentID = Model.AssessmentID };

                GateValues.Add(newVal);
                Model.GateOpenings.Add(newVal);
            }
            OnPropertyChanged(nameof(HasGates)); // برای مدیریت نمایش در UI
        }

        private async void UpdatePumpStates(Location location)
        {
            PumpStates.Clear();
            AvailablePumps.Clear();

            var existingStates = Model.PumpStates?
                                     .Where(state => state.LocationPumpID != 0)
                                     .ToDictionary(state => state.LocationPumpID, state => state.IsRunning)
                                 ?? new Dictionary<int, bool>();

            // لود کردن پمپ‌هایی که برای این مکان تعریف شده‌اند
            var pumps = await _assessmentService.GetLocationPumpsAsync(location.LocationID);

            foreach (var pump in pumps)
            {
                AvailablePumps.Add(pump);

                // ایجاد یک رکورد وضعیت برای این اندازه‌گیری
                var state = new AssessmentPump
                {
                    LocationPumpID = pump.Id,
                    LocationPump = pump,
                    IsRunning = existingStates.TryGetValue(pump.Id, out var isRunning) && isRunning,
                    AssessmentID = Model.AssessmentID
                };
                PumpStates.Add(state);
            }
            OnPropertyChanged(nameof(HasPumps));
        }

        // پراپرتی‌های کمکی برای کنترل Visibility در XAML
        public bool HasGates => GateValues.Count > 0;
        public bool HasPumps => PumpStates.Count > 0;

        // ==========================================================
        // پراپرتی‌های نمایشی و اصلی
        // ==========================================================

        public string LocationNameDisplay => Model.Location?.LocationName ?? "نامشخص";
        public string PropellerNameDisplay => Model.Propeller?.PropellerName ?? "نامشخص";
        public string CurrentMeterNameDisplay => Model.CurrentMeter?.CurrentMeterName ?? "نامشخص";
        public string EchelonDisplay => Echelon.HasValue ? Echelon.Value.ToString("N2") : "---";
        public string GateOpeningsDisplay
        {
            get
            {
                // اگر لیستی وجود ندارد یا خالی است
                if (Model.GateOpenings == null || !Model.GateOpenings.Any())
                    return "---";

                // مقادیر را می‌گیرد، درصد به تهش می‌چسباند و با خط تیره جدا می‌کند
                // نتیجه: "20% - 15%"
                return string.Join(" - ", Model.GateOpenings.Select(g => $"{g.Value}%"));
            }
        }
        public string EmployeeNamesDisplay => AssessmentEmployees.Any()
            ? string.Join(" - ", AssessmentEmployees.Select(ae => ae.Employee?.LastName ?? "?"))
            : "---";


        // ==========================================================
        // متدهای ذخیره‌سازی (Add / Edit)
        // ==========================================================

        [RelayCommand]
        private async Task SubmitFormAsync()
        {
            if (SelectedLocation == null)
            {
                await ShowInfo("لطفاً محل اندازه گیری را انتخاب کنید.", InfoBarSeverity.Warning);
                return;
            }

            if (SelectedPropeller == null)
            {
                await ShowInfo("لطفاً پروانه را انتخاب کنید.", InfoBarSeverity.Warning);
                return;
            }

            if (SelectedCurrentMeter == null)
            {
                await ShowInfo("لطفاً مولینه را انتخاب کنید.", InfoBarSeverity.Warning);
                return;
            }

            if (!Echelon.HasValue)
            {
                await ShowInfo("لطفاً مقدار اشل را وارد کنید.", InfoBarSeverity.Warning);
                return;
            }

            if (!AssessmentEmployees.Any())
            {
                await ShowInfo("لطفاً حداقل یک اندازه گیر را انتخاب کنید", InfoBarSeverity.Warning);
                return;
            }

            if (IsEditMode)
            {
                await EditAssessmentAsync();
            }
            else
            {
                await AddAssessmentAsync();
            }
        }

        public async Task AddAssessmentAsync()
        {
            // استفاده از IsBusy برای جلوگیری از کلیک تکراری
            if (IsBusy) return;

            // 1. اعتبارسنجی ساده
            if (FormValues.Count == 0)
            {
                await ShowInfo("هیچ سطری برای ثبت وجود ندارد.", InfoBarSeverity.Warning);
                return;
            }

            try
            {
                IsBusy = true; // قفل کردن UI

                var formValues = BuildFormValueModels();

                var success = await _assessmentService.AddAssessmentAsync(
                    Model,
                    formValues,
                    PumpStates,
                    Model.AssessmentEmployees);

                if (success)
                {
                    ResetToNewForm();
                    await ShowInfo("اندازه گیری جدید با موفقیت ثبت شد.", InfoBarSeverity.Success);
                }
                else
                {
                    await ShowInfo(_assessmentService.GetLastErrorMessage(), InfoBarSeverity.Error);
                }
            }
            catch (Exception ex)
            {
                await ShowInfo($"خطا در ثبت اطلاعات: {ex.Message}", InfoBarSeverity.Error);
            }
            finally
            {
                IsBusy = false; // آزاد کردن UI
            }
        }

        /// <summary>
        /// منطق ویرایش رکورد موجود (Update)
        /// </summary>
        private async Task EditAssessmentAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var formValues = BuildFormValueModels();

                var success = await _assessmentService.UpdateAssessmentAsync(
                    Model,
                    formValues,
                    PumpStates,
                    AssessmentEmployees,
                    Model.GateOpenings ?? new List<AssessmentGate>());
                if (success)
                {
                    ResetToNewForm();
                    await ShowInfo("ویرایش با موفقیت انجام شد.", InfoBarSeverity.Success);
                }
                else
                {
                    await ShowInfo(_assessmentService.GetLastErrorMessage(), InfoBarSeverity.Error);
                }
            }
            catch (Exception ex)
            {
                await ShowInfo($"خطا در ویرایش اطلاعات: {ex.Message}", InfoBarSeverity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ShowInfo(string message, InfoBarSeverity severity, int durationSeconds = 4)
        {
            InfoMessage = message;
            InfoSeverity = severity;
            IsInfoOpen = true;

            await Task.Delay(durationSeconds * 1000);

            if (InfoMessage == message)
            {
                IsInfoOpen = false;
            }
        }

        private List<FormValue> BuildFormValueModels()
        {
            foreach (var vm in FormValues)
            {
                vm.Model.SectionWidth = vm.SectionWidth;
                vm.Model.SectionArea = vm.SectionArea;
                vm.Model.SectionFlow = vm.SectionFlow;
                vm.Model.VerticalMeanVelocity = vm.VerticalMeanVelocity;
            }

            return FormValues.Select(vm => vm.Model).ToList();
        }

        /// <summary>
        /// افزودن سطر جدید به صورت داینامیک در UI
        /// </summary>
        [RelayCommand]
        private async Task AddRowAsync()
        {
            if (SelectedPropeller == null)
            {
                await ShowInfo("لطفاً پروانه مورد نظر را انتخاب کنید.", InfoBarSeverity.Warning);
                return;
            }

            // 1. ساخت مدل جدید
            var newModel = new FormValue
            {
                AssessmentID = this.Model.AssessmentID,
                // مقداردهی اولیه زمان با تایمر هدر
                MeasureTime = this.Timer
            };

            // 2. ساخت ویومدل فرزند
            var newVM = _formValueViewModelFactory.Create(newModel, SelectedPropeller);

            // پیشنهاد هوشمندانه فاصله:
            // اگر ردیف قبلی وجود دارد، فاصله جدید را مثلاً 1 متر بعد از آن پیشنهاد بده

            if (FormValues.Any())
            {
                newVM.Distance = FormValues.Last().Distance + 1; // مقدار پیشنهادی
            }

            // محاسبه ایندکس ردیف
            newVM.Model.RowIndex = FormValues.Count + 1;

            // 3. اشتراک در تغییرات این فرزند (بسیار مهم)
            SubscribeToChildEvents(newVM);

            // 4. افزودن به لیست
            FormValues.Add(newVM);

            // 5. محاسبه مجدد عرض‌ها و دبی کل
            RecalculateGeometryAndFlow();
        }

        /// <summary>
        /// حذف سطر انتخاب شده
        /// </summary>
        [RelayCommand]
        private void RemoveRow(FormValueViewModel row)
        {
            if (row == null) return;

            // حذف اشتراک رویداد برای جلوگیری از Memory Leak
            row.PropertyChanged -= OnChildRowPropertyChanged;

            FormValues.Remove(row);

            // اصلاح شماره ردیف‌ها (اختیاری)
            for (int i = 0; i < FormValues.Count; i++)
            {
                FormValues[i].Model.RowIndex = i + 1;
            }

            // محاسبه مجدد بعد از حذف
            RecalculateGeometryAndFlow();
        }

        [RelayCommand]
        private void AddEmployee()
        {
            if (SelectedEmployeeToAdd == null || AssessmentEmployees.Count >= 3) return;
            if (AssessmentEmployees.Any(x => x.EmployeeID == SelectedEmployeeToAdd.EmployeeID)) return;

            var rel = new Assessment_Employee
            {
                AssessmentID = Model.AssessmentID,
                EmployeeID = SelectedEmployeeToAdd.EmployeeID,
                Employee = SelectedEmployeeToAdd
            };
            Model.AssessmentEmployees.Add(rel);
            AssessmentEmployees.Add(rel);
            SelectedEmployeeToAdd = null;
        }

        [RelayCommand]
        private void RemoveEmployee(Assessment_Employee item)
        {
            Model.AssessmentEmployees.Remove(item);
            AssessmentEmployees.Remove(item);
        }

        [RelayCommand]
        public void ResetToNewForm()
        {
            // 1. تغییر وضعیت مدل به حالت "جدید" (ID صفر شود)
            Model.AssessmentID = 0;

            // 2. ریست کردن فیلدهای اصلی
            SelectedLocation = null;
            SelectedPropeller = null;
            SelectedCurrentMeter = null;
            SelectedEmployeeToAdd = null;

            Timer = 50;
            Date = DateTime.Now;
            Echelon = null;

            // 3. پاک کردن لیست‌ها
            AssessmentEmployees.Clear();
            Model.AssessmentEmployees.Clear();

            GateValues.Clear();
            Model.GateOpenings.Clear();

            PumpStates.Clear();
            Model.PumpStates.Clear();
            AvailablePumps.Clear();
            OnPropertyChanged(nameof(HasPumps));

            //  ایجاد سطرهای پیش‌فرض (خالی)
            InitializeDefaultRows(6);

            //  اطلاع‌رسانی به UI برای تغییر دکمه‌ها و وضعیت
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(SaveButtonContent));

            // بستن پیام‌های قبلی
            IsInfoOpen = false;
        }

        // ==========================================================
        // منطق هماهنگی (Coordination Logic)
        // ==========================================================

        private void SubscribeToChildEvents(FormValueViewModel child)
        {
            // گوش دادن به تغییرات هر سلول در گرید
            child.PropertyChanged += OnChildRowPropertyChanged;
        }

        private void OnChildRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // لیست مواردی که اگر تغییر کردند، باید دبی کل دوباره محاسبه شود
            if (e.PropertyName == nameof(FormValueViewModel.Distance) ||
                e.PropertyName == nameof(FormValueViewModel.TotalDepth) ||
                e.PropertyName == nameof(FormValueViewModel.VerticalMeanVelocity))
            {
                RecalculateGeometryAndFlow();
            }
        }

        /// <summary>
        /// محاسبه هوشمند تمام عرض‌ها و دبی کل
        /// این متد قلب تپنده فرم است!
        /// </summary>
        /// <summary>
        /// محاسبه دبی به روش Mean-Section (میانگین‌گیری بین دو خط)
        /// </summary>
        private void RecalculateGeometryAndFlow()
        {
            if (!FormValues.Any()) { TotalFlow = 0; return; }

            // مرتب‌سازی بر اساس فاصله
            var sortedRows = FormValues.OrderBy(x => x.Distance).ToList();
            double sumFlow = 0;

            for (int i = 0; i < sortedRows.Count; i++)
            {
                var current = sortedRows[i];

                if (i == 0)
                {
                    // سطر اول: چون سطر قبلی ندارد، مقادیر پنل خالی/صفر هستند
                    current.SectionWidth = 0;
                    current.SectionArea = 0;
                    current.SectionFlow = 0;
                    current.PanelAvgVelocity = 0; // <--- مهم: برای سطر اول مقدار 0 یا نامعتبر می‌گذاریم
                }
                else
                {
                    var prev = sortedRows[i - 1];

                    // 1. محاسبه عرض پنل
                    double w = current.Distance - prev.Distance;

                    // 2. محاسبه میانگین عمق
                    double d_avg = (current.TotalDepth + prev.TotalDepth) / 2.0;

                    // 3. محاسبه میانگین سرعت (درخواست شما)
                    // فرمول: (سرعت قائم سطر قبل + سرعت قائم سطر جاری) تقسیم بر 2
                    double v_avg = (current.VerticalMeanVelocity + prev.VerticalMeanVelocity) / 2.0;

                    // 4. محاسبه سطح و دبی
                    double area = w * d_avg;
                    double flow = area * v_avg;

                    // 5. ذخیره در سطر جاری برای نمایش
                    current.SectionWidth = w;
                    current.PanelAvgDepth = d_avg;
                    current.PanelAvgVelocity = v_avg; // <--- ذخیره سرعت میانگین پنل
                    current.SectionArea = area;
                    current.SectionFlow = flow;

                    sumFlow += flow;
                }
            }

            TotalFlow = sumFlow;
            Model.TotalFlow = sumFlow;
        }

        // این متد را به کلاس AssessmentViewModel اضافه کنید
        private void InitializeDefaultRows(int count = 6)
        {
            // پاک کردن سطرهای موجود
            FormValues.Clear();
            // پاک کردن از مدل برای اطمینان
            if (Model.FormValues == null) Model.FormValues = new List<FormValue>();
            Model.FormValues.Clear();

            for (int i = 0; i < count; i++)
            {
                var newModel = new FormValue
                {
                    AssessmentID = Model.AssessmentID, // اگر آیدی 0 باشد مشکلی نیست
                    RowIndex = i + 1,
                    MeasureTime = this.Timer,
                    Distance = i, // فاصله پیش‌فرض
                    TotalDepth = 0
                };

                var newVM = _formValueViewModelFactory.Create(newModel, SelectedPropeller);
                SubscribeToChildEvents(newVM); // اشتراک در رویدادها برای محاسبات
                FormValues.Add(newVM);
            }

            // محاسبه مجدد برای اینکه جمع کل صفر شود
            RecalculateGeometryAndFlow();
        }
    }
}
