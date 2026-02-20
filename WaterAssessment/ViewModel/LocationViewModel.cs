using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WaterAssessment.Messages;
using WaterAssessment.Services;

namespace WaterAssessment.ViewModel
{
    public partial class LocationViewModel : PagedViewModelBase<Location>
    {
        private readonly ILocationService _locationService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<Location> Locations => PagedItems;
        public int TotalLocations => TotalItems;

        public ObservableCollection<LocationType> LocationTypes { get; } = new();

        public ObservableCollection<Area> Areas { get; } = new();

        // لیستی برای مدیریت پمپ‌های یک مکان در حافظه (قبل از ذخیره)
        public ObservableCollection<LocationPump> TempPumps { get; } = new();

        public ObservableCollection<HydraulicGate> TempHydraulicGates { get; } = new();

        public ObservableCollection<MeasurementFormType> AvailableFormTypes { get; } = new(Enum.GetValues<MeasurementFormType>());

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

        // نوع فرم اندازه‌گیری انتخاب شده

        [ObservableProperty]
        private MeasurementFormType _selectedMeasurementFormType = MeasurementFormType.HydrometrySingleSection;

        // تعداد مقاطع (فقط برای فرم چند مقطعی)
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SectionCountDouble))]
        private int? _sectionCount;

        // برای راحتی کار با کنترل‌های عددی که ممکن است مقدار نال داشته باشند، پراپرتی‌های کمکی از نوع double تعریف می‌کنیم
        public double SectionCountDouble
        {
            get => SectionCount ?? 1;
            set => SectionCount = (int)value;
        }

        // نمایش یا عدم نمایش فرم مربوط به تعداد مقاطع در فرم چند مقطعی
        public Visibility SectionCountVisibility => SelectedMeasurementFormType == MeasurementFormType.HydrometryMultiSection
            ? Visibility.Visible
            : Visibility.Collapsed;

        // نمایش یا عدم نمایش فرم مربوط به معادله دبی دریچه
        public Visibility GateDischargeConfigVisibility => SelectedMeasurementFormType == MeasurementFormType.GateDischargeEquation
            ? Visibility.Visible
            : Visibility.Collapsed;

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
            if (!value)
            {
                GateCount = null;
                TempHydraulicGates.Clear();
            }
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

        // وقتی نوع فرم اندازه‌گیری تغییر می‌کند، باید نمایش فرم‌های مرتبط را به‌روزرسانی کنیم و مقادیر غیرمرتبط را پاک کنیم
        partial void OnSelectedMeasurementFormTypeChanged(MeasurementFormType value)
        {
            OnPropertyChanged(nameof(SectionCountVisibility));
            OnPropertyChanged(nameof(GateDischargeConfigVisibility));

            if (value != MeasurementFormType.HydrometryMultiSection)
            {
                SectionCount = null;
            }

            if (value == MeasurementFormType.GateDischargeEquation)
            {
                HasGate = true;
                if (!GateCount.HasValue || GateCount.Value <= 0)
                {
                    GateCount = 1;
                }
                SyncHydraulicGatesWithGateCount();
            }
            else
            {
                TempHydraulicGates.Clear();
            }
        }

        // وقتی تعداد دریچه‌ها تغییر می‌کند، باید لیست دریچه‌های هیدرولیکی را همگام‌سازی کنیم
        partial void OnGateCountChanged(int? value)
        {
            if (SelectedMeasurementFormType == MeasurementFormType.GateDischargeEquation)
            {
                SyncHydraulicGatesWithGateCount();
            }
        }

        // این متد لیست دریچه‌های هیدرولیکی را با توجه به تعداد دریچه‌ها همگام‌سازی می‌کند، بدون اینکه داده‌های وارد شده قبلی را از دست بدهد (تا حد امکان)
        private void SyncHydraulicGatesWithGateCount()
        {
            int count = GateCount ?? 0;
            if (count < TempHydraulicGates.Count)
            {
                while (TempHydraulicGates.Count > count) TempHydraulicGates.RemoveAt(TempHydraulicGates.Count - 1);
            }
            else
            {
                for (int i = TempHydraulicGates.Count + 1; i <= count; i++)
                {
                    TempHydraulicGates.Add(new HydraulicGate { GateNumber = i, DischargeCoefficient = 0.62, Width = 1.0 });
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

                SelectedMeasurementFormType = value.MeasurementFormType;
                SectionCount = value.SectionCount;

                TempHydraulicGates.Clear();
                if (value.HydraulicGates != null)
                {
                    foreach (var gate in value.HydraulicGates.OrderBy(g => g.GateNumber))
                    {
                        TempHydraulicGates.Add(new HydraulicGate
                        {
                            Id = gate.Id,
                            GateNumber = gate.GateNumber,
                            DischargeCoefficient = gate.DischargeCoefficient,
                            Width = gate.Width
                        });
                    }
                }

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
        public LocationViewModel(ILocationService locationService, IDialogService dialogService) : base(pageSize: 5)
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
                HasGate = HasGate,
                HasPump = HasPump,
                GateCount = HasGate ? this.GateCount : null,
                PumpCount = HasPump ? this.PumpCount : null,
                MeasurementFormType = SelectedMeasurementFormType,
                SectionCount = SelectedMeasurementFormType == MeasurementFormType.HydrometryMultiSection ? SectionCount : null,
                LocationPumps = HasPump ? TempPumps.ToList() : new List<LocationPump>(),
                HydraulicGates = SelectedMeasurementFormType == MeasurementFormType.GateDischargeEquation ? TempHydraulicGates.ToList() : new List<HydraulicGate>()
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
                closeButtonText: "خیر"
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
            SelectedMeasurementFormType = MeasurementFormType.HydrometrySingleSection;
            SectionCount = null;
            TempHydraulicGates.Clear();
            AddEditBtnContent = "ذخیره";
            IsErrorVisible = false;
        }

        private async Task LoadInitialDataAsync()
        {
            await LoadLocationsAsync();
            await LoadAreasAsync();
            await LoadLocationTypesAsync();
        }

        private async Task LoadLocationsAsync()
        {
            var locationsResult = await _locationService.GetAllLocationsAsync();
            SetItems(locationsResult);
            OnPropertyChanged(nameof(TotalLocations));
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
