namespace WaterAssessment.Services
{
    public interface IEmployeeService
    {
        /// <summary>
        /// تمام کارمندان را به صورت آسنکرون دریافت می‌کند.
        /// </summary>
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();

        /// <summary>
        /// یک کارمند جدید اضافه می‌کند. در صورت موفقیت true برمی‌گرداند.
        /// </summary>
        Task<bool> AddNewEmployeeAsync(string firstName, string lastName);

        /// <summary>
        /// اطلاعات یک کارمند موجود را به‌روزرسانی می‌کند.
        /// </summary>
        Task<bool> UpdateEmployeeAsync(int employeeId, string firstName, string lastName);

        /// <summary>
        /// یک کارمند را با شناسه مشخص حذف می‌کند.
        /// </summary>
        Task<bool> DeleteEmployeeAsync(int employeeId);

        /// <summary>
        /// آخرین پیام خطا را برمی‌گرداند.
        /// </summary>
        string GetLastErrorMessage();
    }
}
