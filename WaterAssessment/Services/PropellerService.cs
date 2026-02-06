using Microsoft.EntityFrameworkCore;

namespace WaterAssessment.Services
{
    public class PropellerService : IPropellerService
    {
        private readonly IDbContextFactory<WaterAssessmentContext> _dbFactory;
        private string _lastErrorMessage = string.Empty;

        public PropellerService(IDbContextFactory<WaterAssessmentContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public string GetLastErrorMessage() => _lastErrorMessage;

        public async Task<IEnumerable<Propeller>> GetAllPropellersAsync()
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                return await db.Propellers
                    .Include(e => e.CreatedBy)
                    .Include(e => e.UpdatedBy)
                    .AsNoTracking()
                    .OrderByDescending(p => p.PropellerID) // ترتیب نمایش جدیدترین‌ها
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در بارگذاری لیست پروانه‌ها: {ex.Message}";
                return Enumerable.Empty<Propeller>();
            }
        }

        public async Task<bool> AddNewPropellerAsync(Propeller propeller)
        {
            try
            {
                if (!ValidatePropeller(propeller)) return false;

                using var db = _dbFactory.CreateDbContext();

                // بررسی تکراری بودن نام
                bool exists = await db.Propellers.AnyAsync(p => p.PropellerName == propeller.PropellerName);
                if (exists)
                {
                    _lastErrorMessage = "پروانه‌ای با این نام قبلاً ثبت شده است.";
                    return false;
                }

                db.Propellers.Add(propeller);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در ثبت پروانه جدید: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> UpdatePropellerAsync(Propeller propeller)
        {
            try
            {
                if (!ValidatePropeller(propeller)) return false;

                using var db = _dbFactory.CreateDbContext();

                var propToEdit = await db.Propellers.FindAsync(propeller.PropellerID);
                if (propToEdit == null)
                {
                    _lastErrorMessage = "پروانه مورد نظر یافت نشد.";
                    return false;
                }

                // بررسی تکراری بودن نام (به جز خودش)
                bool isDuplicate = await db.Propellers.AnyAsync(p => p.PropellerName == propeller.PropellerName && p.PropellerID != propeller.PropellerID);
                if (isDuplicate)
                {
                    _lastErrorMessage = "نام پروانه تکراری است.";
                    return false;
                }

                // آپدیت فیلدها
                propToEdit.PropellerName = propeller.PropellerName;
                propToEdit.Mode = propeller.Mode;

                propToEdit.A1 = propeller.A1;
                propToEdit.B1 = propeller.B1;

                propToEdit.TransitionPoint1 = propeller.TransitionPoint1;
                propToEdit.A2 = propeller.A2;
                propToEdit.B2 = propeller.B2;

                propToEdit.TransitionPoint2 = propeller.TransitionPoint2;
                propToEdit.A3 = propeller.A3;
                propToEdit.B3 = propeller.B3;

                // فیلدهای Auditing (CreatedBy/UpdatedBy) توسط Context هندل می‌شوند

                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در ویرایش پروانه: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> DeletePropellerAsync(int propellerId)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();

                var propToDelete = await db.Propellers.FindAsync(propellerId);
                if (propToDelete == null)
                {
                    _lastErrorMessage = "پروانه یافت نشد.";
                    return false;
                }

                // بررسی وابستگی‌ها
                bool hasDependents = await db.Assessments.AnyAsync(a => a.PropellerID == propellerId);
                if (hasDependents)
                {
                    _lastErrorMessage = "این پروانه در اندازه‌گیری‌ها استفاده شده و قابل حذف نیست.";
                    return false;
                }

                db.Propellers.Remove(propToDelete);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در حذف پروانه: {ex.Message}";
                return false;
            }
        }

        private bool ValidatePropeller(Propeller p)
        {
            if (string.IsNullOrWhiteSpace(p.PropellerName))
            {
                _lastErrorMessage = "نام پروانه نمی‌تواند خالی باشد.";
                return false;
            }

            // اعتبارسنجی نقاط شکست (T1 باید کوچکتر از T2 باشد)
            if (p.Mode == CalibrationMode.ThreeEquations &&
                p.TransitionPoint1.HasValue && p.TransitionPoint2.HasValue &&
                p.TransitionPoint1 >= p.TransitionPoint2)
            {
                _lastErrorMessage = "نقطه شکست اول باید کوچکتر از نقطه شکست دوم باشد.";
                return false;
            }

            return true;
        }
    }
}
