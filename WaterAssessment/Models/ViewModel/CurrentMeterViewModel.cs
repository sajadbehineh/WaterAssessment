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
    public partial class CurrentMeterViewModel : ObservableObject
    {
        // لیست داده‌ها
        public ObservableCollection<CurrentMeter> CurrentMeters { get; } = new();

        [ObservableProperty] private int _currentMeterId;

        [ObservableProperty] private bool _isErrorVisible;

        [ObservableProperty] private InfoBarSeverity _infoBarSeverity;

        [ObservableProperty] private string _infoBarMessage = string.Empty;

        [ObservableProperty] private string _addEditBtnContent = "ذخیره";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCurrentMeterCommand))]
        private string _currentMeterName = string.Empty;

        // آیتم انتخاب شده
        // وقتی تغییر می‌کند، باید دکمه افزودن بررسی شود
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCurrentMeterCommand))]
        private CurrentMeter? _selectedCurrentMeter;


        partial void OnSelectedCurrentMeterChanged(CurrentMeter? value)
        {
            if (value != null)
            {
                CurrentMeterName = value.CurrentMeterName;
                AddEditBtnContent = "ویرایش"; // تغییر متن دکمه هنگام انتخاب
            }
            else
            {
                // اگر انتخاب برداشته شد، دکمه به حالت ذخیره برگردد
                AddEditBtnContent = "ذخیره";
            }
        }


        public CurrentMeterViewModel()
        {
            // لود اولیه (بدون await در سازنده، بهتر است فایر اند فورگت باشد یا در رویداد Loaded صفحه صدا زده شود)
            _ = LoadCurrentMetersAsync();
        }


        [RelayCommand(CanExecute = nameof(CanAddCurrentMeter))]
        private async Task AddCurrentMeterAsync()
        {
            // ۱. اعتبارسنجی اولیه
            IsErrorVisible = false;
            InfoBarMessage = string.Empty;

            // نکته: چون CanExecute داریم، شاید نیازی به این شرط نباشد، اما بودنش ضرر ندارد
            if (string.IsNullOrWhiteSpace(CurrentMeterName)) return;

            // ۲. تشخیص حالت (افزودن یا ویرایش)
            if (SelectedCurrentMeter == null)
            {
                await InsertNewCurrentMeterAsync();
            }
            else
            {
                await UpdateExistingCurrentMeterAsync();
            }
        }

        private async Task InsertNewCurrentMeterAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();
                bool exists = await db.CurrentMeters.AnyAsync(e => e.CurrentMeterName == CurrentMeterName);

                if (exists)
                {
                    ShowError("مولینه مورد نظر قبلاً ثبت شده است.");
                    return;
                }

                var newCurrentMeter = new CurrentMeter { CurrentMeterName = CurrentMeterName };
                db.CurrentMeters.Add(newCurrentMeter);
                await db.SaveChangesAsync();

                CurrentMeters.Add(newCurrentMeter);
                ResetInputs();
                ShowSuccess("مولینه جدید با موفقیت ثبت شد.");
            }
            catch (Exception ex)
            {
                ShowError($"خطا در ثبت: {ex.Message}");
            }
        }

        private async Task UpdateExistingCurrentMeterAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();
                var CurrentMeterToEdit = await db.CurrentMeters.FindAsync(SelectedCurrentMeter!.CurrentMeterID); // ! چون مطمئنیم null نیست

                if (CurrentMeterToEdit == null)
                {
                    ShowError("این رکورد یافت نشد.");
                    return;
                }

                // چک تکراری بودن (به جز خودش)
                bool isDuplicate = await db.CurrentMeters
                    .AnyAsync(a => a.CurrentMeterName == CurrentMeterName && a.CurrentMeterID != SelectedCurrentMeter.CurrentMeterID);

                if (isDuplicate)
                {
                    ShowError("مولینه وارد شده تکراری است.");
                    return;
                }

                CurrentMeterToEdit.CurrentMeterName = CurrentMeterName;
                await db.SaveChangesAsync();

                // آپدیت UI
                SelectedCurrentMeter.CurrentMeterName = CurrentMeterName;

                int index = CurrentMeters.IndexOf(SelectedCurrentMeter);
                if (index != -1) CurrentMeters[index] = SelectedCurrentMeter; // تریگر کردن رفرش لیست

                // ==========================================================
                // ارسال پیام ویرایش به LocationViewModel
                // ==========================================================

                ResetInputs();
                ShowSuccess("ویرایش با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                ShowError($"خطا در ویرایش: {ex.Message}");
            }
        }

        private bool CanAddCurrentMeter()
        {
            return !string.IsNullOrWhiteSpace(CurrentMeterName);
        }

        [RelayCommand]
        private async Task DeleteCurrentMeterAsync(int id)
        {
            if (id <= 0) return;

            try
            {
                using var db = new WaterAssessmentContext();
                var CurrentMeterToDelete = await db.CurrentMeters.FindAsync(id);

                // آیا مکانی وجود دارد که مربوط به این حوزه باشد؟
                bool hasDependents = await db.Assessments.AnyAsync(l => l.CurrentMeterID == id);

                if (hasDependents)
                {
                    // اگر زیرمجموعه داشت، خطا بده و خارج شو
                    ShowError("این مولینه دارای اندازه گیری ‌های ثبت شده است. برای حذف، ابتدا باید اندازه گیری‌های مربوطه را حذف کنید.");
                    return;
                }

                if (CurrentMeterToDelete != null)
                {
                    db.CurrentMeters.Remove(CurrentMeterToDelete);
                    await db.SaveChangesAsync();

                    // حذف از لیست UI
                    var currentMeterInList = CurrentMeters.FirstOrDefault(e => e.CurrentMeterID == id);
                    if (currentMeterInList != null)
                    {
                        CurrentMeters.Remove(currentMeterInList);
                    }

                    // بازخورد
                    ShowSuccess("مولینه با موفقیت حذف شد.");

                    // اگر آیتم در حال ویرایش حذف شد، فرم پاک شود
                    if (SelectedCurrentMeter != null && SelectedCurrentMeter.CurrentMeterID == id)
                    {
                        ClearForm();
                        ShowSuccess("مولینه با موفقیت حذف شد.");
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
        private void PrepareForEdit(int currentMeterId)
        {
            var CurrentMeter = CurrentMeters.FirstOrDefault(e => e.CurrentMeterID == currentMeterId);
            if (CurrentMeter != null)
            {
                // با ست کردن SelectedArea، متد OnSelectedAreaChanged اجرا شده و فیلدها پر می‌شوند
                SelectedCurrentMeter = CurrentMeter;

                // تمیزکاری خطاها
                IsErrorVisible = false;
                InfoBarMessage = "";
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

        private void ResetInputs()
        {
            SelectedCurrentMeter = null;
            CurrentMeterName = string.Empty;
            AddEditBtnContent = "ذخیره";
        }

        // ==========================================================
        // متدهای کمکی (Helpers)
        // ==========================================================

        private async Task LoadCurrentMetersAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();
                var list = await db.CurrentMeters.AsNoTracking().ToListAsync();
                CurrentMeters.Clear();
                foreach (var item in list) CurrentMeters.Add(item);
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
