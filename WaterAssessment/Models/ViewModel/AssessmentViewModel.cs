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

        private readonly List<Employee> _allEmployeesDataSource; // برای افزودن نفر جدید

        // لیست ردیف‌های اندازه‌گیری (فرزندان)
        public ObservableCollection<FormValueViewModel> FormValues { get; } = new();
        public ObservableCollection<Assessment_Employee> AssessmentEmployees { get; } = new();
        public ObservableCollection<Employee> AllEmployees { get; } = new();


        // پروانه انتخاب شده (برای پاس دادن به فرزندان جهت محاسبه سرعت)
        [ObservableProperty] private Propeller _selectedPropeller;
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

        public AssessmentViewModel(Assessment model, List<Employee> allEmployees)
        {
            Model = model;
            _allEmployeesDataSource = allEmployees;
            SelectedPropeller = model.Propeller;

            // مقداردهی اولیه پراپرتی‌ها از مدل
            _timer = model.Timer;
            _date = model.Date;
            _echelon = model.Echelon;

            _totalFlow = model.TotalFlow;

            foreach (var emp in allEmployees) AllEmployees.Add(emp);
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
        public string TotalFlowDisplay => TotalFlow.ToString("N3"); // نمایش با 3 رقم اعشار

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


        // دبی کل (نتیجه محاسبات)
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalFlowDisplay))]
        private double _totalFlow;


        // ==========================================================
        // مدیریت ردیف‌ها (CRUD در UI)
        // ==========================================================
        // 3. متد هوشمندی که تصمیم می‌گیرد کدام تابع را صدا بزند
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

        //[RelayCommand]
        //private async Task SaveAssessmentAsync()
        //{
        //    if (IsBusy) return;

        //    if (FormValues.Count == 0)
        //    {
        //        ShowInfo("هیچ سطری برای محاسبه ثبت نشده است.", InfoBarSeverity.Warning);
        //        return;
        //    }

        //    try
        //    {
        //        IsBusy = true;
        //        using var db = new WaterAssessmentContext();

        //        if (Model.AssessmentID == 0)
        //        {
        //            await InsertNewAssessment(db);
        //        }
        //        else
        //        {
        //            await UpdateExistingAssessment(db);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowInfo($"خطا در ذخیره سازی: {ex.Message}", InfoBarSeverity.Error);
        //    }
        //    finally
        //    {
        //        IsBusy = false;
        //    }
        //}

        //private async Task InsertNewAssessment(WaterAssessmentContext db)
        //{
        //    // جلوگیری از درج تکراری اشیاء وابسته
        //    Model.Location = null;
        //    Model.Propeller = null;
        //    Model.CurrentMeter = null;

        //    foreach (var ae in Model.AssessmentEmployees) ae.Employee = null;

        //    // اطمینان از اینکه مدل با ویومدل یکی است
        //    Model.FormValues = FormValues.Select(vm => vm.Model).ToList();

        //    db.Assessments.Add(Model);
        //    await db.SaveChangesAsync();

        //    ShowInfo("اندازه گیری با موفقیت ثبت شد.", InfoBarSeverity.Success);
        //}

        //private async Task UpdateExistingAssessment(WaterAssessmentContext db)
        //{
        //    var existing = await db.Assessments
        //        .Include(a => a.FormValues)
        //        .Include(a => a.AssessmentEmployees)
        //        .Include(a => a.GateOpenings)
        //        .FirstOrDefaultAsync(a => a.AssessmentID == Model.AssessmentID);

        //    if (existing == null)
        //    {
        //        ShowInfo("رکورد یافت نشد.", InfoBarSeverity.Error);
        //        return;
        //    }

        //    // 1. آپدیت فیلدها
        //    existing.Date = Model.Date;
        //    existing.Timer = Model.Timer;
        //    existing.Echelon = Model.Echelon;
        //    existing.TotalFlow = Model.TotalFlow;
        //    existing.PropellerID = Model.PropellerID;
        //    existing.CurrentMeterID = Model.CurrentMeterID;

        //    // 2. آپدیت سطرها
        //    // (چون دکمه حذف را برداشتید، اینجا عملاً فقط سطرهای جدید اضافه می‌شوند یا قدیمی‌ها آپدیت می‌شوند)
        //    foreach (var vm in FormValues)
        //    {
        //        if (vm.Model.FormValueID == 0)
        //        {
        //            vm.Model.AssessmentID = existing.AssessmentID;
        //            existing.FormValues.Add(vm.Model);
        //        }
        //        else
        //        {
        //            var dbRow = existing.FormValues.FirstOrDefault(r => r.FormValueID == vm.Model.FormValueID);
        //            if (dbRow != null)
        //            {
        //                // کپی مقادیر
        //                dbRow.Distance = vm.Distance;
        //                dbRow.TotalDepth = vm.TotalDepth;
        //                dbRow.RowIndex = vm.RowIndex;
        //                dbRow.Rev02 = vm.Model.Rev02;
        //                dbRow.Rev06 = vm.Model.Rev06;
        //                dbRow.Rev08 = vm.Model.Rev08;
        //                dbRow.MeasureTime = vm.MeasureTime;
        //                dbRow.SectionWidth = vm.SectionWidth;
        //                dbRow.SectionArea = vm.SectionArea;
        //                dbRow.SectionFlow = vm.SectionFlow;
        //            }
        //        }
        //    }

        //    // 3. آپدیت تیم (جایگزینی کامل لیست)
        //    db.AssessmentEmployees.RemoveRange(existing.AssessmentEmployees);
        //    foreach (var ae in AssessmentEmployees)
        //    {
        //        existing.AssessmentEmployees.Add(new Assessment_Employee
        //        {
        //            AssessmentID = existing.AssessmentID,
        //            EmployeeID = ae.EmployeeID
        //        });
        //    }

        //    // 4. آپدیت گشودگی‌ها (جایگزینی کامل)
        //    db.AssessmentGates.RemoveRange(existing.GateOpenings);
        //    if (Model.GateOpenings != null)
        //    {
        //        foreach (var gate in Model.GateOpenings)
        //        {
        //            existing.GateOpenings.Add(new AssessmentGate
        //            {
        //                GateNumber = gate.GateNumber,
        //                Value = gate.Value,
        //                AssessmentID = existing.AssessmentID
        //            });
        //        }
        //    }

        //    await db.SaveChangesAsync();
        //    ShowInfo("ویرایش انجام شد.", InfoBarSeverity.Success);
        //}

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
            if (!FormValues.Any())
            {
                TotalFlow = 0;
                return;
            }

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

        private void UpdateTotalFlowOnly()
        {
            double sum = 0;
            foreach (var row in FormValues)
            {
                sum += row.SectionFlow;
            }

            TotalFlow = sum;
            Model.TotalFlow = sum; // ذخیره در مدل اصلی
        }
    }
}
