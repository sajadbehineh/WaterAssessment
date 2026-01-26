using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace WaterAssessment.ViewModels;

public partial class PropellerViewModel : ObservableObject
{
    public ObservableCollection<Propeller> Propellers { get; } = new();

    // لیست گزینه‌ها برای ComboBox انتخاب نوع کالیبراسیون
    public List<CalibrationMode> CalibrationModes { get; } = Enum.GetValues(typeof(CalibrationMode)).Cast<CalibrationMode>().ToList();

    public List<CalibrationOption> CalibrationOptions { get; } = new()
    {
        new CalibrationOption("یک معادله‌ای", CalibrationMode.OneEquation),
        new CalibrationOption("دو معادله‌ای", CalibrationMode.TwoEquations),
        new CalibrationOption("سه معادله‌ای", CalibrationMode.ThreeEquations)
    };

    // ==========================================
    // ورودی‌های فرم
    // ==========================================

    [ObservableProperty]
    private string _propellerName = string.Empty;

    // انتخاب نوع کالیبراسیون: با تغییر این، وضعیت نمایش فیلدها آپدیت می‌شود
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSection2Visible))]
    [NotifyPropertyChangedFor(nameof(IsSection3Visible))]
    private CalibrationOption _selectedOption; //نگهداری کل آبجکت رکورد

    private CalibrationMode SelectedMode => SelectedOption?.Mode ?? CalibrationMode.OneEquation;

    // معادله ۱
    [ObservableProperty] private double _a1;
    [ObservableProperty] private double _b1;

    // نقطه شکست ۱ و معادله ۲
    [ObservableProperty] private double? _transitionPoint1;
    [ObservableProperty] private double? _a2;
    [ObservableProperty] private double? _b2;

    // نقطه شکست ۲ و معادله ۳
    [ObservableProperty] private double? _transitionPoint2;
    [ObservableProperty] private double? _a3;
    [ObservableProperty] private double? _b3;


    // ==========================================
    // ویژگی‌های کمکی برای UI (مدیریت Visibility)
    // ==========================================

    // آیا بخش دوم (نقطه شکست ۱ و ضرایب ۲) نمایش داده شود؟
    public bool IsSection2Visible => SelectedMode == CalibrationMode.TwoEquations || SelectedMode == CalibrationMode.ThreeEquations;

    // آیا بخش سوم (نقطه شکست ۲ و ضرایب ۳) نمایش داده شود؟
    public bool IsSection3Visible => SelectedMode == CalibrationMode.ThreeEquations;


    // ==========================================
    // وضعیت‌های عمومی
    // ==========================================
    [ObservableProperty] private Propeller? _selectedPropeller;
    [ObservableProperty] private string _addEditBtnContent = "ذخیره";
    [ObservableProperty] private bool _isErrorVisible;
    [ObservableProperty] private InfoBarSeverity _infoBarSeverity;
    [ObservableProperty] private string _infoBarMessage = string.Empty;

    public PropellerViewModel()
    {
        _ = LoadPropellersAsync();
        SelectedOption = CalibrationOptions.First();
    }

    // وقتی یک پروانه برای ویرایش انتخاب می‌شود
    partial void OnSelectedPropellerChanged(Propeller? value)
    {
        if (value != null)
        {
            PropellerName = value.PropellerName;
            SelectedOption = CalibrationOptions.FirstOrDefault(x => x.Mode == value.Mode)
                             ?? CalibrationOptions.First();

            A1 = value.A1;
            B1 = value.B1;

            TransitionPoint1 = value.TransitionPoint1;
            A2 = value.A2;
            B2 = value.B2;

            TransitionPoint2 = value.TransitionPoint2;
            A3 = value.A3;
            B3 = value.B3;

            AddEditBtnContent = "ویرایش";
        }
    }

    private bool CanAddPropeller()
    {
        return true; // همیشه فعال است
    }


    [RelayCommand]
    private async Task SavePropellerAsync()
    {
        if (string.IsNullOrWhiteSpace(PropellerName)) return;

        // اعتبارسنجی نقاط شکست (T1 باید کوچکتر از T2 باشد)
        if (SelectedMode == CalibrationMode.ThreeEquations &&
            TransitionPoint1.HasValue && TransitionPoint2.HasValue &&
            TransitionPoint1 >= TransitionPoint2)
        {
            ShowError("نقطه شکست اول باید کوچکتر از نقطه شکست دوم باشد.");
            return;
        }

        try
        {
            using var db = new WaterAssessmentContext();
            var modeToSave = SelectedOption.Mode;
            if (SelectedPropeller == null)
            {
                // افزودن جدید
                var newProp = new Propeller
                {
                    PropellerName = PropellerName,
                    Mode = modeToSave,
                    A1 = A1,
                    B1 = B1,
                    // فقط اگر مد مربوطه فعال باشد مقادیر را ذخیره می‌کنیم
                    TransitionPoint1 = IsSection2Visible ? TransitionPoint1 : null,
                    A2 = IsSection2Visible ? A2 : null,
                    B2 = IsSection2Visible ? B2 : null,
                    TransitionPoint2 = IsSection3Visible ? TransitionPoint2 : null,
                    A3 = IsSection3Visible ? A3 : null,
                    B3 = IsSection3Visible ? B3 : null
                };
                db.Propellers.Add(newProp);
                await db.SaveChangesAsync();
                Propellers.Add(newProp);

                ResetInputs();
                ShowSuccess("پروانه مورد نظر با موفقیت ثبت شد.");
            }
            else
            {
                // ویرایش
                var propellerToEdit = await db.Propellers.FindAsync(SelectedPropeller.PropellerID);
                if (propellerToEdit == null)
                {
                    ShowError("این رکورد یافت نشد");
                    return;
                }

                bool isDuplicate = await db.Propellers
                    .AnyAsync(a => a.PropellerName == PropellerName && a.PropellerID != SelectedPropeller.PropellerID);
                if (isDuplicate)
                {
                    ShowError("حوزه وارد شده تکراری است.");
                    return;
                }

                propellerToEdit.PropellerName = PropellerName;
                propellerToEdit.Mode = SelectedMode;
                propellerToEdit.A1 = A1; propellerToEdit.B1 = B1;

                propellerToEdit.TransitionPoint1 = IsSection2Visible ? TransitionPoint1 : null;
                propellerToEdit.A2 = IsSection2Visible ? A2 : null;
                propellerToEdit.B2 = IsSection2Visible ? B2 : null;

                propellerToEdit.TransitionPoint2 = IsSection3Visible ? TransitionPoint2 : null;
                propellerToEdit.A3 = IsSection3Visible ? A3 : null;
                propellerToEdit.B3 = IsSection3Visible ? B3 : null;

                await db.SaveChangesAsync();

                var updatedUiModel = new Propeller
                {
                    PropellerID = SelectedPropeller.PropellerID,
                    PropellerName = PropellerName,              // نام جدید
                    Mode = SelectedMode,                        // مد جدید
                    A1 = A1,
                    B1 = B1,
                    TransitionPoint1 = IsSection2Visible ? TransitionPoint1 : null,
                    A2 = IsSection2Visible ? A2 : null,
                    B2 = IsSection2Visible ? B2 : null,
                    TransitionPoint2 = IsSection3Visible ? TransitionPoint2 : null,
                    A3 = IsSection3Visible ? A3 : null,
                    B3 = IsSection3Visible ? B3 : null,
                };

                // پیدا کردن جایگاه آیتم در لیست
                int index = Propellers.IndexOf(SelectedPropeller);

                if (index != -1)
                {
                    // جایگزینی با شیء جدید -> این خط باعث آپدیت آنی در ListView می‌شود
                    Propellers[index] = updatedUiModel;
                }

                ResetInputs();
                ShowSuccess("ویرایش با موفقیت انجام شد.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"خطا در ثبت: {ex.Message}");
        }
    }

    // دکمه مداد در لیست
    [RelayCommand]
    private void PrepareForEdit(int id)
    {
        var propeller = Propellers.FirstOrDefault(p => p.PropellerID == id);
        if (propeller != null)
        {
            SelectedPropeller = propeller; // تریگر کردن OnSelectedLocationChanged
            IsErrorVisible = false;
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedPropeller = null;
        PropellerName = string.Empty;
        SelectedOption = CalibrationOptions.First(); // برگشت به حالت پیش‌فرض گزینه اول

        A1 = 0; B1 = 0;
        TransitionPoint1 = null; A2 = null; B2 = null;
        TransitionPoint2 = null; A3 = null; B3 = null;

        AddEditBtnContent = "ذخیره";
        IsErrorVisible = false;
    }

    private void ResetInputs()
    {
        SelectedPropeller = null;
        PropellerName = string.Empty;
        SelectedOption = CalibrationOptions.First();
        A1 = 0; B1 = 0;
        TransitionPoint1 = null; A2 = null; B2 = null;
        TransitionPoint2 = null; A3 = null; B3 = null;
        AddEditBtnContent = "ذخیره";
    }

    [RelayCommand]
    private async Task DeletePropellerAsync(int id)
    {
        if (id <= 0) return;

        try
        {
            using var db = new WaterAssessmentContext();
            var propellerToDelete = await db.Propellers.FindAsync(id);

            // آیا مکانی وجود دارد که مربوط به این حوزه باشد؟
            bool hasDependents = await db.Assessments.AnyAsync(l => l.PropellerID == id);

            if (hasDependents)
            {
                // اگر زیرمجموعه داشت، خطا بده و خارج شو
                ShowError("این پروانه دارای اندازه گیری های ثبت شده است. برای حذف، ابتدا باید اندازه گیری ‌های مربوطه را حذف کنید.");
                return;
            }

            if (propellerToDelete != null)
            {
                db.Propellers.Remove(propellerToDelete);
                await db.SaveChangesAsync();

                // حذف از لیست UI
                var propellerInList = Propellers.FirstOrDefault(e => e.PropellerID == id);
                if (propellerInList != null)
                {
                    Propellers.Remove(propellerInList);
                }

                // بازخورد
                ShowSuccess("پروانه با موفقیت حذف شد.");

                // اگر آیتم در حال ویرایش حذف شد، فرم پاک شود
                if (SelectedPropeller != null && SelectedPropeller.PropellerID == id)
                {
                    ClearForm();
                    ShowSuccess("پروانه با موفقیت حذف شد.");
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

    private async Task LoadPropellersAsync()
    {
        using var db = new WaterAssessmentContext();
        var list = await db.Propellers.AsNoTracking().ToListAsync();
        Propellers.Clear();
        foreach (var item in list) Propellers.Add(item);
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
public record CalibrationOption(string Title, CalibrationMode Mode);
