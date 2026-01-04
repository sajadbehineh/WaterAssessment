using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;

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
        [ObservableProperty]
        private Propeller _selectedPropeller;
        [ObservableProperty] private Employee _selectedEmployeeToAdd;

        public AssessmentViewModel(Assessment model, List<Employee> allEmployees)
        {
            Model = model;
            _allEmployeesDataSource = allEmployees;
            SelectedPropeller = model.Propeller;

            // مقداردهی اولیه پراپرتی‌ها از مدل
            Timer = model.Timer;
            _date = model.Date;
            _echelon = model.Echelon;
            _openness = model.Openness;
            _isCanal = model.IsCanal;
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
        // پراپرتی‌های هدر (Assessment)
        // ==========================================================

        [ObservableProperty]
        private int _timer;
        partial void OnTimerChanged(int value)
        {
            Model.Timer = value;

            foreach (var row in FormValues)
            {
                row.MeasureTime = value;
            }
        }

        [ObservableProperty] private DateTime _date;
        partial void OnDateChanged(DateTime value) => Model.Date = value;

        [ObservableProperty] private double? _echelon;
        partial void OnEchelonChanged(double? value) => Model.Echelon = value;

        [ObservableProperty] private double? _openness;
        partial void OnOpennessChanged(double? value) => Model.Openness = value;

        [ObservableProperty] private bool _isCanal;
        partial void OnIsCanalChanged(bool value) => Model.IsCanal = value;

        // دبی کل (نتیجه محاسبات)
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalFlowDisplay))]
        private double _totalFlow;
        public string TotalFlowDisplay => _totalFlow.ToString("N3"); // نمایش با 3 رقم اعشار

        // ==========================================================
        // مدیریت ردیف‌ها (CRUD در UI)
        // ==========================================================

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
