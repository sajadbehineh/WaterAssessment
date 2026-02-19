using Microsoft.EntityFrameworkCore;

namespace WaterAssessment.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IDbContextFactory<WaterAssessmentContext> _dbFactory;
        private string _lastErrorMessage = string.Empty;

        public EmployeeService(IDbContextFactory<WaterAssessmentContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                return await db.Employees
                    .Include(e => e.CreatedBy)
                    .Include(e => e.UpdatedBy)
                    .OrderByDescending(e => e.UpdatedAt)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در بارگذاری لیست کارمندان: {ex.Message}";
                return Enumerable.Empty<Employee>(); // در صورت خطا، یک لیست خالی برگردان
            }
        }

        public async Task<bool> AddNewEmployeeAsync(string firstName, string lastName)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                bool exists = await db.Employees.AnyAsync(e => e.FirstName == firstName && e.LastName == lastName);
                if (exists)
                {
                    _lastErrorMessage = "همکار با این نام و نام خانوادگی قبلاً ثبت شده است.";
                    return false;
                }

                var newEmp = new Employee { FirstName = firstName, LastName = lastName };
                db.Employees.Add(newEmp);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در ثبت کارمند جدید: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> UpdateEmployeeAsync(int employeeId, string firstName, string lastName)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                var empToEdit = await db.Employees.FindAsync(employeeId);
                if (empToEdit == null)
                {
                    _lastErrorMessage = "کارمند مورد نظر برای ویرایش یافت نشد.";
                    return false;
                }

                // بررسی عدم تکرار با دیگران
                bool isDuplicate = await db.Employees.AnyAsync(e => e.FirstName == firstName && e.LastName == lastName && e.EmployeeID != employeeId);
                if (isDuplicate)
                {
                    _lastErrorMessage = "نام و نام خانوادگی تکراری است.";
                    return false;
                }

                empToEdit.FirstName = firstName;
                empToEdit.LastName = lastName;
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در ویرایش اطلاعات: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> DeleteEmployeeAsync(int employeeId)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                var empToDelete = await db.Employees.FindAsync(employeeId);
                if (empToDelete == null)
                {
                    _lastErrorMessage = "کارمند مورد نظر برای حذف یافت نشد.";
                    return false; // یا true، بسته به منطق برنامه
                }

                // بررسی وابستگی‌ها
                bool hasDependents = await db.AssessmentEmployees.AnyAsync(ae => ae.EmployeeID == employeeId);
                if (hasDependents)
                {
                    _lastErrorMessage = "این کارمند دارای اندازه‌گیری‌های ثبت شده است و قابل حذف نیست.";
                    return false;
                }

                db.Employees.Remove(empToDelete);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در حذف کارمند: {ex.Message}";
                return false;
            }
        }

        public string GetLastErrorMessage() => _lastErrorMessage;
    }
}
