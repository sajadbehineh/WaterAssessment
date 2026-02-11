using System.Windows.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WaterAssessment.Controls;

public sealed partial class PaginationControl : UserControl
{
    public PaginationControl()
    {
        InitializeComponent();
    }

    // پراپرتی برای دریافت تعداد کل آیتم‌ها
    public int TotalItemCount
    {
        get { return (int)GetValue(TotalItemCountProperty); }
        set { SetValue(TotalItemCountProperty, value); }
    }
    public static readonly DependencyProperty TotalItemCountProperty =
        DependencyProperty.Register(nameof(TotalItemCount), typeof(int), typeof(PaginationControl), new PropertyMetadata(0, OnPagingPropertyChanged));

    // پراپرتی برای سایز صفحه
    public int PageSize
    {
        get { return (int)GetValue(PageSizeProperty); }
        set { SetValue(PageSizeProperty, value); }
    }
    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(nameof(PageSize), typeof(int), typeof(PaginationControl), new PropertyMetadata(1, OnPagingPropertyChanged));

    // پراپرتی برای صفحه فعلی (با قابلیت TwoWay Binding)
    public int CurrentPage
    {
        get { return (int)GetValue(CurrentPageProperty); }
        set { SetValue(CurrentPageProperty, value); }
    }
    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(PaginationControl), new PropertyMetadata(1, OnPagingPropertyChanged));

    // کامندی که هنگام تغییر صفحه اجرا می‌شود
    public ICommand PageChangedCommand
    {
        get { return (ICommand)GetValue(PageChangedCommandProperty); }
        set { SetValue(PageChangedCommandProperty, value); }
    }
    public static readonly DependencyProperty PageChangedCommandProperty =
        DependencyProperty.Register(nameof(PageChangedCommand), typeof(ICommand), typeof(PaginationControl), new PropertyMetadata(null));

    // پراپرتی‌های داخلی برای فعال/غیرفعال کردن دکمه‌ها
    public int TotalPages => (PageSize > 0) ? (int)Math.Ceiling(TotalItemCount / (double)PageSize) : 0;
    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage < TotalPages;

    // این متد هر زمان پراپرتی‌های اصلی تغییر کنند، UI را آپدیت می‌کند
    private static void OnPagingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as PaginationControl;
        control.Bindings.Update(); // آپدیت کردن Binding های x:Bind
    }

    private void FirstPage_Click(object sender, RoutedEventArgs e)
    {
        CurrentPage = 1;
        PageChangedCommand?.Execute(CurrentPage);
    }

    private void PreviousPage_Click(object sender, RoutedEventArgs e)
    {
        CurrentPage--;
        PageChangedCommand?.Execute(CurrentPage);
    }

    private void NextPage_Click(object sender, RoutedEventArgs e)
    {
        CurrentPage++;
        PageChangedCommand?.Execute(CurrentPage);
    }

    private void LastPage_Click(object sender, RoutedEventArgs e)
    {
        CurrentPage = TotalPages;
        PageChangedCommand?.Execute(CurrentPage);
    }
}
