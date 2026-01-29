using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;
using RelayCommand = WaterAssessment.Core.RelayCommand;

namespace WaterAssessment.Models.ViewModel
{
    public class EmployeeViewModel : ObservableObject
    {
        public ObservableCollection<Employee> Employees { get; set; } = new();

        private Employee _selectedEmployee;
        public Employee? SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                if (SetProperty(ref _selectedEmployee, value))
                {
                    // وقتی ردیف انتخاب شد، نام و فامیل را در TextBoxها پر کن برای ویرایش
                    if (value != null)
                    {
                        FirstName = value.FirstName;
                        LastName = value.LastName;
                    }
                    (AddCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private int _employeeId;
        public int EmployeeId
        {
            get => _employeeId;
            set => SetProperty(ref _employeeId, value);
        }

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set
            {
                SetProperty(ref _firstName, value); //اگر ورودی مقدارش تکراری بود OnPropertyChanged اجرا نشود
                ((RelayCommand)AddCommand).RaiseCanExecuteChanged(); // بررسی فعال بودن دکمه
            }
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set
            {
                SetProperty(ref _lastName, value);
                ((RelayCommand)AddCommand).RaiseCanExecuteChanged();
            }
        }

        //  متغیر برای وضعیت اینفوبار نام
        private bool _isFirstNameErrorVisible;
        public bool IsFirstNameErrorVisible
        {
            get => _isFirstNameErrorVisible;
            set => SetProperty(ref _isFirstNameErrorVisible, value);
        }

        // 2. متغیر برای وضعیت اینفوبار نام خانوادگی
        private bool _isLastNameErrorVisible;
        public bool IsLastNameErrorVisible
        {
            get => _isLastNameErrorVisible;
            set => SetProperty(ref _isLastNameErrorVisible, value);
        }

        private InfoBarSeverity _infoBarSeverity;

        public InfoBarSeverity InfoBarSeverity
        {
            get => _infoBarSeverity;
            set => SetProperty(ref _infoBarSeverity, value);
        }

        private string _infoBarMessage;
        public string InfoBarMessage
        {
            get => _infoBarMessage;
            set => SetProperty(ref _infoBarMessage, value);
        }

        private string _addEditBtnContent = "ذخیره";
        public string AddEditBtnContent
        {
            get => _addEditBtnContent;
            set => SetProperty(ref _addEditBtnContent, value);
        }

        // ۳. دکمه‌ها (Commands)
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ClearCommand { get; }

        public EmployeeViewModel()
        {
            AddCommand = new RelayCommand(async (_) => await AddEmployeeAsync(), _ => CanAddEmployee());
            DeleteCommand = new WinUICommunity.RelayCommand<int>(async (id) => await DeleteEmployeeAsync(id));
            EditCommand = new WinUICommunity.RelayCommand<int>(PrepareForEdit);
            ClearCommand = new RelayCommand(_ => ClearForm());

            // بارگذاری اولیه لیست
            _ = LoadEmployeesAsync();
        }

        private bool CanAddEmployee()
        {
            // دکمه فقط زمانی فعال باشد که نام و فامیل پر شده باشند
            return !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName);
        }

        private async Task LoadEmployeesAsync()
        {
            using var db = new WaterAssessmentContext();
            var list = await db.Employees.AsNoTracking().ToListAsync();

            Employees.Clear();

            foreach (var item in list) Employees.Add(item);
        }

        private void PrepareForEdit(int employeeId)
        {
            // پیدا کردن کارمند از لیست موجود در حافظه
            var emp = Employees.FirstOrDefault(e => e.EmployeeID == employeeId);

            if (emp != null)
            {
                AddEditBtnContent = "ویرایش";
                // با ست کردن این مقدار، تکست‌باکس‌ها خودکار پر می‌شوند
                // (چون در Setter پراپرتی SelectedEmployee کد پر کردن FirstName/LastName را نوشته‌اید)
                SelectedEmployee = emp;

                // مدیریت وضعیت دکمه‌ها (مثلاً دکمه افزودن غیرفعال شود)
                ((RelayCommand)AddCommand).RaiseCanExecuteChanged();

                // بستن پیام‌های خطا برای شروع تمیز
                IsFirstNameErrorVisible = false;
                IsLastNameErrorVisible = false;
                InfoBarMessage = "";
            }
        }

        private async Task DeleteEmployeeAsync(int id)
        {
            // اگر ID نامعتبر بود خارج شو
            if (id <= 0) return;

            try
            {
                using var db = new WaterAssessmentContext();

                // 1. پیدا کردن رکورد در دیتابیس
                var empToDelete = await db.Employees.FindAsync(id);

                bool hasDependents = await db.AssessmentEmployees.AnyAsync(ae => ae.EmployeeID == id);

                if (hasDependents)
                {
                    ShowWarning("این کارمند دارای اندازه‌گیری‌های ثبت شده است و قابل حذف نیست.");
                    return;
                }

                if (empToDelete != null)
                {
                    // 2. حذف از دیتابیس
                    db.Employees.Remove(empToDelete);
                    await db.SaveChangesAsync();

                    // 3. حذف از لیست UI (بدون نیاز به لود مجدد کل لیست)
                    // پیدا کردن آیتم در لیست ObservableCollection
                    var empInList = Employees.FirstOrDefault(e => e.EmployeeID == id);
                    if (empInList != null)
                    {
                        Employees.Remove(empInList);
                    }

                    ShowSuccess("همکار با موفقیت حذف شد.");

                    // اگر آیتم حذف شده همان آیتمی بود که در حال ویرایشش بودیم، فرم را خالی کن
                    if (SelectedEmployee != null && SelectedEmployee.EmployeeID == id)
                    {
                        SelectedEmployee = null;
                        FirstName = LastName = string.Empty;
                        AddEditBtnContent = "ذخیره";
                    }
                }
                else
                {
                    ShowWarning("این رکورد قبلاً حذف شده یا وجود ندارد.");
                }
            }
            catch (Exception ex)
            {
                // مدیریت خطا
                InfoBarSeverity = InfoBarSeverity.Error;
                InfoBarMessage = $"خطا در حذف اطلاعات: {ex.Message}";
                IsFirstNameErrorVisible = true;
            }
        }

        private void ClearForm()
        {
            //خروج از حالت ویرایش 
            SelectedEmployee = null;

            FirstName = string.Empty;
            LastName = string.Empty;

            IsFirstNameErrorVisible = false;
            IsLastNameErrorVisible = false;
            InfoBarMessage = string.Empty;
            AddEditBtnContent = "ذخیره";

            // چون در Setter های FirstName/LastName دستور NotifyCanExecuteChanged را برای AddCommand نوشتیم،
            // دکمه افزودن/ویرایش خود به خود غیرفعال می‌شود.
        }

        private async Task AddEmployeeAsync()
        {
            // ۱. اعتبارسنجی اولیه (مشترک بین افزودن و ویرایش)
            IsFirstNameErrorVisible = false;
            IsLastNameErrorVisible = false;
            InfoBarMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FirstName))
            {
                IsFirstNameErrorVisible = true;
                InfoBarSeverity = InfoBarSeverity.Error;
                return; // همینجا خارج شویم بهتر است تا فلگ hasError
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                IsLastNameErrorVisible = true;
                InfoBarSeverity = InfoBarSeverity.Error;
                return;
            }

            // ۲. تشخیص حالت افزودن یا ویرایش
            // به جای چک کردن متن دکمه، از وضعیت آبجکت استفاده می‌کنیم
            if (SelectedEmployee == null)
            {
                await InsertNewEmployeeAsync();
            }
            else
            {
                await UpdateExistingEmployeeAsync();
            }
        }

        // متد کمکی برای افزودن (Insert)
        private async Task InsertNewEmployeeAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();

                // پرفورمنس: استفاده از AnyAsync بجای FirstOrDefault (بسیار سبک‌تر)
                // چک می‌کنیم آیا کسی با این نام و فامیل وجود دارد؟
                bool exists = await db.Employees.AnyAsync(e => e.FirstName == FirstName && e.LastName == LastName);

                if (exists)
                {
                    ShowError("همکار مورد نظر قبلاً ثبت شده است.");
                    return;
                }

                var newEmp = new Employee
                {
                    FirstName = FirstName,
                    LastName = LastName
                };

                db.Employees.Add(newEmp);
                await db.SaveChangesAsync();

                // آپدیت UI
                Employees.Add(newEmp);
                ClearForm(); // متد پاکسازی که خودتان داشتید

                ShowSuccess("همکار جدید با موفقیت ثبت شد.");
            }
            catch (Exception e)
            {
                HandleException(e, "ثبت");
            }
        }

        // متد کمکی برای ویرایش (Update)
        private async Task UpdateExistingEmployeeAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();

                // پیدا کردن رکورد
                var empToEdit = await db.Employees.FindAsync(SelectedEmployee.EmployeeID);

                if (empToEdit == null)
                {
                    ShowError("این رکورد یافت نشد (شاید حذف شده است).");
                    return;
                }

                // پرفورمنس: چک تکراری بودن هنگام ویرایش (اختیاری ولی توصیه می‌شود)
                // می‌گوییم: اگر رکوردی هست که نامش این است ولی ID آن، ID من نیست (یعنی شخص دیگری است)
                bool isDuplicate = await db.Employees.AnyAsync(e =>
                    e.FirstName == FirstName &&
                    e.LastName == LastName &&
                    e.EmployeeID != SelectedEmployee.EmployeeID);

                if (isDuplicate)
                {
                    ShowError("نام و نام خانوادگی تکراری است.");
                    return;
                }

                // اعمال تغییرات
                empToEdit.FirstName = FirstName;
                empToEdit.LastName = LastName;

                await db.SaveChangesAsync();

                // آپدیت لیست UI (تکنیک جایگزینی برای رفرش شدن لیست)
                SelectedEmployee.FirstName = FirstName;
                SelectedEmployee.LastName = LastName;

                int index = Employees.IndexOf(SelectedEmployee);
                if (index != -1) Employees[index] = SelectedEmployee;

                // نمایش پیام موفقیت و پاکسازی
                ShowSuccess("ویرایش با موفقیت انجام شد.");

                ClearForm();
            }
            catch (Exception e)
            {
                HandleException(e, "ویرایش");
            }
        }

        // متد کمکی برای مدیریت خطا
        private void HandleException(Exception ex, string operation)
        {
            IsFirstNameErrorVisible = true;
            InfoBarSeverity = InfoBarSeverity.Error; // معمولا خطا Warning نیست، Error است
            InfoBarMessage = $"خطا در {operation}: {ex.Message}";
        }

        private async void ShowError(string message, int durationSeconds = 5)
        {
            InfoBarMessage = message;
            InfoBarSeverity = InfoBarSeverity.Error;
            IsFirstNameErrorVisible = true;
            await Task.Delay(durationSeconds * 1000);

            // بستن اینفوبار
            IsFirstNameErrorVisible = false;
        }

        private async void ShowSuccess(string message, int durationSeconds = 5)
        {
            InfoBarMessage = message;
            InfoBarSeverity = InfoBarSeverity.Success;
            IsFirstNameErrorVisible = true;
            await Task.Delay(durationSeconds * 1000);

            IsFirstNameErrorVisible = false;
        }

        private async void ShowWarning(string message, int durationSeconds = 5)
        {
            InfoBarMessage = message;
            InfoBarSeverity = InfoBarSeverity.Warning;
            IsFirstNameErrorVisible = true;
            await Task.Delay(durationSeconds * 1000);
            IsFirstNameErrorVisible = false;
        }
    }
}

