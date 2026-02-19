using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;

namespace WaterAssessment.Views;

public sealed partial class LocationPage : Page
{
    public AreaViewModel AreaViewModel { get; }
    public LocationViewModel LocationViewModel { get; }

    public LocationTypeViewModel LocationTypeViewModel { get; }

    public LocationPage()
    {
        this.InitializeComponent();
        this.LocationViewModel = App.Services.GetRequiredService<LocationViewModel>();
        this.AreaViewModel = App.Services.GetRequiredService<AreaViewModel>();
        this.LocationTypeViewModel = App.Services.GetRequiredService<LocationTypeViewModel>();
        //AreaViewModel.Areas.CollectionChanged += Areas_CollectionChanged;
        //LocationViewModel.Locations.CollectionChanged += Locations_CollectionChanged;
        //LocationTypeViewModel.LocationTypes.CollectionChanged += LocationTypes_CollectionChanged;
    }

    private void LocationTypes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove ||
            e.Action == NotifyCollectionChangedAction.Add ||
            e.Action == NotifyCollectionChangedAction.Reset)
        {
            // اجرای متد بروزرسانی روی ترد اصلی UI
            DispatcherQueue.TryEnqueue(UpdateVisibleIndices);
        }
    }

    // رویداد تغییر لیست (حذف/اضافه)
    private void Areas_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // فقط اگر چیزی اضافه، حذف یا لیست ریست شد، شماره‌ها را آپدیت کن
        if (e.Action == NotifyCollectionChangedAction.Remove ||
            e.Action == NotifyCollectionChangedAction.Add ||
            e.Action == NotifyCollectionChangedAction.Reset)
        {
            // اجرای متد بروزرسانی روی ترد اصلی UI
            DispatcherQueue.TryEnqueue(UpdateVisibleIndices);
        }
    }

    private void Locations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // فقط اگر چیزی اضافه، حذف یا لیست ریست شد، شماره‌ها را آپدیت کن
        if (e.Action == NotifyCollectionChangedAction.Remove ||
            e.Action == NotifyCollectionChangedAction.Add ||
            e.Action == NotifyCollectionChangedAction.Reset)
        {
            // اجرای متد بروزرسانی روی ترد اصلی UI
            DispatcherQueue.TryEnqueue(UpdateLocationVisibleIndices);
        }
    }

    private void UpdateVisibleIndices()
    {
        // حلقه روی تمام آیتم‌های موجود در لیست
        for (int i = 0; i < AreaViewModel.Areas.Count; i++)
        {
            var item = AreaViewModel.Areas[i];

            // تلاش برای گرفتن کانتینر (سطر گرافیکی) مربوط به این آیتم
            // اگر آیتم خارج از دید باشد (اسکرول شده باشد)، مقدار null برمی‌گردد که مشکلی نیست
            var container = AreaListView.ContainerFromItem(item) as DependencyObject;

            if (container != null)
            {
                var indexBlock = FindChild<TextBlock>(container, "IndexTextBlock");
                if (indexBlock != null)
                {
                    // اصلاح شماره ردیف
                    indexBlock.Text = (i + 1).ToString();
                }
            }
        }
    }
    private void UpdateLocationVisibleIndices()
    {
        // حلقه روی تمام آیتم‌های موجود در لیست
        for (int i = 0; i < LocationViewModel.Locations.Count; i++)
        {
            var item = LocationViewModel.Locations[i];

            // تلاش برای گرفتن کانتینر (سطر گرافیکی) مربوط به این آیتم
            // اگر آیتم خارج از دید باشد (اسکرول شده باشد)، مقدار null برمی‌گردد که مشکلی نیست
            var container = locationsListView.ContainerFromItem(item) as DependencyObject;

            if (container != null)
            {
                var indexBlock = FindChild<TextBlock>(container, "LocationIndexTextBlock");
                if (indexBlock != null)
                {
                    // اصلاح شماره ردیف
                    indexBlock.Text = (i + 1).ToString();
                }
            }
        }
    }

    private void UpdateLocationTypeVisibleIndices()
    {
        // حلقه روی تمام آیتم‌های موجود در لیست
        for (int i = 0; i < LocationTypeViewModel.LocationTypes.Count; i++)
        {
            var item = LocationTypeViewModel.LocationTypes[i];

            // تلاش برای گرفتن کانتینر (سطر گرافیکی) مربوط به این آیتم
            // اگر آیتم خارج از دید باشد (اسکرول شده باشد)، مقدار null برمی‌گردد که مشکلی نیست
            var container = LocationTypeListView.ContainerFromItem(item) as DependencyObject;

            if (container != null)
            {
                var indexBlock = FindChild<TextBlock>(container, "LocationIndexTextBlock");
                if (indexBlock != null)
                {
                    // اصلاح شماره ردیف
                    indexBlock.Text = (i + 1).ToString();
                }
            }
        }
    }

    private void areaListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        // اگر آیتم در حال بازیافت است، کاری نکن (برای پرفورمنس)
        if (args.InRecycleQueue) return;

        // ریشه تمپلیت شما یک UserControl است (طبق کد شما)
        var root = args.ItemContainer.ContentTemplateRoot as DependencyObject;

        // تلاش برای پیدا کردن تکست‌باکس با نام "IndexTextBlock"
        var indexBlock = FindChild<TextBlock>(root, "IndexTextBlock");

        if (indexBlock != null)
        {
            // مقداردهی شماره ردیف (ایندکس از 0 شروع می‌شود، پس +1 می‌کنیم)
            indexBlock.Text = (args.ItemIndex + 1).ToString();
        }
    }

    private void locationTypeListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        // اگر آیتم در حال بازیافت است، کاری نکن (برای پرفورمنس)
        if (args.InRecycleQueue) return;

        // ریشه تمپلیت شما یک UserControl است (طبق کد شما)
        var root = args.ItemContainer.ContentTemplateRoot as DependencyObject;

        // تلاش برای پیدا کردن تکست‌باکس با نام "IndexTextBlock"
        var indexBlock = FindChild<TextBlock>(root, "LocationTypeIndexTextBlock");

        if (indexBlock != null)
        {
            // مقداردهی شماره ردیف (ایندکس از 0 شروع می‌شود، پس +1 می‌کنیم)
            indexBlock.Text = (args.ItemIndex + 1).ToString();
        }
    }

    private void locationListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        // اگر آیتم در حال بازیافت است، کاری نکن (برای پرفورمنس)
        if (args.InRecycleQueue) return;

        // ریشه تمپلیت شما یک UserControl است (طبق کد شما)
        var root = args.ItemContainer.ContentTemplateRoot as DependencyObject;

        // تلاش برای پیدا کردن تکست‌باکس با نام "IndexTextBlock"
        var indexBlock = FindChild<TextBlock>(root, "LocationIndexTextBlock");

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

    private void areaSwipeContainer_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse ||
            e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
        {
            VisualStateManager.GoToState(
                sender as Control, "HoverButtonsShown", true);
        }
    }

    private void areaSwipeContainer_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(
            sender as Control, "HoverButtonsHidden", true);
    }

    private void locationTypeSwipeContainer_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse ||
            e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
        {
            VisualStateManager.GoToState(
                sender as Control, "HoverButtonsShown", true);
        }
    }

    private void locationTypeSwipeContainer_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(
            sender as Control, "HoverButtonsHidden", true);
    }
}
