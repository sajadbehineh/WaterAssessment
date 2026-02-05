using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WaterAssessment.Services;

namespace WaterAssessment.Models.ViewModel
{
    public partial class EmployeeViewModel : ObservableObject
    {
        private readonly IEmployeeService _employeeService;
        public ObservableCollection<Employee> Employees { get; } = new();

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

        public EmployeeViewModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;

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
        private async Task DeleteEmployee(int employeeId)
        {
            var success = await _employeeService.DeleteEmployeeAsync(employeeId);
            if (success)
            {
                if (SelectedEmployee?.EmployeeID == employeeId) ClearForm();
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
            var employees = await _employeeService.GetAllEmployeesAsync();
            Employees.Clear();
            foreach (var emp in employees)
            {
                Employees.Add(emp);
            }
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

