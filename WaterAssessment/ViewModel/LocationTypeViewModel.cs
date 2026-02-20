using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WaterAssessment.Messages;
using WaterAssessment.Services;

namespace WaterAssessment.ViewModel
{
    public partial class LocationTypeViewModel : PagedViewModelBase<LocationType>
    {
        private readonly ILocationTypeService _locationTypeService;
        private readonly IDialogService _dialogService;
        public ObservableCollection<LocationType> LocationTypes => PagedItems;
        public int TotalLocationTypes => TotalItems;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddLocationTypeCommand))]
        private string _title = string.Empty;

        [ObservableProperty] private bool _isErrorVisible;

        [ObservableProperty] private InfoBarSeverity _infoBarSeverity;

        [ObservableProperty] private string _infoBarMessage = string.Empty;

        [ObservableProperty] private string _addEditBtnContent = "ذخیره";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddLocationTypeCommand))]
        private LocationType? _selectedLocationType;

        partial void OnSelectedLocationTypeChanged(LocationType? value)
        {
            if (value != null)
            {
                Title = value.Title;
                AddEditBtnContent = "ویرایش";
            }
            else
            {
                AddEditBtnContent = "ذخیره";
            }
        }


        public LocationTypeViewModel(ILocationTypeService locationTypeService, IDialogService dialogService)
        {
            _locationTypeService = locationTypeService;
            _dialogService = dialogService;
            _ = LoadLocationTypesAsync();
        }

        private bool CanAddLocationType()
        {
            return !string.IsNullOrWhiteSpace(Title);
        }

        [RelayCommand(CanExecute = nameof(CanAddLocationType))]
        private async Task AddLocationTypeAsync()
        {
            if (!ValidateInput()) return;

            if (SelectedLocationType == null)
            {
                var result = await _locationTypeService.AddNewLocationTypeAsync(Title);
                if (result != null) // اگر null نبود یعنی موفق بوده
                {
                    // ارسال پیام از سمت ViewModel
                    WeakReferenceMessenger.Default.Send(new LocationTypeAddedMessage(result));

                    ClearForm();
                    await LoadLocationTypesAsync();
                    await ShowMessageAsync("افزودن نوع مکان با موفقیت انجام شد.", InfoBarSeverity.Success);
                }
                else
                {
                    await ShowMessageAsync(_locationTypeService.GetLastErrorMessage(), InfoBarSeverity.Error);
                }
            }
            else
            {
                var result = await _locationTypeService.UpdateLocationTypeAsync(SelectedLocationType.LocationTypeID, Title);

                if (result != null)
                {
                    WeakReferenceMessenger.Default.Send(new LocationTypeUpdatedMessage(result));

                    ClearForm();
                    await LoadLocationTypesAsync();
                    await ShowMessageAsync("نوع مکان با موفقیت ویرایش شد.", InfoBarSeverity.Success);
                }
                else
                {
                    await ShowMessageAsync(_locationTypeService.GetLastErrorMessage(), InfoBarSeverity.Error);
                }
            }
        }

        [RelayCommand]
        private async Task RequestDeleteLocationTypeAsync(LocationType locationType)
        {
            if (locationType == null) return;

            bool confirmed = await _dialogService.ShowConfirmationDialogAsync(
                title: "تأیید عملیات حذف",
                content: $"آیا از حذف نوع مکان «{locationType.Title}» اطمینان دارید؟\nاین عملیات غیرقابل بازگشت است.",
                primaryButtonText: "بله، حذف کن",
                closeButtonText: "خیر"
            );

            if (confirmed)
            {
                await DeleteLocationTypeAsync(locationType);
            }
        }

        private async Task DeleteLocationTypeAsync(LocationType locType)
        {
            var success = await _locationTypeService.DeleteLocationTypeAsync(locType.LocationTypeID);
            if (success)
            {
                WeakReferenceMessenger.Default.Send(new LocationTypeDeletedMessage(locType));
                if (SelectedLocationType?.LocationTypeID == locType.LocationTypeID) ClearForm();
                await LoadLocationTypesAsync();
                await ShowMessageAsync("نوع مکان با موفقیت حذف شد.", InfoBarSeverity.Success);
            }
            else
            {
                await ShowMessageAsync(_locationTypeService.GetLastErrorMessage(), InfoBarSeverity.Warning);
            }
        }

        [RelayCommand]
        private void ClearForm()
        {
            SelectedLocationType = null;
            Title = string.Empty;
            IsErrorVisible = false;
            InfoBarMessage = string.Empty;
        }

        private async Task LoadLocationTypesAsync()
        {
            var locTypesResult = await _locationTypeService.GetAllLocationTypesAsync();
            SetItems(locTypesResult);
            OnPropertyChanged(nameof(TotalLocationTypes));
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                _ = ShowMessageAsync("نوع مکان نمی‌تواند خالی باشد.", InfoBarSeverity.Error);
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
