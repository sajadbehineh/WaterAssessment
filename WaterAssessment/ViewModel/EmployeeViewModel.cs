using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WaterAssessment.Services;

namespace WaterAssessment.ViewModel
{
    public partial class EmployeeViewModel : PagedViewModelBase<Employee>
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<Employee> Employees => PagedItems;
        public int TotalEmployees => TotalItems;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddEmployeeCommand))]
        private Employee? _selectedEmployee;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddEmployeeCommand))]
        private string _firstName = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddEmployeeCommand))]
        private string _lastName = string.Empty;

        [ObservableProperty] private bool _isErrorVisible;
        [ObservableProperty] private InfoBarSeverity _infoBarSeverity;
        [ObservableProperty] private string _infoBarMessage = string.Empty;
        [ObservableProperty] private string _addEditBtnContent = "ذخیره";

        public EmployeeViewModel(IEmployeeService employeeService, IDialogService dialogService) : base(pageSize: 10)
        {
            _employeeService = employeeService;
            _dialogService = dialogService;

            _ = LoadEmployeesAsync();
        }

        private bool CanAddEmployee()
        {
            // دکمه فقط زمانی فعال باشد که نام و فامیل پر شده باشند
            return !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName);
        }

        [RelayCommand(CanExecute = nameof(CanAddEmployee))]
        private async Task AddEmployee()
        {
            if (!ValidateInput()) return;

            bool success;
            if (SelectedEmployee == null)
            {
                success = await _employeeService.AddNewEmployeeAsync(FirstName, LastName);
            }
            else
            {
                success = await _employeeService.UpdateEmployeeAsync(SelectedEmployee.EmployeeID, FirstName, LastName);
            }

            if (success)
            {
                ClearForm();
                await LoadEmployeesAsync(); // Refresh list after operation
                await ShowMessageAsync("عملیات با موفقیت انجام شد.", InfoBarSeverity.Success);
            }
            else
            {
                await ShowMessageAsync(_employeeService.GetLastErrorMessage(), InfoBarSeverity.Error);
            }
        }

        [RelayCommand]
        private async Task RequestDeleteEmployeeAsync(Employee employee)
        {
            if (employee == null) return;

            // از سرویس دیالوگ برای نمایش پیغام تایید استفاده کنید
            bool confirmed = await _dialogService.ShowConfirmationDialogAsync(
                title: "تأیید عملیات حذف",
                content: $"آیا از حذف کارمند «{employee.LastName}» اطمینان دارید؟\nاین عملیات غیرقابل بازگشت است.",
                primaryButtonText: "بله، حذف کن",
                closeButtonText: "خیر"
            );

            // فقط در صورت تایید کاربر، حذف را ادامه دهید
            if (confirmed)
            {
                await DeleteEmployee(employee);
            }
        }

        private async Task DeleteEmployee(Employee employee)
        {
            var success = await _employeeService.DeleteEmployeeAsync(employee.EmployeeID);
            if (success)
            {
                if (SelectedEmployee?.EmployeeID == employee.EmployeeID) ClearForm();
                await LoadEmployeesAsync();
                await ShowMessageAsync("کارمند با موفقیت حذف شد.", InfoBarSeverity.Success);
            }
            else
            {
                await ShowMessageAsync(_employeeService.GetLastErrorMessage(), InfoBarSeverity.Warning);
            }
        }

        partial void OnSelectedEmployeeChanged(Employee? value)
        {
            if (value != null)
            {
                FirstName = value.FirstName;
                LastName = value.LastName;
                AddEditBtnContent = "ویرایش";
            }
            else
            {
                AddEditBtnContent = "ذخیره";
            }
        }

        [RelayCommand]
        private void ClearForm()
        {
            SelectedEmployee = null;
            FirstName = string.Empty;
            LastName = string.Empty;
            IsErrorVisible = false;
            InfoBarMessage = string.Empty;
        }

        private async Task LoadEmployeesAsync()
        {
            var employeesResult = await _employeeService.GetAllEmployeesAsync();
            SetItems(employeesResult);
            OnPropertyChanged(nameof(TotalEmployees));
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                _ = ShowMessageAsync("نام و نام خانوادگی نمی‌توانند خالی باشند.", InfoBarSeverity.Error);
                return false;
            }
            return true;
        }

        private async Task ShowMessageAsync(string message, InfoBarSeverity severity, int durationSeconds = 4)
        {
            InfoBarMessage = message;
            InfoBarSeverity = severity;
            IsErrorVisible = true;

            await Task.Delay(durationSeconds * 1000);

            // فقط اگر پیام فعلی همان پیامی است که نمایش داده بودیم، آن را ببند
            if (InfoBarMessage == message)
            {
                IsErrorVisible = false;
            }
        }
    }
}

