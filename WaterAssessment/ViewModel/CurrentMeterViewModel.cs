using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WaterAssessment.Services;

namespace WaterAssessment.ViewModel
{
    public partial class CurrentMeterViewModel : PagedViewModelBase<CurrentMeter>
    {
        private readonly ICurrentMeterService _currentMeterService;
        private readonly IDialogService _dialogService;
        public ObservableCollection<CurrentMeter> CurrentMeters => PagedItems;
        public int TotalCurrentMeters => TotalItems;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCurrentMeterCommand))]
        private CurrentMeter? _selectedCurrentMeter;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCurrentMeterCommand))]
        private string _currentMeterName = string.Empty;

        [ObservableProperty] private bool _isErrorVisible;

        [ObservableProperty] private InfoBarSeverity _infoBarSeverity;

        [ObservableProperty] private string _infoBarMessage = string.Empty;

        [ObservableProperty] private string _addEditBtnContent = "ذخیره";

        public CurrentMeterViewModel(ICurrentMeterService currentMeterService, IDialogService dialogService) : base(pageSize: 10)
        {
            _currentMeterService = currentMeterService;
            _dialogService = dialogService;
            _ = LoadCurrentMetersAsync();
        }

        private bool CanAddCurrentMeter()
        {
            return !string.IsNullOrWhiteSpace(CurrentMeterName);
        }

        [RelayCommand(CanExecute = nameof(CanAddCurrentMeter))]
        private async Task AddCurrentMeterAsync()
        {
            if (!ValidateInput()) return;
            bool success;

            if (SelectedCurrentMeter == null)
            {
                success = await _currentMeterService.AddNewCurrentMeterAsync(CurrentMeterName);
            }
            else
            {
                success = await _currentMeterService.UpdateCurrentMeterAsync(SelectedCurrentMeter.CurrentMeterID,
                    CurrentMeterName);
            }

            if (success)
            {
                ClearForm();
                await LoadCurrentMetersAsync();
                await ShowMessageAsync("عملیات با موفقیت انجام شد.", InfoBarSeverity.Success);
            }
            else
            {
                await ShowMessageAsync("عملیات با موفقیت انجام شد.", InfoBarSeverity.Success);
            }
        }

        [RelayCommand]
        private async Task RequestDeleteCurrentMeterAsync(CurrentMeter currentMeter)
        {
            if (currentMeter == null) return;

            // از سرویس دیالوگ برای نمایش پیغام تایید استفاده کنید
            bool confirmed = await _dialogService.ShowConfirmationDialogAsync(
                title: "تأیید عملیات حذف",
                content: $"آیا از حذف مولینه «{currentMeter.CurrentMeterName}» اطمینان دارید؟\nاین عملیات غیرقابل بازگشت است.",
                primaryButtonText: "بله، حذف کن",
                closeButtonText: "انصراف"
            );

            // فقط در صورت تایید کاربر، حذف را ادامه دهید
            if (confirmed)
            {
                await DeleteCurrentMeterAsync(currentMeter.CurrentMeterID);
            }
        }

        private async Task DeleteCurrentMeterAsync(int currentMeterId)
        {
            var success = await _currentMeterService.DeleteCurrentMeterAsync(currentMeterId);
            if (success)
            {
                if (SelectedCurrentMeter?.CurrentMeterID == currentMeterId) ClearForm();
                await LoadCurrentMetersAsync();
                await ShowMessageAsync("مولینه با موفقیت حذف شد.", InfoBarSeverity.Success);
            }
            else
            {
                await ShowMessageAsync(_currentMeterService.GetLastErrorMessage(), InfoBarSeverity.Warning);
            }
        }

        partial void OnSelectedCurrentMeterChanged(CurrentMeter? value)
        {
            if (value != null)
            {
                CurrentMeterName = value.CurrentMeterName;
                AddEditBtnContent = "ویرایش";
            }
            else
            {
                // اگر انتخاب برداشته شد، دکمه به حالت ذخیره برگردد
                AddEditBtnContent = "ذخیره";
            }
        }

        [RelayCommand]
        private void ClearForm()
        {
            SelectedCurrentMeter = null;
            CurrentMeterName = string.Empty;
            IsErrorVisible = false;
            InfoBarMessage = string.Empty;
        }

        private async Task LoadCurrentMetersAsync()
        {
            var currentMetersResult = await _currentMeterService.GetAllCurrentMetersAsync();
            SetItems(currentMetersResult);
            OnPropertyChanged(nameof(TotalCurrentMeters));
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(CurrentMeterName))
            {
                _ = ShowMessageAsync("نام مولینه نمی‌توانند خالی باشند.", InfoBarSeverity.Error);
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
