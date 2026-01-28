using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaterAssessment.Messages;

namespace WaterAssessment.Models.ViewModel
{
    public partial class LocationTypeViewModel : ObservableObject
    {
        public ObservableCollection<LocationType> LocationTypes { get; } = new();

        // ==========================================================
        // تعریف پراپرتی‌ها با [ObservableProperty]
        // ==========================================================

        [ObservableProperty]
        private int _locationTypeID;

        [ObservableProperty]
        private bool _isTitleErrorVisible;

        [ObservableProperty]
        private InfoBarSeverity _infoBarSeverity;

        [ObservableProperty]
        private string _infoBarMessage = string.Empty;

        [ObservableProperty]
        private string _addEditBtnContent = "ذخیره";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddLocationTypeCommand))]
        private string _title = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddLocationTypeCommand))]
        private LocationType? _selectedLocationType;

        // این متد زمانی اجرا می‌شود که SelectedArea تغییر کند (جایگزین لاجیک درون Setter)
        partial void OnSelectedLocationTypeChanged(LocationType? value)
        {
            if (value != null)
            {
                Title = value.Title;
                AddEditBtnContent = "ویرایش"; // تغییر متن دکمه هنگام انتخاب
            }
            else
            {
                // اگر انتخاب برداشته شد، دکمه به حالت ذخیره برگردد
                AddEditBtnContent = "ذخیره";
            }
        }


        public LocationTypeViewModel()
        {
            // لود اولیه (بدون await در سازنده، بهتر است فایر اند فورگت باشد یا در رویداد Loaded صفحه صدا زده شود)
            _ = LoadLocationTypesAsync();
        }

        // ==========================================================
        // تعریف متدها و کامندها با [RelayCommand]
        // نام کامندها خودکار ساخته می‌شود: AddAreaAsync -> AddAreaCommand
        // ==========================================================

        [RelayCommand(CanExecute = nameof(CanAddLocationType))]
        private async Task AddLocationTypeAsync()
        {
            // ۱. اعتبارسنجی اولیه
            IsTitleErrorVisible = false;
            InfoBarMessage = string.Empty;

            // نکته: چون CanExecute داریم، شاید نیازی به این شرط نباشد، اما بودنش ضرر ندارد
            if (string.IsNullOrWhiteSpace(Title)) return;

            // ۲. تشخیص حالت (افزودن یا ویرایش)
            if (SelectedLocationType == null)
            {
                await InsertNewLocationTypeAsync();
            }
            else
            {
                await UpdateExistingLocationTypeAsync();
            }
        }

        private async Task InsertNewLocationTypeAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();
                bool exists = await db.LocationTypes.AnyAsync(e => e.Title == Title);

                if (exists)
                {
                    ShowError("نوع مکان مورد نظر قبلاً ثبت شده است.");
                    return;
                }

                var newLocationType = new LocationType { Title = Title };
                db.LocationTypes.Add(newLocationType);
                await db.SaveChangesAsync();

                LocationTypes.Add(newLocationType);
                WeakReferenceMessenger.Default.Send(new LocationTypeAddedMessage(newLocationType));
                ResetInputs();
                ShowSuccess("نوع مکان جدید با موفقیت ثبت شد.");
            }
            catch (Exception ex)
            {
                ShowError($"خطا در ثبت: {ex.Message}");
            }
        }

        private async Task UpdateExistingLocationTypeAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();
                var locationTypeToEdit = await db.LocationTypes.FindAsync(SelectedLocationType!.LocationTypeID); // ! چون مطمئنیم null نیست

                if (locationTypeToEdit == null)
                {
                    ShowError("این رکورد یافت نشد.");
                    return;
                }

                // چک تکراری بودن (به جز خودش)
                bool isDuplicate = await db.LocationTypes.AnyAsync(a => a.Title == Title && a.LocationTypeID != SelectedLocationType.LocationTypeID);
                if (isDuplicate)
                {
                    ShowError("نوع مکان وارد شده تکراری است.");
                    return;
                }

                locationTypeToEdit.Title = Title;
                await db.SaveChangesAsync();

                // آپدیت UI
                SelectedLocationType.Title = Title;

                int index = LocationTypes.IndexOf(SelectedLocationType);
                if (index != -1) LocationTypes[index] = SelectedLocationType; // تریگر کردن رفرش لیست

                // ==========================================================
                // ارسال پیام ویرایش به LocationTypeViewModel
                // ==========================================================
                WeakReferenceMessenger.Default.Send(new LocationTypeUpdatedMessage(SelectedLocationType));

                ResetInputs();
                ShowSuccess("ویرایش با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                ShowError($"خطا در ویرایش: {ex.Message}");
            }
        }

        private bool CanAddLocationType()
        {
            return !string.IsNullOrWhiteSpace(Title);
        }

        [RelayCommand]
        private async Task DeleteLocationTypeAsync(int id)
        {
            if (id <= 0) return;

            try
            {
                using var db = new WaterAssessmentContext();
                var locationTypeToDelete = await db.LocationTypes.FindAsync(id);

                // آیا مکانی وجود دارد که مربوط به این نوع مکان باشد؟
                bool hasDependents = await db.Locations.AnyAsync(l => l.LocationTypeID == id);

                if (hasDependents)
                {
                    // اگر زیرمجموعه داشت، خطا بده و خارج شو
                    ShowError("این نوع مکان دارای مکان‌های ثبت شده است. برای حذف، ابتدا باید مکان‌های مربوطه را حذف کنید.");
                    return;
                }

                if (locationTypeToDelete != null)
                {
                    db.LocationTypes.Remove(locationTypeToDelete);
                    await db.SaveChangesAsync();

                    // حذف از لیست UI
                    var locationTypeInList = LocationTypes.FirstOrDefault(e => e.LocationTypeID == id);
                    if (locationTypeInList != null)
                    {
                        LocationTypes.Remove(locationTypeInList);
                        WeakReferenceMessenger.Default.Send(new LocationTypeDeletedMessage(locationTypeInList));
                    }

                    // بازخورد
                    ShowSuccess("نوع مکان با موفقیت حذف شد.");

                    // اگر آیتم در حال ویرایش حذف شد، فرم پاک شود
                    if (SelectedLocationType != null && SelectedLocationType.LocationTypeID == id)
                    {
                        ClearForm();
                    }
                }
                else
                {
                    ShowError("این رکورد قبلاً حذف شده یا وجود ندارد.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"خطا در حذف: {ex.Message}");
            }
        }

        // کامند ویرایش (انتخاب آیتم برای ویرایش)
        [RelayCommand]
        private void PrepareForEdit(int locationTypeId)
        {
            var locationType = LocationTypes.FirstOrDefault(e => e.LocationTypeID == locationTypeId);
            if (locationType != null)
            {
                // با ست کردن SelectedLocationType، متد OnSelectedLocationTypeChanged اجرا شده و فیلدها پر می‌شوند
                SelectedLocationType = locationType;

                // تمیزکاری خطاها
                IsTitleErrorVisible = false;
                InfoBarMessage = "";
            }
        }

        [RelayCommand]
        private void ClearForm()
        {
            SelectedLocationType = null;
            Title = string.Empty;
            IsTitleErrorVisible = false;
            InfoBarMessage = string.Empty;
        }

        private void ResetInputs()
        {
            SelectedLocationType = null;
            Title = string.Empty;
            AddEditBtnContent = "ذخیره";
        }

        // ==========================================================
        // متدهای کمکی (Helpers)
        // ==========================================================

        private async Task LoadLocationTypesAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();
                var list = await db.LocationTypes.AsNoTracking().ToListAsync();
                LocationTypes.Clear();
                foreach (var item in list) LocationTypes.Add(item);
            }
            catch (Exception ex)
            {
                ShowError($"خطا در بارگذاری اطلاعات: {ex.Message}");
            }
        }


        // متدهای کمکی برای تمیز شدن کد اصلی
        private void ShowError(string message)
        {
            InfoBarMessage = message;
            InfoBarSeverity = InfoBarSeverity.Error;
            IsTitleErrorVisible = true;
        }

        private void ShowSuccess(string message)
        {
            InfoBarMessage = message;
            InfoBarSeverity = InfoBarSeverity.Success;
            IsTitleErrorVisible = true;
        }
    }
}
