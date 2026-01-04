using System.Collections.Specialized;

namespace WaterAssessment.Views;

public sealed partial class EmployeePage : Page
{
    public EmployeeViewModel ViewModel { get; } = new();

    public EmployeePage()
    {
        this.InitializeComponent();
        //DataContext = ViewModel;
        //Loaded += EmployeePage_Loaded;
        ViewModel.Employees.CollectionChanged += Employees_CollectionChanged;
    }

    // رویداد تغییر لیست (حذف/اضافه)
    private void Employees_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

    // این متد فقط شماره سطرهایی که الان دیده می‌شوند را اصلاح می‌کند
    private void UpdateVisibleIndices()
    {
        // حلقه روی تمام آیتم‌های موجود در لیست
        for (int i = 0; i < ViewModel.Employees.Count; i++)
        {
            var item = ViewModel.Employees[i];

            // تلاش برای گرفتن کانتینر (سطر گرافیکی) مربوط به این آیتم
            // اگر آیتم خارج از دید باشد (اسکرول شده باشد)، مقدار null برمی‌گردد که مشکلی نیست
            var container = employeeListView.ContainerFromItem(item) as DependencyObject;

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

    // این متد هر بار که یک ردیف می‌خواهد نمایش داده شود (یا اسکرول شود) اجرا می‌شود
    private void employeeListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
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

    // متد کمکی برای جستجو در ویژوال تری (Visual Tree)
    // این متد داخل لایه‌های تودرتو می‌گردد تا کنترلی با نام مشخص را پیدا کند
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

    private void employeeSwipeContainer_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse ||
            e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
        {
            VisualStateManager.GoToState(
                sender as Control, "HoverButtonsShown", true);
        }
    }

    private void employeeSwipeContainer_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(
            sender as Control, "HoverButtonsHidden", true);
    }
}