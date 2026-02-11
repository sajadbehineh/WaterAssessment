using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;

namespace WaterAssessment.Views
{
    public sealed partial class UserManagementPage : Page
    {
        public UserManagementViewModel ViewModel { get; }
        public UserManagementPage()
        {
            InitializeComponent();
            ViewModel = App.Services.GetRequiredService<UserManagementViewModel>();
            //ViewModel.Users.CollectionChanged += Users_CollectionChanged;
            DataContext = ViewModel;
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserManagementViewModel viewModel)
            {
                viewModel.AddUserCommand.NotifyCanExecuteChanged();
            }
        }

        private void Users_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Reset)
            {
                // اجرای متد بروزرسانی روی ترد اصلی UI
                DispatcherQueue.TryEnqueue(UpdateVisibleIndices);
            }
        }

        private void UpdateVisibleIndices()
        {
            // حلقه روی تمام آیتم‌های موجود در لیست
            for (int i = 0; i < ViewModel.Users.Count; i++)
            {
                var item = ViewModel.Users[i];

                // تلاش برای گرفتن کانتینر (سطر گرافیکی) مربوط به این آیتم
                // اگر آیتم خارج از دید باشد (اسکرول شده باشد)، مقدار null برمی‌گردد که مشکلی نیست
                var container = UserListView.ContainerFromItem(item) as DependencyObject;

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

        private void UserListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
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

        private void UserSwipeContainer_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse ||
                e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
            {
                VisualStateManager.GoToState(
                    sender as Control, "HoverButtonsShown", true);
            }
        }

        private void UserSwipeContainer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(
                sender as Control, "HoverButtonsHidden", true);
        }
    }
}
