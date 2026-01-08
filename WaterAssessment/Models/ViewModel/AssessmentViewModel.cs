using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Globalization;
using WinUICommunity;

namespace WaterAssessment.Models.ViewModel
{
    public partial class AssessmentViewModel : ObservableObject
    {
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
            UpdateGateOpenings(value.GateCount);
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
        public string SaveButtonContent => IsEditMode ? "ثبت تغییرات (ویرایش)" : "ثبت اندازه گیری جدید";

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

        public AssessmentViewModel(Assessment model)
        {
            Model = model;
            SelectedPropeller = model.Propeller;

            // مقداردهی اولیه فیلد‌ها از مدل
            _timer = model.Timer == 0 ? 50 : model.Timer; // پیش‌فرض 50
            _date = model.Date == default ? DateTime.Now : model.Date;
            _echelon = model.Echelon;
            _totalFlow = model.TotalFlow;

            // لود کردن کارمندان انتخاب شده از دیالوگ
            foreach (var rel in model.AssessmentEmployees) AssessmentEmployees.Add(rel);

            // اگر مدل شامل ردیف‌های ذخیره شده قبلی است، آن‌ها را لود کن
            if (model.FormValues != null)
            {
                foreach (var fv in model.FormValues)
                {
                    var vm = new FormValueViewModel(fv, SelectedPropeller);
                    SubscribeToChildEvents(vm); // اشتراک در رویدادها
                    FormValues.Add(vm);
                }
                // یک بار محاسبات کلی را انجام بده (برای اطمینان از صحت مقادیر لود شده)
                RecalculateGeometryAndFlow();
            }

            // لود کردن داده‌های مرجع (مکان‌ها و...) از دیتابیس
            LoadReferenceData();
        }

        private void LoadReferenceData()
        {
            using var db = new WaterAssessmentContext();

            // 1. لود کردن لیست‌ها
            var locations = db.Locations.AsNoTracking().ToList();
            var propellers = db.Propellers.AsNoTracking().ToList();
            var meters = db.CurrentMeters.AsNoTracking().ToList();
            var employees = db.Employees.AsNoTracking().ToList();

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
            if (Model.GateOpenings != null && Model.GateOpenings.Any())
            {
                foreach (var g in Model.GateOpenings) GateValues.Add(g);
            }
            else if (SelectedLocation != null)
            {
                // اگر جدید است، بر اساس مکان پیش‌فرض بساز
                UpdateGateOpenings(SelectedLocation.GateCount);
            }
        }

        private void UpdateGateOpenings(int count)
        {
            // اگر تعداد فعلی با تعداد درخواستی یکی است، دست نزن (تا مقادیر وارد شده نپرد)
            if (GateValues.Count == count) return;

            // بازسازی لیست دریچه‌ها
            var currentValues = GateValues.ToList(); // کپی مقادیر فعلی
            GateValues.Clear();

            // همگام‌سازی با مدل
            if (Model.GateOpenings == null) Model.GateOpenings = new List<AssessmentGate>();
            Model.GateOpenings.Clear();

            for (int i = 1; i <= count; i++)
            {
                // اگر قبلاً مقداری داشتیم حفظ کن، وگرنه صفر
                var existing = currentValues.FirstOrDefault(g => g.GateNumber == i);
                var newVal = existing ?? new AssessmentGate { GateNumber = i, Value = 0, AssessmentID = Model.AssessmentID };

                GateValues.Add(newVal);
                Model.GateOpenings.Add(newVal);
            }
        }

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
            if (IsEditMode)
            {
                // اگر حالت ویرایش است، متد Update را صدا بزن
                // (که باید آن را پیاده‌سازی کنید یا اگر فعلا ندارید خالی بگذارید)
                await EditAssessmentAsync();
            }
            else
            {
                // اگر حالت جدید است، متد Add را صدا بزن
                await AddAssessmentAsync();
            }
        }

        /// <summary>
        /// تنها وظیفه این متد، درج یک رکورد کاملاً جدید در دیتابیس است.
        /// </summary>
        public async Task AddAssessmentAsync()
        {
            // استفاده از IsBusy برای جلوگیری از کلیک تکراری
            if (IsBusy) return;

            // 1. اعتبارسنجی ساده
            if (FormValues.Count == 0)
            {
                ShowInfo("هیچ سطری برای ثبت وجود ندارد.", InfoBarSeverity.Warning);
                return;
            }

            try
            {
                IsBusy = true; // قفل کردن UI
                using var db = new WaterAssessmentContext();

                // 2. آماده‌سازی مدل برای درج
                // نکته مهم: چون Location و Propeller قبلاً در دیتابیس هستند،
                // باید آبجکت آن‌ها را نال کنیم تا EF تلاش نکند دوباره آن‌ها را بسازد.
                // EF فقط با ID ها (LocationID, PropellerID) کار خواهد کرد.
                Model.Location = null;
                Model.Propeller = null;
                Model.CurrentMeter = null;

                // قطع رابطه شیئی کارمندان (فقط ID مهم است)
                foreach (var ae in Model.AssessmentEmployees)
                {
                    ae.Employee = null;
                }

                // همگام‌سازی لیست سطرها (مطمئن شویم چیزی که در گرید است ذخیره می‌شود)
                Model.FormValues = FormValues.Select(vm => vm.Model).ToList();

                // 3. درج در کانتکست
                db.Assessments.Add(Model);

                // 4. ذخیره نهایی در دیتابیس
                await db.SaveChangesAsync();

                ShowInfo("اندازه گیری جدید با موفقیت ثبت شد.", InfoBarSeverity.Success);

                // اختیاری: پاک کردن فرم برای ثبت بعدی
                // ClearForm(); 
            }
            catch (Exception ex)
            {
                ShowInfo($"خطا در ثبت اطلاعات: {ex.Message}", InfoBarSeverity.Error);
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
                using var db = new WaterAssessmentContext();

                // 1. لود کردن رکورد موجود از دیتابیس همراه با تمام جزئیات
                // استفاده از Include حیاتی است تا بتوانیم فرزندان را ویرایش کنیم
                var existing = await db.Assessments
                    .Include(a => a.FormValues)
                    .Include(a => a.AssessmentEmployees)
                    .Include(a => a.GateOpenings)
                    .FirstOrDefaultAsync(a => a.AssessmentID == Model.AssessmentID);

                if (existing == null)
                {
                    ShowInfo("این رکورد در دیتابیس یافت نشد (شاید قبلاً حذف شده است).", InfoBarSeverity.Error);
                    return;
                }

                // 2. آپدیت فیلدهای اصلی (Header)
                existing.Date = Model.Date;
                existing.Timer = Model.Timer;
                existing.Echelon = Model.Echelon;
                existing.TotalFlow = Model.TotalFlow;

                // آپدیت کلیدهای خارجی (اگر کاربر پروانه یا مولینه را عوض کرده باشد)
                existing.PropellerID = Model.PropellerID;
                existing.CurrentMeterID = Model.CurrentMeterID;

                // نکته: معمولاً LocationID در ویرایش تغییر نمی‌کند، اما اگر لازم است:
                // existing.LocationID = Model.LocationID;

                // 3. آپدیت سطرها (FormValues)
                foreach (var vm in FormValues)
                {
                    if (vm.Model.FormValueID == 0)
                    {
                        // حالت الف: سطر جدید است (کاربر دکمه افزودن سطر را زده)
                        // باید ID پدر را ست کنیم و به لیست اضافه کنیم
                        vm.Model.AssessmentID = existing.AssessmentID;
                        existing.FormValues.Add(vm.Model);
                    }
                    else
                    {
                        // حالت ب: سطر قدیمی است (باید مقادیرش آپدیت شود)
                        var dbRow = existing.FormValues.FirstOrDefault(r => r.FormValueID == vm.Model.FormValueID);
                        if (dbRow != null)
                        {
                            // کپی مقادیر از ویومدل به سطر دیتابیس
                            dbRow.Distance = vm.Distance;
                            dbRow.TotalDepth = vm.TotalDepth;
                            dbRow.RowIndex = vm.RowIndex;
                            dbRow.MeasureTime = vm.MeasureTime;

                            // نکته مهم: خواندن دورها از vm.Model برای جلوگیری از خطای تبدیل نوع (double? به int?)
                            dbRow.Rev02 = vm.Model.Rev02;
                            dbRow.Rev06 = vm.Model.Rev06;
                            dbRow.Rev08 = vm.Model.Rev08;

                            // ذخیره نتایج محاسباتی
                            dbRow.SectionWidth = vm.SectionWidth;
                            dbRow.SectionArea = vm.SectionArea;
                            dbRow.SectionFlow = vm.SectionFlow;

                            // ذخیره سرعت میانگین (ممکن است فرمولش تغییر کرده باشد)
                            // dbRow.VerticalMeanVelocity = vm.VerticalMeanVelocity; // اگر این فیلد را در دیتابیس دارید
                        }
                    }
                }

                // 4. آپدیت تیم اندازه گیری (AssessmentEmployees)
                // استراتژی: حذف همه روابط قبلی و درج روابط جدید
                db.AssessmentEmployees.RemoveRange(existing.AssessmentEmployees);
                foreach (var ae in AssessmentEmployees)
                {
                    existing.AssessmentEmployees.Add(new Assessment_Employee
                    {
                        AssessmentID = existing.AssessmentID,
                        EmployeeID = ae.EmployeeID
                        // Employee = null // نیازی نیست چون فقط با ID کار داریم
                    });
                }

                // 5. آپدیت گشودگی دریچه‌ها (GateOpenings)
                // استراتژی: حذف همه و درج مجدد
                // (فرض بر این است که DbSet مربوطه در کانتکست AssessmentGates نام دارد)
                db.AssessmentGates.RemoveRange(existing.GateOpenings);

                if (Model.GateOpenings != null)
                {
                    foreach (var gate in Model.GateOpenings)
                    {
                        existing.GateOpenings.Add(new AssessmentGate
                        {
                            AssessmentID = existing.AssessmentID,
                            GateNumber = gate.GateNumber,
                            Value = gate.Value
                        });
                    }
                }

                // 6. ذخیره نهایی تغییرات
                await db.SaveChangesAsync();

                ShowInfo("ویرایش با موفقیت انجام شد.", InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                ShowInfo($"خطا در ویرایش اطلاعات: {ex.Message}", InfoBarSeverity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ShowInfo(string msg, InfoBarSeverity severity)
        {
            InfoMessage = msg;
            InfoSeverity = severity;
            IsInfoOpen = true;
        }

        /// <summary>
        /// افزودن سطر جدید به صورت داینامیک در UI
        /// </summary>
        [RelayCommand]
        private void AddRow()
        {
            if (SelectedPropeller == null) return; // یا هندل کردن خطا

            // 1. ساخت مدل جدید
            var newModel = new FormValue
            {
                AssessmentID = this.Model.AssessmentID,
                // مقداردهی اولیه زمان با تایمر هدر
                MeasureTime = this.Timer
            };

            // 2. ساخت ویومدل فرزند
            var newVM = new FormValueViewModel(newModel, SelectedPropeller);

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
    }
}
