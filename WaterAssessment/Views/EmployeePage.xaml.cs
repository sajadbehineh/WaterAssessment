using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;

namespace WaterAssessment.Views;

public sealed partial class EmployeePage : Page
{
    private Employee _lastHoveredItem;
    public EmployeeViewModel ViewModel { get; }

    public EmployeePage()
    {
        this.InitializeComponent();
        this.ViewModel = App.Services.GetRequiredService<EmployeeViewModel>();
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

    private void BtnHoverDeleteEmployee_OnClick(object sender, RoutedEventArgs e)
    {
        var button = sender as FrameworkElement;
        if (button == null) return;

        var employeeToDelete = button.DataContext as Employee;
        if (employeeToDelete == null) return;

        if (this.ViewModel != null && this.ViewModel.RequestDeleteEmployeeCommand.CanExecute(employeeToDelete))
        {
            this.ViewModel.RequestDeleteEmployeeCommand.Execute(employeeToDelete);
        }
    }

    private T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        DependencyObject parent = VisualTreeHelper.GetParent(child);

        while (parent != null)
        {
            if (parent is T correctlyTyped)
                return correctlyTyped;

            parent = VisualTreeHelper.GetParent(parent);
        }

        return null;
    }

    private void OnEmployeeDeleteClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Employee item)
        {
            ViewModel.RequestDeleteEmployeeCommand.Execute(item);
        }
    }
}