using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Windows.Forms;
using WaterAssessment.Services;

namespace WaterAssessment.Models.ViewModel;

public partial class PropellerViewModel : ObservableObject
{
    private readonly IPropellerService _propellerService;
    public ObservableCollection<Propeller> Propellers { get; } = new();

    // لیست گزینه‌ها برای ComboBox انتخاب نوع کالیبراسیون
    public List<CalibrationMode> CalibrationModes { get; } = Enum.GetValues(typeof(CalibrationMode)).Cast<CalibrationMode>().ToList();

    public List<CalibrationOption> CalibrationOptions { get; } = new()
    {
        new CalibrationOption("یک معادله‌ای", CalibrationMode.OneEquation),
        new CalibrationOption("دو معادله‌ای", CalibrationMode.TwoEquations),
        new CalibrationOption("سه معادله‌ای", CalibrationMode.ThreeEquations)
    };

    [ObservableProperty] private Propeller? _selectedPropeller;
    [ObservableProperty] private string _propellerName = string.Empty;

    // ==========================================
    // ورودی‌های فرم
    // ==========================================


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

    [ObservableProperty] private string _addEditBtnContent = "ذخیره";
    [ObservableProperty] private bool _isErrorVisible;
    [ObservableProperty] private InfoBarSeverity _infoBarSeverity;
    [ObservableProperty] private string _infoBarMessage = string.Empty;

    public PropellerViewModel(IPropellerService propellerService)
    {
        _propellerService = propellerService;
        SelectedOption = CalibrationOptions.First();
        _ = LoadPropellersAsync();
    }

    // وقتی یک پروانه برای ویرایش انتخاب می‌شود
    partial void OnSelectedPropellerChanged(Propeller? value)
    {
        if (value != null)
        {
            PropellerName = value.PropellerName;
            SelectedOption = CalibrationOptions.FirstOrDefault(x => x.Mode == value.Mode)
                             ?? CalibrationOptions.First();

            A1 = value.A1; B1 = value.B1; TransitionPoint1 = value.TransitionPoint1;
            A2 = value.A2; B2 = value.B2; TransitionPoint2 = value.TransitionPoint2;
            A3 = value.A3; B3 = value.B3;
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
        var propellerModel = new Propeller
        {
            PropellerID = SelectedPropeller?.PropellerID ?? 0,
            PropellerName = PropellerName,
            Mode = SelectedMode,
            A1 = A1,
            B1 = B1,
            // ارسال null برای فیلدهایی که در UI مخفی هستند
            TransitionPoint1 = IsSection2Visible ? TransitionPoint1 : null,
            A2 = IsSection2Visible ? A2 : null,
            B2 = IsSection2Visible ? B2 : null,
            TransitionPoint2 = IsSection3Visible ? TransitionPoint2 : null,
            A3 = IsSection3Visible ? A3 : null,
            B3 = IsSection3Visible ? B3 : null
        };
        bool success;
        if (SelectedPropeller == null)
        {
            success = await _propellerService.AddNewPropellerAsync(propellerModel);
        }
        else
        {
            success = await _propellerService.UpdatePropellerAsync(propellerModel);
        }

        if (success)
        {
            await LoadPropellersAsync(); // رفرش لیست
            ClearForm();
            _ = ShowSuccess("عملیات با موفقیت انجام شد.");
        }
        else
        {
            _ = ShowError(_propellerService.GetLastErrorMessage());
        }
        //if (string.IsNullOrWhiteSpace(PropellerName)) return;

        //// اعتبارسنجی نقاط شکست (T1 باید کوچکتر از T2 باشد)
        //if (SelectedMode == CalibrationMode.ThreeEquations &&
        //    TransitionPoint1.HasValue && TransitionPoint2.HasValue &&
        //    TransitionPoint1 >= TransitionPoint2)
        //{
        //    ShowError("نقطه شکست اول باید کوچکتر از نقطه شکست دوم باشد.");
        //    return;
        //}

        //try
        //{
        //    using var db = new WaterAssessmentContext();
        //    var modeToSave = SelectedOption.Mode;
        //    if (SelectedPropeller == null)
        //    {
        //        // افزودن جدید
        //        var newProp = new Propeller
        //        {
        //            PropellerName = PropellerName,
        //            Mode = modeToSave,
        //            A1 = A1,
        //            B1 = B1,
        //            // فقط اگر مد مربوطه فعال باشد مقادیر را ذخیره می‌کنیم
        //            TransitionPoint1 = IsSection2Visible ? TransitionPoint1 : null,
        //            A2 = IsSection2Visible ? A2 : null,
        //            B2 = IsSection2Visible ? B2 : null,
        //            TransitionPoint2 = IsSection3Visible ? TransitionPoint2 : null,
        //            A3 = IsSection3Visible ? A3 : null,
        //            B3 = IsSection3Visible ? B3 : null
        //        };
        //        db.Propellers.Add(newProp);
        //        await db.SaveChangesAsync();
        //        Propellers.Add(newProp);

        //        ResetInputs();
        //        ShowSuccess("پروانه مورد نظر با موفقیت ثبت شد.");
        //    }
        //    else
        //    {
        //        // ویرایش
        //        var propellerToEdit = await db.Propellers.FindAsync(SelectedPropeller.PropellerID);
        //        if (propellerToEdit == null)
        //        {
        //            ShowError("این رکورد یافت نشد");
        //            return;
        //        }

        //        bool isDuplicate = await db.Propellers
        //            .AnyAsync(a => a.PropellerName == PropellerName && a.PropellerID != SelectedPropeller.PropellerID);
        //        if (isDuplicate)
        //        {
        //            ShowError("پروانه وارد شده تکراری است.");
        //            return;
        //        }

        //        propellerToEdit.PropellerName = PropellerName;
        //        propellerToEdit.Mode = SelectedMode;
        //        propellerToEdit.A1 = A1; propellerToEdit.B1 = B1;

        //        propellerToEdit.TransitionPoint1 = IsSection2Visible ? TransitionPoint1 : null;
        //        propellerToEdit.A2 = IsSection2Visible ? A2 : null;
        //        propellerToEdit.B2 = IsSection2Visible ? B2 : null;

        //        propellerToEdit.TransitionPoint2 = IsSection3Visible ? TransitionPoint2 : null;
        //        propellerToEdit.A3 = IsSection3Visible ? A3 : null;
        //        propellerToEdit.B3 = IsSection3Visible ? B3 : null;

        //        await db.SaveChangesAsync();

        //        var updatedUiModel = new Propeller
        //        {
        //            PropellerID = SelectedPropeller.PropellerID,
        //            PropellerName = PropellerName,              // نام جدید
        //            Mode = SelectedMode,                        // مد جدید
        //            A1 = A1,
        //            B1 = B1,
        //            TransitionPoint1 = IsSection2Visible ? TransitionPoint1 : null,
        //            A2 = IsSection2Visible ? A2 : null,
        //            B2 = IsSection2Visible ? B2 : null,
        //            TransitionPoint2 = IsSection3Visible ? TransitionPoint2 : null,
        //            A3 = IsSection3Visible ? A3 : null,
        //            B3 = IsSection3Visible ? B3 : null,
        //        };

        //        // پیدا کردن جایگاه آیتم در لیست
        //        int index = Propellers.IndexOf(SelectedPropeller);

        //        if (index != -1)
        //        {
        //            // جایگزینی با شیء جدید -> این خط باعث آپدیت آنی در ListView می‌شود
        //            Propellers[index] = updatedUiModel;
        //        }

        //        ResetInputs();
        //        ShowSuccess("ویرایش با موفقیت انجام شد.");
        //    }
        //}
        //catch (Exception ex)
        //{
        //    ShowError($"خطا در ثبت: {ex.Message}");
        //}
    }

    // دکمه مداد در لیست
    //[RelayCommand]
    //private void PrepareForEdit(int id)
    //{
    //    var propeller = Propellers.FirstOrDefault(p => p.PropellerID == id);
    //    if (propeller != null)
    //    {
    //        SelectedPropeller = propeller; // تریگر کردن OnSelectedLocationChanged
    //        IsErrorVisible = false;
    //    }
    //}

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

    //private void ResetInputs()
    //{
    //    SelectedPropeller = null;
    //    PropellerName = string.Empty;
    //    SelectedOption = CalibrationOptions.First();
    //    A1 = 0; B1 = 0;
    //    TransitionPoint1 = null; A2 = null; B2 = null;
    //    TransitionPoint2 = null; A3 = null; B3 = null;
    //    AddEditBtnContent = "ذخیره";
    //    IsErrorVisible = false;
    //}

    [RelayCommand]
    private async Task DeletePropellerAsync(int id)
    {
        var success = await _propellerService.DeletePropellerAsync(id);
        if (success)
        {
            if (SelectedPropeller?.PropellerID == id) ClearForm();
            await LoadPropellersAsync();
            _ = ShowSuccess("پروانه با موفقیت حذف شد.");
        }
        else
        {
            _ = ShowError(_propellerService.GetLastErrorMessage());
        }
    }

    private async Task LoadPropellersAsync()
    {
        var list = await _propellerService.GetAllPropellersAsync();
        Propellers.Clear();
        foreach (var propeller in list) Propellers.Add(propeller);
    }

    private async Task ShowError(string msg, int durationSeconds = 4)
    {
        InfoBarMessage = msg; InfoBarSeverity = InfoBarSeverity.Error; IsErrorVisible = true;
        await Task.Delay(durationSeconds * 1000);

        // فقط اگر پیام فعلی همان پیامی است که نمایش داده بودیم، آن را ببند
        if (InfoBarMessage == msg)
        {
            IsErrorVisible = false;
        }
    }

    private async Task ShowSuccess(string msg, int durationSeconds = 4)
    {
        InfoBarMessage = msg; InfoBarSeverity = InfoBarSeverity.Success; IsErrorVisible = true;
        await Task.Delay(durationSeconds * 1000);

        // فقط اگر پیام فعلی همان پیامی است که نمایش داده بودیم، آن را ببند
        if (InfoBarMessage == msg)
        {
            IsErrorVisible = false;
        }
    }
}
public record CalibrationOption(string Title, CalibrationMode Mode);
