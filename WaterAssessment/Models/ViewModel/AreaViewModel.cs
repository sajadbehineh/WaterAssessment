using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WaterAssessment.Messages;
using WaterAssessment.Services;

namespace WaterAssessment.Models.ViewModel
{
    public partial class AreaViewModel : ObservableObject
    {
        private readonly IAreaService _areaService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<Area> Areas { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddAreaCommand))]
        private string _areaName = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddAreaCommand))]
        private Area? _selectedArea;

        [ObservableProperty] private bool _isErrorVisible;
        [ObservableProperty] private InfoBarSeverity _infoBarSeverity;
        [ObservableProperty] private string _infoBarMessage = string.Empty;
        [ObservableProperty] private string _addEditBtnContent = "ذخیره";

        public AreaViewModel(IAreaService areaService, IDialogService dialogService)
        {
            _areaService = areaService;
            _dialogService = dialogService;
            _ = LoadAreasAsync();
        }

        private bool CanAddArea()
        {
            return !string.IsNullOrWhiteSpace(AreaName);
        }

        [RelayCommand(CanExecute = nameof(CanAddArea))]
        private async Task AddAreaAsync()
        {
            if (!ValidateInput()) return;

            bool success;
            if (SelectedArea == null)
            {
                var result = await _areaService.AddNewAreaAsync(AreaName);
                if (result != null)
                {
                    WeakReferenceMessenger.Default.Send(new AreaAddedMessage(result));
                    ClearForm();
                    await LoadAreasAsync(); // Refresh list after operation
                    await ShowMessageAsync("افزودن حوزه جدید با موفقیت انجام شد.", InfoBarSeverity.Success);
                }
                else
                {
                    await ShowMessageAsync(_areaService.GetLastErrorMessage(), InfoBarSeverity.Error);
                }
            }
            else
            {
                var result = await _areaService.UpdateAreaAsync(SelectedArea.AreaID, AreaName);
                if (result != null)
                {
                    WeakReferenceMessenger.Default.Send(new AreaUpdatedMessage(result));
                    ClearForm();
                    await LoadAreasAsync(); // Refresh list after operation
                    await ShowMessageAsync("حوزه مورد نظر با موفقیت ویرایش شد.", InfoBarSeverity.Success);
                }
                else
                {
                    await ShowMessageAsync(_areaService.GetLastErrorMessage(), InfoBarSeverity.Error);
                }
            }
        }

        [RelayCommand]
        private async Task RequestDeleteAreaAsync(Area area)
        {
            if (area == null) return;

            // از سرویس دیالوگ برای نمایش پیغام تایید استفاده کنید
            bool confirmed = await _dialogService.ShowConfirmationDialogAsync(
                title: "تأیید عملیات حذف",
                content: $"آیا از حذف حوزه «{area.AreaName}» اطمینان دارید؟\nاین عملیات غیرقابل بازگشت است.",
                primaryButtonText: "بله، حذف کن",
                closeButtonText: "انصراف"
            );

            // فقط در صورت تایید کاربر، حذف را ادامه دهید
            if (confirmed)
            {
                await DeleteAreaAsync(area);
            }
        }

        private async Task DeleteAreaAsync(Area area)
        {
            try
            {
                var success = await _areaService.DeleteAreaAsync(area.AreaID);
                WeakReferenceMessenger.Default.Send(new AreaDeletedMessage(area));
                if (SelectedArea?.AreaID == area.AreaID) ClearForm();
                await LoadAreasAsync();
                await ShowMessageAsync("حوزه با موفقیت حذف شد.", InfoBarSeverity.Success);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                await ShowMessageAsync("این رکورد وجود ندارد", InfoBarSeverity.Warning);
            }
        }

        partial void OnSelectedAreaChanged(Area? value)
        {
            if (value != null)
            {
                AreaName = value.AreaName;
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
            SelectedArea = null;
            AreaName = string.Empty;
            IsErrorVisible = false;
            InfoBarMessage = string.Empty;
        }

        private async Task LoadAreasAsync()
        {
            var areas = await _areaService.GetAllAreasAsync();
            Areas.Clear();
            foreach (var area in areas)
            {
                Areas.Add(area);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(AreaName))
            {
                _ = ShowMessageAsync("نام حوزه نمی‌توانند خالی باشند.", InfoBarSeverity.Error);
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

