using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WaterAssessment.Services;

namespace WaterAssessment.ViewModel;

public partial class PropellerViewModel : PagedViewModelBase<Propeller>
{
    private readonly IPropellerService _propellerService;
    private readonly IDialogService _dialogService;

    public ObservableCollection<Propeller> Propellers => PagedItems;
    public int TotalPropellers => TotalItems;

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

    public PropellerViewModel(IPropellerService propellerService, IDialogService dialogService) : base(pageSize: 10)
    {
        _propellerService = propellerService;
        _dialogService = dialogService;
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

    [RelayCommand]
    private async Task RequestDeletePropellerAsync(Propeller propeller)
    {
        if (propeller == null) return;

        // از سرویس دیالوگ برای نمایش پیغام تایید استفاده کنید
        bool confirmed = await _dialogService.ShowConfirmationDialogAsync(
            title: "تأیید عملیات حذف",
            content: $"آیا از حذف پروانه «{propeller.PropellerName}» اطمینان دارید؟\nاین عملیات غیرقابل بازگشت است.",
            primaryButtonText: "بله، حذف کن",
            closeButtonText: "انصراف"
        );

        // فقط در صورت تایید کاربر، حذف را ادامه دهید
        if (confirmed)
        {
            await DeletePropellerAsync(propeller);
        }
    }

    [RelayCommand]
    private async Task DeletePropellerAsync(Propeller propeller)
    {
        var success = await _propellerService.DeletePropellerAsync(propeller.PropellerID);
        if (success)
        {
            if (SelectedPropeller?.PropellerID == propeller.PropellerID) ClearForm();
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
        var propellersResult = await _propellerService.GetAllPropellersAsync();
        SetItems(propellersResult);
        OnPropertyChanged(nameof(TotalPropellers));
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
