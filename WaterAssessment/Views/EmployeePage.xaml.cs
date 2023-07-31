using Microsoft.EntityFrameworkCore;

namespace WaterAssessment.Views;

public sealed partial class EmployeePage : Page
{
    public ObservableCollection<Employee> Employees { get; set; } = new ObservableCollection<Employee>();

    private bool _isEdited = false;

    public EmployeePage()
    {
        this.InitializeComponent();
        DataContext = this;
        Loaded += EmployeePage_Loaded;
    }

    private void EmployeePage_Loaded(object sender, RoutedEventArgs e)
    {
        GetEmployeesFromDB();
    }

    private void GetEmployeesFromDB()
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            Employees?.Clear();
            await using var db = new WaterAssessmentContext();
            var data = await db.Employees.ToListAsync();
            Employees = new ObservableCollection<Employee>(data);
            employeeListView.ItemsSource = Employees;
        });
    }

    private void employeeTextBoxes_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var currentTxt = (sender as TextBox).Text;
        btnClearEmployeeBox.IsEnabled = !string.IsNullOrWhiteSpace(currentTxt);
    }

    private void BtnAddEmployee_OnClick(object sender, RoutedEventArgs e)
    {
        var btnAdd = sender as Button;
        using var db = new WaterAssessmentContext();
        InitInfoBar.ImplementInfoBar(firstNameBoxInfoBar, InfoBarSeverity.Error,
            string.IsNullOrWhiteSpace(firstNameBox.Text), "لطفاً نام را وارد کنید.");
        InitInfoBar.ImplementInfoBar(lastNameBoxInfoBar, InfoBarSeverity.Error,
            string.IsNullOrWhiteSpace(lastNameBox.Text), "لطفاً نام خانوادگی را وارد کنید.");

        if (!string.IsNullOrWhiteSpace(firstNameBox.Text) && !string.IsNullOrWhiteSpace(lastNameBox.Text))
        {
            Employee newEmployee = new Employee
            {
                FirstName = firstNameBox.Text,
                LastName = lastNameBox.Text
            };
            var duplicate = db.Employees
                .Where(emp => emp.FirstName == newEmployee.FirstName && emp.LastName == newEmployee.LastName)
                .FirstOrDefault();

            if (duplicate != null)
            {
                InitInfoBar.ImplementInfoBar(firstNameBoxInfoBar, InfoBarSeverity.Error, true,
                    $"{duplicate.FirstName} {duplicate.LastName} قبلاً ثبت شده است");
                return;
            }

            if (_isEdited && btnAdd.DataContext is Employee selectedEmployee)
            {
                Employee employee = db.Employees.Find(selectedEmployee.EmployeeID);
                if (employee != null)
                {
                    employee.FirstName = newEmployee.FirstName;
                    employee.LastName = newEmployee.LastName;
                    db.SaveChanges();
                    _isEdited = false;
                    GetEmployeesFromDB();
                    firstNameBox.Text = string.Empty;
                    lastNameBox.Text = string.Empty;
                    btnAdd.Content = "ذخیره";
                    InitInfoBar.ImplementInfoBar(firstNameBoxInfoBar, InfoBarSeverity.Success,
                        true, "همکار مورد نظر شما با موفقیت ویرایش شد.");
                    firstNameBox.Focus(FocusState.Pointer);
                }
            }
            else
            {
                db.Employees.Add(newEmployee);
                db.SaveChanges();
                GetEmployeesFromDB();
                firstNameBox.Text = string.Empty;
                lastNameBox.Text = string.Empty;
                InitInfoBar.ImplementInfoBar(firstNameBoxInfoBar, InfoBarSeverity.Success,
                    true, "همکار مورد نظر شما با موفقیت ثبت شد.");
                firstNameBox.Focus(FocusState.Pointer);
            }
        }
    }

    private void BtnClearEmployeeBox_OnClick(object sender, RoutedEventArgs e)
    {
        if (_isEdited)
        {
            _isEdited = false;
        }
        firstNameBox.Text = string.Empty;
        lastNameBox.Text = string.Empty;
        firstNameBoxInfoBar.IsOpen = false;
        lastNameBoxInfoBar.IsOpen = false;
        btnAddEmployee.Content = "ذخیره";
        firstNameBox.Focus(FocusState.Pointer);
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
        var employeeID = (sender as AppBarButton).DataContext;
        using var db = new WaterAssessmentContext();
        var employee = db.Employees.Find(employeeID);

        if (employee != null)
        {
            db.Employees.Remove(employee);
            db.SaveChanges();
            GetEmployeesFromDB();
            InitInfoBar.ImplementInfoBar(firstNameBoxInfoBar, InfoBarSeverity.Success,
                true, "همکار مورد نظر شما با موفقیت حذف شد.");
            firstNameBox.Focus(FocusState.Pointer);
        }
    }

    private void BtnHoverEditEmployee_OnClick(object sender, RoutedEventArgs e)
    {
        var employeeID = Convert.ToInt32((sender as AppBarButton).DataContext);

        if (((employeeListView.Items).Where(emp => (emp is Employee employee) && employee.EmployeeID == employeeID))
            .FirstOrDefault() is Employee selectedEmployee)
        {
            _isEdited = true;
            firstNameBox.Text = selectedEmployee.FirstName;
            lastNameBox.Text = selectedEmployee.LastName;
            btnAddEmployee.Content = "ویرایش";
            btnAddEmployee.DataContext = selectedEmployee;
        }
    }
}