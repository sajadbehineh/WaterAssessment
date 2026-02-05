using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WaterAssessment.Messages;
using WaterAssessment.Services;

namespace WaterAssessment.Models.ViewModel
{
    public partial class LocationViewModel : ObservableObject
    {
        private readonly ILocationService _locationService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<Location> Locations { get; } = new();

        public ObservableCollection<LocationType> LocationTypes { get; } = new();

        public ObservableCollection<Area> Areas { get; } = new();

        // لیستی برای مدیریت پمپ‌های یک مکان در حافظه (قبل از ذخیره)
        public ObservableCollection<LocationPump> TempPumps { get; } = new();

        // ==========================================================
        // تعریف پراپرتی‌های ورودی (Inputs)
        // ==========================================================

        // نام مکان
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveLocationCommand))]
        private string _locationName = string.Empty;

        [ObservableProperty] private bool _hasGate;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GateCountDouble))]
        private int? _gateCount;

        [ObservableProperty] private bool _hasPump;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PumpCountDouble))]
        private int? _pumpCount;

        public double GateCountDouble
        {
            get => GateCount ?? 0; // تبدیل نال به صفر یا مقدار پیش‌فرض
            set => GateCount = (int)value;
        }

        public double PumpCountDouble
        {
            get => PumpCount ?? 0;
            set => PumpCount = (int)value;
        }

        // ==========================================================
        // متد کمکی برای اطلاع‌رسانی نمایش در UI (Visibility)
        // ==========================================================

        public Visibility GateVisibility => HasGate ? Visibility.Visible : Visibility.Collapsed;
        public Visibility PumpVisibility => HasPump ? Visibility.Visible : Visibility.Collapsed;

        // حوزه‌ای که در ComboBox انتخاب شده است (برای افزودن/ویرایش)
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveLocationCommand))]
        private Area? _selectedArea;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveLocationCommand))]
        private LocationType? _selectedLocationType;


        // آیتم انتخاب شده در جدول (برای ویرایش یا حذف)
        [ObservableProperty]
        private Location? _selectedLocation;

        // ==========================================================
        // تعریف پراپرتی‌های کنترلی و وضعیت (State)
        // ==========================================================

        [ObservableProperty]
        private bool _isErrorVisible;

        [ObservableProperty]
        private InfoBarSeverity _infoBarSeverity;

        [ObservableProperty]
        private string _infoBarMessage = string.Empty;

        [ObservableProperty]
        private string _addEditBtnContent = "ذخیره";


        // ==========================================================
        // Logic & Lifecycle
        // ==========================================================

        // وقتی کاربر تیک HasGate را می‌زند یا برمی‌دارد
        partial void OnHasGateChanged(bool value)
        {
            OnPropertyChanged(nameof(GateVisibility));
            if (!value) GateCount = null;
        }

        // وقتی کاربر تیک HasPump را می‌زند یا برمی‌دارد
        partial void OnHasPumpChanged(bool value)
        {
            OnPropertyChanged(nameof(PumpVisibility));
            if (!value)
            {
                PumpCount = null;
                TempPumps.Clear();
            }
        }

        // وقتی تعداد پمپ‌ها تغییر می‌کند، سطرهای ورود دبی اسمی ساخته می‌شوند
        partial void OnPumpCountChanged(int? value)
        {
            int count = value ?? 0;

            // همگام‌سازی لیست موقت پمپ‌ها بدون از دست دادن داده‌های قبلی در صورت امکان
            if (count < 0) return;

            if (count < TempPumps.Count)
            {
                while (TempPumps.Count > count) TempPumps.RemoveAt(TempPumps.Count - 1);
            }
            else
            {
                for (int i = TempPumps.Count + 1; i <= count; i++)
                {
                    TempPumps.Add(new LocationPump { PumpName = $"پمپ شماره {i}", NominalFlow = 0 });
                }
            }
        }

        partial void OnSelectedLocationChanged(Location? value)
        {
            if (value != null)
            {
                LocationName = value.LocationName;
                SelectedArea = Areas.FirstOrDefault(a => a.AreaID == value.AreaID);
                SelectedLocationType = LocationTypes.FirstOrDefault(lt => lt.LocationTypeID == value.LocationTypeID);

                HasGate = value.GateCount.HasValue;
                GateCountDouble = value.GateCount.HasValue ? (double)value.GateCount.Value : 0.0; // +++ لود کردن تعداد دریچه +++

                HasPump = value.PumpCount.HasValue;
                PumpCountDouble = value.PumpCount.HasValue ? (double)value.PumpCount.Value : 0.0;

                TempPumps.Clear();
                if (value.LocationPumps != null)
                {
                    foreach (var p in value.LocationPumps)
                        TempPumps.Add(new LocationPump { Id = p.Id, PumpName = p.PumpName, NominalFlow = p.NominalFlow });
                }

                AddEditBtnContent = "ویرایش";
            }
            else
            {
                AddEditBtnContent = "ذخیره";
            }
        }

        // ==========================================================
        // Constructor
        // ==========================================================
        public LocationViewModel(ILocationService locationService, IDialogService dialogService)
        {
            _locationService = locationService;
            _dialogService = dialogService;
            // لود همزمان مکان‌ها و حوزه‌ها
            // استفاده از روش Fire and Forget (یا فراخوانی در Loaded صفحه)
            _ = LoadInitialDataAsync();

            // Register for messages to keep dropdowns in sync
            RegisterMessages();
        }

        // ==========================================================
        // Commands
        // ==========================================================

        private bool CanSaveLocation()
        {
            // دکمه فعال است اگر: نام پر باشد AND یک حوزه انتخاب شده باشد
            return !string.IsNullOrWhiteSpace(LocationName) && SelectedArea != null &&
                   SelectedLocationType != null;
        }

        [RelayCommand(CanExecute = nameof(CanSaveLocation))]
        private async Task SaveLocationAsync()
        {
            if (!ValidateInput()) return;

            var locationData = new Location
            {
                LocationName = this.LocationName,
                AreaID = this.SelectedArea!.AreaID,
                LocationTypeID = this.SelectedLocationType!.LocationTypeID,
                GateCount = HasGate ? this.GateCount : null,
                PumpCount = HasPump ? this.PumpCount : null,
                LocationPumps = HasPump ? TempPumps.ToList() : new List<LocationPump>()
            };

            bool success;
            if (SelectedLocation == null)
                success = await _locationService.AddNewLocationAsync(locationData);

            else
                success = await _locationService.UpdateLocationAsync(SelectedLocation.LocationID, locationData);

            if (success)
            {
                ClearForm();
                await LoadLocationsAsync(); // Refresh list
                await ShowMessageAsync("عملیات با موفقیت انجام شد.", InfoBarSeverity.Success);
            }
            else
            {
                await ShowMessageAsync(_locationService.GetLastErrorMessage(), InfoBarSeverity.Error);
            }
        }

        [RelayCommand]
        private async Task RequestDeleteLocationAsync(Location location)
        {
            if (location == null) return;

            bool confirmed = await _dialogService.ShowConfirmationDialogAsync(
                title: "تأیید عملیات حذف",
                content: $"آیا از حذف مکان «{location.LocationName}» اطمینان دارید؟",
                primaryButtonText: "بله، حذف کن",
                closeButtonText: "انصراف"
            );

            if (confirmed)
            {
                await DeleteLocationAsync(location.LocationID);
            }
        }

        private async Task DeleteLocationAsync(int locationId)
        {
            var success = await _locationService.DeleteLocationAsync(locationId);
            if (success)
            {
                if (SelectedLocation?.LocationID == locationId) ClearForm();
                await LoadLocationsAsync(); // Refresh list
                await ShowMessageAsync("مکان با موفقیت حذف شد.", InfoBarSeverity.Success);
            }
            else
            {
                await ShowMessageAsync(_locationService.GetLastErrorMessage(), InfoBarSeverity.Warning);
            }
        }

        //// دکمه مداد در لیست
        //[RelayCommand]
        //private void PrepareForEdit(int id)
        //{
        //    var loc = Locations.FirstOrDefault(l => l.LocationID == id);
        //    if (loc != null)
        //    {
        //        SelectedLocation = loc; // تریگر کردن OnSelectedLocationChanged
        //        SelectedLocationType = LocationTypes.FirstOrDefault(t => t.LocationTypeID == loc.LocationTypeID);
        //        IsErrorVisible = false;
        //    }
        //}

        // دکمه انصراف
        [RelayCommand]
        private void ClearForm()
        {
            SelectedLocation = null;
            LocationName = string.Empty;
            SelectedArea = null;
            SelectedLocationType = null;
            HasGate = false;
            GateCount = null;
            HasPump = false;
            PumpCount = null;
            TempPumps.Clear();
            AddEditBtnContent = "ذخیره";
            IsErrorVisible = false;
        }

        private async Task LoadInitialDataAsync()
        {
            await LoadLocationsAsync();
            await LoadAreasAsync();
            await LoadLocationTypesAsync();
        }

        //private async Task InsertNewLocationAsync()
        //{
        //    if (string.IsNullOrWhiteSpace(LocationName))
        //    {
        //        ShowError("لطفاً نام مکان را وارد کنید.");
        //        return;
        //    }

        //    if (SelectedAreaForInput == null)
        //    {
        //        ShowError("لطفاً حوزه آبریز را انتخاب کنید.");
        //        return;
        //    }

        //    if (SelectedLocationType == null)
        //    {
        //        ShowError("لطفاً نوع مکان (کانال/زهکش و...) را انتخاب کنید.");
        //        return;
        //    }

        //    try
        //    {
        //        using var db = new WaterAssessmentContext();

        //        // چک تکراری بودن (نام مکان در همان حوزه نباید تکراری باشد)
        //        bool exists = await db.Locations.AnyAsync(l =>
        //            l.LocationName == LocationName &&
        //            l.AreaID == SelectedAreaForInput!.AreaID);

        //        if (exists)
        //        {
        //            ShowError("این مکان قبلاً در این حوزه ثبت شده است.");
        //            return;
        //        }

        //        var newLocation = new Location
        //        {
        //            LocationName = LocationName,
        //            AreaID = SelectedAreaForInput.AreaID, // آی‌دی را از کمبوباکس می‌گیریم
        //                                                  // نکته: Area = ... را ست نمی‌کنیم، فقط ID کافیست.
        //                                                  // اما برای نمایش درست در UI، باید آبجکت Area را هم داشته باشیم
        //            LocationTypeID = SelectedLocationType.LocationTypeID,
        //            GateCount = GateCount
        //        };

        //        db.Locations.Add(newLocation);
        //        await db.SaveChangesAsync();

        //        // برای اینکه در لیست UI نام حوزه درست نمایش داده شود، آبجکت Area را دستی ست می‌کنیم
        //        newLocation.Area = SelectedAreaForInput;
        //        newLocation.LocationType = SelectedLocationType;

        //        Locations.Add(newLocation);

        //        ResetInputs();
        //        ShowSuccess("مکان جدید با موفقیت ثبت شد.");
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowError($"خطا در ثبت: {ex.Message}");
        //    }
        //}

        //private async Task UpdateExistingLocationAsync()
        //{
        //    if (SelectedLocation == null) return;

        //    if (SelectedLocationType == null)
        //    {
        //        ShowError("لطفاً نوع مکان را انتخاب کنید.");
        //        return;
        //    }

        //    try
        //    {
        //        using var db = new WaterAssessmentContext();
        //        var locToEdit = await db.Locations.FindAsync(SelectedLocation!.LocationID);

        //        if (locToEdit == null)
        //        {
        //            ShowError("رکورد پیدا نشد.");
        //            return;
        //        }

        //        // چک تکراری هنگام ویرایش
        //        bool isDuplicate = await db.Locations.AnyAsync(l =>
        //            l.LocationName == LocationName &&
        //            l.AreaID == SelectedAreaForInput!.AreaID &&
        //            l.LocationID != SelectedLocation.LocationID);

        //        if (isDuplicate)
        //        {
        //            ShowError("نام مکان تکراری است.");
        //            return;
        //        }

        //        // اعمال تغییرات
        //        locToEdit.LocationName = LocationName;
        //        locToEdit.AreaID = SelectedAreaForInput!.AreaID;
        //        locToEdit.LocationTypeID = SelectedLocationType.LocationTypeID;
        //        locToEdit.GateCount = GateCount;

        //        await db.SaveChangesAsync();

        //        // آپدیت UI
        //        SelectedLocation.LocationName = LocationName;
        //        SelectedLocation.AreaID = SelectedAreaForInput.AreaID;
        //        SelectedLocation.Area = SelectedAreaForInput; // آپدیت نویگیشن پراپرتی برای نمایش
        //        SelectedLocation.LocationTypeID = SelectedLocationType.LocationTypeID;
        //        SelectedLocation.LocationType = SelectedLocationType;

        //        SelectedLocation.GateCount = GateCount;

        //        // رفرش لیست
        //        int index = Locations.IndexOf(SelectedLocation);
        //        if (index != -1) Locations[index] = SelectedLocation;

        //        ResetInputs();
        //        ShowSuccess("ویرایش انجام شد.");
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowError($"خطا در ویرایش: {ex.Message}");
        //    }
        //}

        private async Task LoadLocationsAsync()
        {
            var locations = await _locationService.GetAllLocationsAsync();
            Locations.Clear();
            foreach (var location in locations)
            {
                Locations.Add(location);
            }
        }

        private async Task LoadAreasAsync()
        {
            var areas = await _locationService.GetAllAreasAsync();
            Areas.Clear();
            foreach (var area in areas)
            {
                Areas.Add(area);
            }
        }

        private async Task LoadLocationTypesAsync()
        {
            var types = await _locationService.GetAllLocationTypesAsync();
            LocationTypes.Clear();
            foreach (var type in types)
            {
                LocationTypes.Add(type);
            }
        }

        //private void ResetInputs()
        //{
        //    SelectedLocation = null;
        //    LocationName = string.Empty;
        //    SelectedLocationType = null;
        //    SelectedAreaForInput = null; // کمبوباکس را خالی کن
        //    AddEditBtnContent = "ذخیره";
        //    GateCount = 1;
        //}

        //private void ShowError(string message)
        //{
        //    InfoBarMessage = message;
        //    InfoBarSeverity = InfoBarSeverity.Error;
        //    IsErrorVisible = true;
        //}

        //private void ShowSuccess(string message)
        //{
        //    InfoBarMessage = message;
        //    InfoBarSeverity = InfoBarSeverity.Success;
        //    IsErrorVisible = true;
        //}

        private async Task ShowMessageAsync(string message, InfoBarSeverity severity, int durationSeconds = 4)
        {
            InfoBarMessage = message;
            InfoBarSeverity = severity;
            IsErrorVisible = true;
            await Task.Delay(durationSeconds * 1000);
            if (InfoBarMessage == message) IsErrorVisible = false;
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(LocationName) || SelectedArea == null || SelectedLocationType == null)
            {
                _ = ShowMessageAsync("نام مکان، حوزه و نوع مکان نمی‌توانند خالی باشند.", InfoBarSeverity.Error);
                return false;
            }
            return true;
        }

        private void RegisterMessages()
        {
            // گوش دادن به پیامِ "حوزه اضافه شد"
            WeakReferenceMessenger.Default.Register<LocationViewModel, AreaAddedMessage>(this, (recipient, message) =>
            {
                // آبجکت جدید را به لیست Areas اضافه می‌کنیم تا در ComboBox دیده شود
                // چون روی ترد UI هستیم نیازی به Dispatcher نیست
                recipient.Areas.Add(message.Value);
            });

            WeakReferenceMessenger.Default.Register<LocationViewModel, AreaUpdatedMessage>(this, (recipient, message) =>
            {
                // پیدا کردن آیتم ویرایش شده در لیست حوزه‌های فرم مکان
                var areaInList = recipient.Areas.FirstOrDefault(a => a.AreaID == message.Value.AreaID);

                if (areaInList != null)
                {
                    // آپدیت نام
                    areaInList.AreaName = message.Value.AreaName;

                    // نکته مهم: برای اینکه ComboBox متوجه تغییر شود، بهتر است آیتم را در لیست جایگزین کنیم
                    // (مگر اینکه کلاس Area خودش ObservableObject باشد)
                    int index = recipient.Areas.IndexOf(areaInList);
                    if (index != -1)
                    {
                        recipient.Areas[index] = areaInList;
                    }
                }
            });

            WeakReferenceMessenger.Default.Register<LocationViewModel, AreaDeletedMessage>(this, (recipient, message) =>
            {
                int deletedId = message.Value.AreaID; // آی‌دی حذف شده

                // پیدا کردن آیتم در لیست کمبوباکس
                var areaToRemove = recipient.Areas.FirstOrDefault(a => a.AreaID == deletedId);

                if (areaToRemove != null)
                {
                    // حذف از لیست
                    recipient.Areas.Remove(areaToRemove);

                    // نکته حیاتی:
                    // اگر کاربری همین الان این حوزه را در کمبوباکس انتخاب کرده باشد،
                    // باید انتخابش را بپرانیم تا اشتباهی ثبت نکند.
                    if (recipient.SelectedArea != null && recipient.SelectedArea.AreaID == deletedId)
                    {
                        recipient.SelectedArea = null;
                    }
                }
            });

            // گوش دادن به پیامِ "نوع مکان اضافه شد"
            WeakReferenceMessenger.Default.Register<LocationViewModel, LocationTypeAddedMessage>(this, (recipient, message) =>
            {
                // آبجکت جدید را به لیست Areas اضافه می‌کنیم تا در ComboBox دیده شود
                // چون روی ترد UI هستیم نیازی به Dispatcher نیست
                recipient.LocationTypes.Add(message.Value);
            });

            WeakReferenceMessenger.Default.Register<LocationViewModel, LocationTypeUpdatedMessage>(this, (recipient, message) =>
            {
                // پیدا کردن آیتم ویرایش شده در لیست حوزه‌های فرم مکان
                var locationTypeInList = recipient.LocationTypes.FirstOrDefault(a => a.LocationTypeID == message.Value.LocationTypeID);

                if (locationTypeInList != null)
                {
                    // آپدیت نام
                    locationTypeInList.Title = message.Value.Title;

                    int index = recipient.LocationTypes.IndexOf(locationTypeInList);
                    if (index != -1)
                    {
                        recipient.LocationTypes[index] = locationTypeInList;
                    }
                }
            });

            WeakReferenceMessenger.Default.Register<LocationViewModel, LocationTypeDeletedMessage>(this, (recipient, message) =>
            {
                int deletedId = message.Value.LocationTypeID; // آی‌دی حذف شده

                // پیدا کردن آیتم در لیست کمبوباکس
                var locationTypeToRemove = recipient.LocationTypes.FirstOrDefault(a => a.LocationTypeID == deletedId);

                if (locationTypeToRemove != null)
                {
                    // حذف از لیست
                    recipient.LocationTypes.Remove(locationTypeToRemove);

                    // نکته حیاتی:
                    // اگر کاربری همین الان این حوزه را در کمبوباکس انتخاب کرده باشد،
                    // باید انتخابش را بپرانیم تا اشتباهی ثبت نکند.
                    if (recipient.SelectedLocationType != null && recipient.SelectedLocationType.LocationTypeID == deletedId)
                    {
                        recipient.SelectedLocationType = null;
                    }
                }
            });
        }
    }
}
