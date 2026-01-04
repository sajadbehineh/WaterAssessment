using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using CommunityToolkit.Mvvm.Messaging;
using WaterAssessment.Messages;

namespace WaterAssessment.Models.ViewModel
{
    public partial class AreaViewModel : ObservableObject
    {
        // لیست داده‌ها
        public ObservableCollection<Area> Areas { get; } = new();

        // ==========================================================
        // تعریف پراپرتی‌ها با [ObservableProperty]
        // ==========================================================

        [ObservableProperty]
        private int _areaId;

        [ObservableProperty]
        private bool _isAreaNameErrorVisible;

        [ObservableProperty]
        private InfoBarSeverity _infoBarSeverity;

        [ObservableProperty]
        private string _infoBarMessage = string.Empty;

        [ObservableProperty]
        private string _addEditBtnContent = "ذخیره";

        // نام حوزه
        // وقتی نام تغییر می‌کند، باید بررسی شود که دکمه افزودن فعال باشد یا خیر
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddAreaCommand))]
        private string _areaName = string.Empty;

        // آیتم انتخاب شده
        // وقتی تغییر می‌کند، باید دکمه افزودن بررسی شود
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddAreaCommand))]
        private Area? _selectedArea;

        // این متد زمانی اجرا می‌شود که SelectedArea تغییر کند (جایگزین لاجیک درون Setter)
        partial void OnSelectedAreaChanged(Area? value)
        {
            if (value != null)
            {
                AreaName = value.AreaName;
                AddEditBtnContent = "ویرایش"; // تغییر متن دکمه هنگام انتخاب
            }
            else
            {
                // اگر انتخاب برداشته شد، دکمه به حالت ذخیره برگردد
                AddEditBtnContent = "ذخیره";
            }
        }


        public AreaViewModel()
        {
            // لود اولیه (بدون await در سازنده، بهتر است فایر اند فورگت باشد یا در رویداد Loaded صفحه صدا زده شود)
            _ = LoadAreasAsync();
        }

        // ==========================================================
        // تعریف متدها و کامندها با [RelayCommand]
        // نام کامندها خودکار ساخته می‌شود: AddAreaAsync -> AddAreaCommand
        // ==========================================================

        [RelayCommand(CanExecute = nameof(CanAddArea))]
        private async Task AddAreaAsync()
        {
            // ۱. اعتبارسنجی اولیه
            IsAreaNameErrorVisible = false;
            InfoBarMessage = string.Empty;

            // نکته: چون CanExecute داریم، شاید نیازی به این شرط نباشد، اما بودنش ضرر ندارد
            if (string.IsNullOrWhiteSpace(AreaName)) return;

            // ۲. تشخیص حالت (افزودن یا ویرایش)
            if (SelectedArea == null)
            {
                await InsertNewAreaAsync();
            }
            else
            {
                await UpdateExistingAreaAsync();
            }
        }

        private async Task InsertNewAreaAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();
                bool exists = await db.Areas.AnyAsync(e => e.AreaName == AreaName);

                if (exists)
                {
                    ShowError("حوزه مورد نظر قبلاً ثبت شده است.");
                    return;
                }

                var newArea = new Area { AreaName = AreaName };
                db.Areas.Add(newArea);
                await db.SaveChangesAsync();

                Areas.Add(newArea);
                WeakReferenceMessenger.Default.Send(new AreaAddedMessage(newArea));
                ResetInputs();
                ShowSuccess("حوزه جدید با موفقیت ثبت شد.");
            }
            catch (Exception ex)
            {
                ShowError($"خطا در ثبت: {ex.Message}");
            }
        }

        private async Task UpdateExistingAreaAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();
                var areaToEdit = await db.Areas.FindAsync(SelectedArea!.AreaID); // ! چون مطمئنیم null نیست

                if (areaToEdit == null)
                {
                    ShowError("این رکورد یافت نشد.");
                    return;
                }

                // چک تکراری بودن (به جز خودش)
                bool isDuplicate = await db.Areas.AnyAsync(a => a.AreaName == AreaName && a.AreaID != SelectedArea.AreaID);
                if (isDuplicate)
                {
                    ShowError("حوزه وارد شده تکراری است.");
                    return;
                }

                areaToEdit.AreaName = AreaName;
                await db.SaveChangesAsync();

                // آپدیت UI
                SelectedArea.AreaName = AreaName;

                int index = Areas.IndexOf(SelectedArea);
                if (index != -1) Areas[index] = SelectedArea; // تریگر کردن رفرش لیست

                // ==========================================================
                // ارسال پیام ویرایش به LocationViewModel
                // ==========================================================
                WeakReferenceMessenger.Default.Send(new AreaUpdatedMessage(SelectedArea));

                ResetInputs();
                ShowSuccess("ویرایش با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                ShowError($"خطا در ویرایش: {ex.Message}");
            }
        }

        private bool CanAddArea()
        {
            return !string.IsNullOrWhiteSpace(AreaName);
        }

        [RelayCommand]
        private async Task DeleteAreaAsync(int id)
        {
            if (id <= 0) return;

            try
            {
                using var db = new WaterAssessmentContext();
                var areaToDelete = await db.Areas.FindAsync(id);

                // آیا مکانی وجود دارد که مربوط به این حوزه باشد؟
                bool hasDependents = await db.Locations.AnyAsync(l => l.AreaID == id);

                if (hasDependents)
                {
                    // اگر زیرمجموعه داشت، خطا بده و خارج شو
                    ShowError("این حوزه دارای مکان‌های ثبت شده است. برای حذف، ابتدا باید مکان‌های مربوطه را حذف کنید.");
                    return;
                }

                if (areaToDelete != null)
                {
                    db.Areas.Remove(areaToDelete);
                    await db.SaveChangesAsync();

                    // حذف از لیست UI
                    var areaInList = Areas.FirstOrDefault(e => e.AreaID == id);
                    if (areaInList != null)
                    {
                        Areas.Remove(areaInList);
                        WeakReferenceMessenger.Default.Send(new AreaDeletedMessage(areaInList));
                    }

                    // بازخورد
                    ShowSuccess("منطقه با موفقیت حذف شد.");

                    // اگر آیتم در حال ویرایش حذف شد، فرم پاک شود
                    if (SelectedArea != null && SelectedArea.AreaID == id)
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
        private void PrepareForEdit(int areaId)
        {
            var area = Areas.FirstOrDefault(e => e.AreaID == areaId);
            if (area != null)
            {
                // با ست کردن SelectedArea، متد OnSelectedAreaChanged اجرا شده و فیلدها پر می‌شوند
                SelectedArea = area;

                // تمیزکاری خطاها
                IsAreaNameErrorVisible = false;
                InfoBarMessage = "";
            }
        }

        [RelayCommand]
        private void ClearForm()
        {
            SelectedArea = null;
            AreaName = string.Empty;
            IsAreaNameErrorVisible = false;
            InfoBarMessage = string.Empty;
        }

        private void ResetInputs()
        {
            SelectedArea = null;
            AreaName = string.Empty;
            AddEditBtnContent = "ذخیره";
        }

        // ==========================================================
        // متدهای کمکی (Helpers)
        // ==========================================================

        private async Task LoadAreasAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();
                var list = await db.Areas.AsNoTracking().ToListAsync();
                Areas.Clear();
                foreach (var item in list) Areas.Add(item);
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
            IsAreaNameErrorVisible = true;
        }

        private void ShowSuccess(string message)
        {
            InfoBarMessage = message;
            InfoBarSeverity = InfoBarSeverity.Success;
            IsAreaNameErrorVisible = true;
        }
    }
}

