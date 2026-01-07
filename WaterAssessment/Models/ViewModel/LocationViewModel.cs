using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using WaterAssessment.Messages;

namespace WaterAssessment.Models.ViewModel
{
    public partial class LocationViewModel : ObservableObject
    {
        // لیست اصلی مکان‌ها (برای نمایش در جدول)
        public ObservableCollection<Location> Locations { get; } = new();

        // لیست حوزه‌ها (برای نمایش در ComboBox جهت انتخاب)
        public ObservableCollection<Area> Areas { get; } = new();

        // ==========================================================
        // تعریف پراپرتی‌های ورودی (Inputs)
        // ==========================================================

        // نام مکان
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddLocationCommand))]
        private string _locationName = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddLocationCommand))]
        private bool _isCanal = true; // پیش‌فرض کانال

        [ObservableProperty]
        private int _gateCount = 1; // پیش‌فرض 1 دریچه

        // حوزه‌ای که در ComboBox انتخاب شده است (برای افزودن/ویرایش)
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddLocationCommand))]
        private Area? _selectedAreaForInput;

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

        // آیتم انتخاب شده در جدول (برای ویرایش یا حذف)
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddLocationCommand))]
        private Location? _selectedLocation;

        // وقتی یک ردیف در جدول انتخاب می‌شود، فرم را پر کن
        partial void OnSelectedLocationChanged(Location? value)
        {
            if (value != null)
            {
                LocationName = value.LocationName;
                IsCanal = value.IsCanal;
                GateCount = value.GateCount; // +++ لود کردن تعداد دریچه +++

                // نکته مهم: باید Area مربوطه را در لیست ComboBox پیدا و انتخاب کنیم
                // تا ComboBox مقدار درست را نشان دهد
                SelectedAreaForInput = Areas.FirstOrDefault(a => a.AreaID == value.AreaID);

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
        public LocationViewModel()
        {
            // لود همزمان مکان‌ها و حوزه‌ها
            // استفاده از روش Fire and Forget (یا فراخوانی در Loaded صفحه)
            _ = LoadDataAsync();

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
                    if (recipient.SelectedAreaForInput != null && recipient.SelectedAreaForInput.AreaID == deletedId)
                    {
                        recipient.SelectedAreaForInput = null;
                    }
                }
            });
        }

        // ==========================================================
        // Commands
        // ==========================================================

        [RelayCommand(CanExecute = nameof(CanAddLocation))]
        private async Task AddLocationAsync()
        {
            IsErrorVisible = false;
            InfoBarMessage = string.Empty;

            // این شرط به خاطر CanExecute تقریبا همیشه برقرار است اما برای اطمینان:
            if (string.IsNullOrWhiteSpace(LocationName) || SelectedAreaForInput == null)
                return;

            if (SelectedLocation == null)
            {
                await InsertNewLocationAsync();
            }
            else
            {
                await UpdateExistingLocationAsync();
            }
        }

        private bool CanAddLocation()
        {
            // دکمه فعال است اگر: نام پر باشد AND یک حوزه انتخاب شده باشد
            return !string.IsNullOrWhiteSpace(LocationName) && SelectedAreaForInput != null;
        }

        [RelayCommand]
        private async Task DeleteLocationAsync(int id)
        {
            if (id <= 0) return;

            try
            {
                using var db = new WaterAssessmentContext();
                var locToDelete = await db.Locations.FindAsync(id);

                if (locToDelete != null)
                {
                    db.Locations.Remove(locToDelete);
                    await db.SaveChangesAsync();

                    // حذف از UI
                    var item = Locations.FirstOrDefault(l => l.LocationID == id);
                    if (item != null) Locations.Remove(item);

                    ShowSuccess("مکان با موفقیت حذف شد.");

                    // اگر آیتم در حال ویرایش حذف شد
                    if (SelectedLocation != null && SelectedLocation.LocationID == id)
                    {
                        ResetInputs();
                    }
                }
                else
                {
                    ShowError("رکورد یافت نشد.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"خطا در حذف: {ex.Message}");
            }
        }

        // دکمه مداد در لیست
        [RelayCommand]
        private void PrepareForEdit(int id)
        {
            var loc = Locations.FirstOrDefault(l => l.LocationID == id);
            if (loc != null)
            {
                SelectedLocation = loc; // تریگر کردن OnSelectedLocationChanged
                IsErrorVisible = false;
            }
        }

        // دکمه انصراف
        [RelayCommand]
        private void ClearForm()
        {
            ResetInputs();
            IsErrorVisible = false;
            InfoBarMessage = string.Empty;
        }

        // ==========================================================
        // Helper Methods (CRUD Logic)
        // ==========================================================

        private async Task LoadDataAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();

                // 1. لود کردن حوزه‌ها برای پر کردن ComboBox
                var areasList = await db.Areas.AsNoTracking().ToListAsync();
                Areas.Clear();
                foreach (var area in areasList) Areas.Add(area);

                // 2. لود کردن مکان‌ها (همراه با نام حوزه - Join)
                var locationsList = await db.Locations
                                            .Include(l => l.Area) // بسیار مهم: برای نمایش نام Area در جدول
                                            .AsNoTracking()
                                            .ToListAsync();
                Locations.Clear();
                foreach (var loc in locationsList) Locations.Add(loc);
            }
            catch (Exception ex)
            {
                ShowError($"خطا در بارگذاری اطلاعات: {ex.Message}");
            }
        }

        private async Task InsertNewLocationAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();

                // چک تکراری بودن (نام مکان در همان حوزه نباید تکراری باشد)
                bool exists = await db.Locations.AnyAsync(l =>
                    l.LocationName == LocationName &&
                    l.AreaID == SelectedAreaForInput!.AreaID);

                if (exists)
                {
                    ShowError("این مکان قبلاً در این حوزه ثبت شده است.");
                    return;
                }

                var newLocation = new Location
                {
                    LocationName = LocationName,
                    AreaID = SelectedAreaForInput!.AreaID, // آی‌دی را از کمبوباکس می‌گیریم
                                                           // نکته: Area = ... را ست نمی‌کنیم، فقط ID کافیست.
                                                           // اما برای نمایش درست در UI، باید آبجکت Area را هم داشته باشیم
                    IsCanal = IsCanal,
                    GateCount = GateCount
                };

                db.Locations.Add(newLocation);
                await db.SaveChangesAsync();

                // برای اینکه در لیست UI نام حوزه درست نمایش داده شود، آبجکت Area را دستی ست می‌کنیم
                newLocation.Area = SelectedAreaForInput;

                Locations.Add(newLocation);

                ResetInputs();
                ShowSuccess("مکان جدید با موفقیت ثبت شد.");
            }
            catch (Exception ex)
            {
                ShowError($"خطا در ثبت: {ex.Message}");
            }
        }

        private async Task UpdateExistingLocationAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();
                var locToEdit = await db.Locations.FindAsync(SelectedLocation!.LocationID);

                if (locToEdit == null)
                {
                    ShowError("رکورد پیدا نشد.");
                    return;
                }

                // چک تکراری هنگام ویرایش
                bool isDuplicate = await db.Locations.AnyAsync(l =>
                    l.LocationName == LocationName &&
                    l.AreaID == SelectedAreaForInput!.AreaID &&
                    l.LocationID != SelectedLocation.LocationID);

                if (isDuplicate)
                {
                    ShowError("نام مکان تکراری است.");
                    return;
                }

                // اعمال تغییرات
                locToEdit.LocationName = LocationName;
                locToEdit.AreaID = SelectedAreaForInput!.AreaID;
                locToEdit.IsCanal = IsCanal;
                locToEdit.GateCount = GateCount;

                await db.SaveChangesAsync();

                // آپدیت UI
                SelectedLocation.LocationName = LocationName;
                SelectedLocation.AreaID = SelectedAreaForInput.AreaID;
                SelectedLocation.Area = SelectedAreaForInput; // آپدیت نویگیشن پراپرتی برای نمایش
                SelectedLocation.IsCanal = IsCanal;
                SelectedLocation.GateCount = GateCount;

                // رفرش لیست
                int index = Locations.IndexOf(SelectedLocation);
                if (index != -1) Locations[index] = SelectedLocation;

                ResetInputs();
                ShowSuccess("ویرایش انجام شد.");
            }
            catch (Exception ex)
            {
                ShowError($"خطا در ویرایش: {ex.Message}");
            }
        }

        private void ResetInputs()
        {
            SelectedLocation = null;
            LocationName = string.Empty;
            SelectedAreaForInput = null; // کمبوباکس را خالی کن
            AddEditBtnContent = "ذخیره";
            IsCanal = true;
            GateCount = 1;
        }

        private void ShowError(string message)
        {
            InfoBarMessage = message;
            InfoBarSeverity = InfoBarSeverity.Error;
            IsErrorVisible = true;
        }

        private void ShowSuccess(string message)
        {
            InfoBarMessage = message;
            InfoBarSeverity = InfoBarSeverity.Success;
            IsErrorVisible = true;
        }
    }
}
