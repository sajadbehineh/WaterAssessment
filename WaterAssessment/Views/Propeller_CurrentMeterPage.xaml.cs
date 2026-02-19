using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;

namespace WaterAssessment.Views;

public sealed partial class Propeller_CurrentMeterPage : Page
{
    public PropellerViewModel PropellerViewModel { get; }
    public CurrentMeterViewModel CurrentMeterViewModel { get; }

    public Propeller_CurrentMeterPage()
    {
        this.InitializeComponent();
        DataContext = this;
        CurrentMeterViewModel = App.Services.GetRequiredService<CurrentMeterViewModel>();
        PropellerViewModel = App.Services.GetRequiredService<PropellerViewModel>();
        //PropellerViewModel.Propellers.CollectionChanged += Propeller_CollectionChanged;
        //CurrentMeterViewModel.CurrentMeters.CollectionChanged += CurrentMeter_CollectionChanged;
    }

    private void Propeller_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // فقط اگر چیزی اضافه، حذف یا لیست ریست شد، شماره‌ها را آپدیت کن
        if (e.Action == NotifyCollectionChangedAction.Remove ||
            e.Action == NotifyCollectionChangedAction.Add ||
            e.Action == NotifyCollectionChangedAction.Reset)
        {
            // اجرای متد بروزرسانی روی ترد اصلی UI
            DispatcherQueue.TryEnqueue(UpdatePropellerVisibleIndices);
        }
    }

    private void UpdatePropellerVisibleIndices()
    {
        // حلقه روی تمام آیتم‌های موجود در لیست
        for (int i = 0; i < PropellerViewModel.Propellers.Count; i++)
        {
            var item = PropellerViewModel.Propellers[i];

            // تلاش برای گرفتن کانتینر (سطر گرافیکی) مربوط به این آیتم
            // اگر آیتم خارج از دید باشد (اسکرول شده باشد)، مقدار null برمی‌گردد که مشکلی نیست
            var container = PropellerListView.ContainerFromItem(item) as DependencyObject;

            if (container != null)
            {
                var indexBlock = FindChild<TextBlock>(container, "PropellerIndexTextBlock");
                if (indexBlock != null)
                {
                    // اصلاح شماره ردیف
                    indexBlock.Text = (i + 1).ToString();
                }
            }
        }
    }

    private void PropellerListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        // اگر آیتم در حال بازیافت است، کاری نکن (برای پرفورمنس)
        if (args.InRecycleQueue) return;

        // ریشه تمپلیت شما یک UserControl است (طبق کد شما)
        var root = args.ItemContainer.ContentTemplateRoot as DependencyObject;

        // تلاش برای پیدا کردن تکست‌باکس با نام "IndexTextBlock"
        var indexBlock = FindChild<TextBlock>(root, "PropellerIndexTextBlock");

        if (indexBlock != null)
        {
            // مقداردهی شماره ردیف (ایندکس از 0 شروع می‌شود، پس +1 می‌کنیم)
            indexBlock.Text = (args.ItemIndex + 1).ToString();
        }
    }

    private void CurrentMeter_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // فقط اگر چیزی اضافه، حذف یا لیست ریست شد، شماره‌ها را آپدیت کن
        if (e.Action == NotifyCollectionChangedAction.Remove ||
            e.Action == NotifyCollectionChangedAction.Add ||
            e.Action == NotifyCollectionChangedAction.Reset)
        {
            // اجرای متد بروزرسانی روی ترد اصلی UI
            DispatcherQueue.TryEnqueue(UpdateCurrentMeterVisibleIndices);
        }
    }

    private void UpdateCurrentMeterVisibleIndices()
    {
        // حلقه روی تمام آیتم‌های موجود در لیست
        for (int i = 0; i < CurrentMeterViewModel.CurrentMeters.Count; i++)
        {
            var item = CurrentMeterViewModel.CurrentMeters[i];

            // تلاش برای گرفتن کانتینر (سطر گرافیکی) مربوط به این آیتم
            // اگر آیتم خارج از دید باشد (اسکرول شده باشد)، مقدار null برمی‌گردد که مشکلی نیست
            var container = CurrentMeterListView.ContainerFromItem(item) as DependencyObject;

            if (container != null)
            {
                var indexBlock = FindChild<TextBlock>(container, "CurrentMeterIndexTextBlock");
                if (indexBlock != null)
                {
                    // اصلاح شماره ردیف
                    indexBlock.Text = (i + 1).ToString();
                }
            }
        }
    }

    private void CurrentMeterListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        // اگر آیتم در حال بازیافت است، کاری نکن (برای پرفورمنس)
        if (args.InRecycleQueue) return;

        // ریشه تمپلیت شما یک UserControl است (طبق کد شما)
        var root = args.ItemContainer.ContentTemplateRoot as DependencyObject;

        // تلاش برای پیدا کردن تکست‌باکس با نام "IndexTextBlock"
        var indexBlock = FindChild<TextBlock>(root, "CurrentMeterIndexTextBlock");

        if (indexBlock != null)
        {
            // مقداردهی شماره ردیف (ایندکس از 0 شروع می‌شود، پس +1 می‌کنیم)
            indexBlock.Text = (args.ItemIndex + 1).ToString();
        }
    }

    private static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
    {
        if (parent == null) return null;

        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            // بررسی نام و نوع کنترل
            if (child is FrameworkElement fe && fe.Name == childName && child is T typedChild)
            {
                return typedChild;
            }

            // جستجوی بازگشتی (Recursive) در فرزندان
            var foundChild = FindChild<T>(child, childName);
            if (foundChild != null) return foundChild;
        }
        return null;
    }

    private void SwipeContainer_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse ||
            e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
        {
            VisualStateManager.GoToState(
                sender as Control, "HoverButtonsShown", true);
        }
    }

    private void SwipeContainer_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(
            sender as Control, "HoverButtonsHidden", true);
    }
}