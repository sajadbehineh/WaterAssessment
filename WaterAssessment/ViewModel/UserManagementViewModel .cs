using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using WaterAssessment.Core;
using WaterAssessment.Services;
using User = WaterAssessment.Models.User;

namespace WaterAssessment.ViewModel
{
    public partial class UserManagementViewModel : PagedViewModelBase<User>
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<User> Users => PagedItems;
        public int TotalUsers => TotalItems;

        public ObservableCollection<string> Roles { get; } = ["Admin", "User"];

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddUserCommand))]
        private string _newUsername = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddUserCommand))]
        private string _newFullName = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddUserCommand))]
        private string _selectedRole = "User";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddUserCommand))]
        private User? _selectedUser;

        [ObservableProperty] private bool _isInfoBarOpen;
        [ObservableProperty] private InfoBarSeverity _infoBarSeverity = InfoBarSeverity.Informational;
        [ObservableProperty] private string _infoBarMessage = string.Empty;
        [ObservableProperty] private string _addEditButtonContent = "افزودن کاربر";

        public UserManagementViewModel(IUserManagementService userManagementService, IDialogService dialogService) : base(pageSize: 10)
        {
            _userManagementService = userManagementService;
            _dialogService = dialogService;
            _ = LoadUsersAsync();
        }

        [RelayCommand]
        private async Task LoadUsersAsync()
        {
            var userList = await _userManagementService.GetAllUsersAsync();
            SetItems(userList);
            OnPropertyChanged(nameof(TotalUsers));

            if (!userList.Any())
            {
                var error = _userManagementService.GetLastErrorMessage();
                if (!string.IsNullOrWhiteSpace(error))
                {
                    await ShowMessageAsync(error, InfoBarSeverity.Error);
                }
            }
        }

        private bool CanAddUser(object? passwordBoxParam)
        {
            var passwordBox = passwordBoxParam as PasswordBox;
            var password = passwordBox?.Password;

            return !string.IsNullOrWhiteSpace(NewUsername)
                   && !string.IsNullOrWhiteSpace(NewFullName)
                   && !string.IsNullOrWhiteSpace(SelectedRole)
                   && (SelectedUser != null || !string.IsNullOrWhiteSpace(password));
        }

        [RelayCommand(CanExecute = nameof(CanAddUser))]
        private async Task AddUserAsync(object passwordBoxParam)
        {
            var passwordBox = passwordBoxParam as PasswordBox;
            var password = passwordBox?.Password;

            if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewFullName))
            {
                await ShowMessageAsync("تمام فیلدها باید تکمیل شوند.", InfoBarSeverity.Warning);
                return;
            }

            if (SelectedUser == null)
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    await ShowMessageAsync("رمز عبور برای کاربر جدید الزامی است.", InfoBarSeverity.Warning);
                    return;
                }
                var newUser = await _userManagementService.AddUserAsync(NewUsername, NewFullName, SelectedRole, password);
                if (newUser == null)
                {
                    await ShowMessageAsync(_userManagementService.GetLastErrorMessage(), InfoBarSeverity.Error);
                    return;
                }
                Users.Add(newUser);
                ClearForm(passwordBox);
                await ShowMessageAsync("کاربر با موفقیت اضافه شد.", InfoBarSeverity.Success);
                return;
            }

            var updatedUser = await _userManagementService.UpdateUserAsync(
                SelectedUser.UserID,
                NewUsername,
                NewFullName,
                SelectedRole,
                password);

            if (updatedUser == null)
            {
                await ShowMessageAsync(_userManagementService.GetLastErrorMessage(), InfoBarSeverity.Error);
                return;
            }
            var index = Users.IndexOf(SelectedUser);
            if (index != -1)
            {
                Users.RemoveAt(index);
                Users.Insert(index, updatedUser);
            }
            else
            {
                await LoadUsersAsync();
            }

            ClearForm(passwordBox);
            await ShowMessageAsync("کاربر با موفقیت ویرایش شد.", InfoBarSeverity.Success);
        }

        [RelayCommand]
        private async Task DeleteUserAsync(User? userToDelete)
        {
            if (userToDelete == null) return;

            bool confirmed = await _dialogService.ShowConfirmationDialogAsync(
                title: "حذف کاربر",
                content: $"آیا از حذف کاربر «{userToDelete.Username}» اطمینان دارید؟",
                primaryButtonText: "بله، حذف کن",
                closeButtonText: "انصراف");

            if (!confirmed) return;

            var success = await _userManagementService.DeleteUserAsync(
                userToDelete.UserID,
                AppSession.CurrentUser?.UserID);

            if (!success)
            {
                await ShowMessageAsync(_userManagementService.GetLastErrorMessage(), InfoBarSeverity.Warning);
                return;
            }

            Users.Remove(userToDelete);

            if (SelectedUser?.UserID == userToDelete.UserID)
            {
                ClearForm();
            }

            await ShowMessageAsync("کاربر حذف شد.", InfoBarSeverity.Success);
        }

        [RelayCommand]
        private void EditUser(User? user)
        {
            SelectedUser = user;
        }

        [RelayCommand]
        private void ClearForm(object? passwordBoxParam = null)
        {
            SelectedUser = null;
            NewUsername = string.Empty;
            NewFullName = string.Empty;
            SelectedRole = "User";
            AddEditButtonContent = "افزودن کاربر";
            IsInfoBarOpen = false;
            InfoBarMessage = string.Empty;

            if (passwordBoxParam is PasswordBox passwordBox)
            {
                passwordBox.Password = string.Empty;
            }
        }

        private async Task ShowMessageAsync(string message, InfoBarSeverity severity, int durationSeconds = 4)
        {
            InfoBarMessage = message;
            InfoBarSeverity = severity;
            IsInfoBarOpen = true;

            await Task.Delay(durationSeconds * 1000);

            if (InfoBarMessage == message)
            {
                IsInfoBarOpen = false;
            }
        }

        partial void OnSelectedUserChanged(User? value)
        {
            if (value == null)
            {
                NewUsername = string.Empty;
                NewFullName = string.Empty;
                SelectedRole = "User";
                AddEditButtonContent = "افزودن کاربر";
                return;
            }

            NewUsername = value.Username;
            NewFullName = value.FullName ?? string.Empty;
            SelectedRole = value.Role;
            AddEditButtonContent = "ویرایش کاربر";
        }
    }
}
