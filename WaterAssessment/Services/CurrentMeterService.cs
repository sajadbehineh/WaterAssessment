using Microsoft.EntityFrameworkCore;
using WaterAssessment.Models;
using Windows.Networking;

namespace WaterAssessment.Services
{
    public class CurrentMeterService : ICurrentMeterService
    {
        private readonly IDbContextFactory<WaterAssessmentContext> _dbFactory;
        private string _lastErrorMessage = string.Empty;

        public CurrentMeterService(IDbContextFactory<WaterAssessmentContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<CurrentMeter>> GetAllCurrentMetersAsync()
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                return await db.CurrentMeters
                    .Include(c => c.CreatedBy)
                    .Include(c => c.UpdatedBy)
                    .OrderByDescending(c => c.CurrentMeterID)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در بارگذاری لیست مولینه ها: {ex.Message}";
                return Enumerable.Empty<CurrentMeter>();
            }
        }

        public async Task<bool> AddNewCurrentMeterAsync(string currentMeterName)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                bool exists = await db.CurrentMeters.AnyAsync(e => e.CurrentMeterName == currentMeterName);
                if (exists)
                {
                    _lastErrorMessage = "مولینه مورد نظر قبلاً ثبت شده است.";
                    return false;
                }

                var newCurrentMeter = new CurrentMeter { CurrentMeterName = currentMeterName };
                db.CurrentMeters.Add(newCurrentMeter);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در ثبت مولینه جدید: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> UpdateCurrentMeterAsync(int currentMeterId, string currentMeterName)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                var currentMeterToEdit = await db.CurrentMeters.FindAsync(currentMeterId);
                if (currentMeterToEdit == null)
                {
                    _lastErrorMessage = "مولینه مورد نظر برای ویرایش یافت نشد.";
                    return false;
                }

                bool isDuplicate = await db.CurrentMeters.AnyAsync(e => e.CurrentMeterName == currentMeterName && e.CurrentMeterID != currentMeterId);
                if (isDuplicate)
                {
                    _lastErrorMessage = "نام مولینه تکراری است.";
                    return false;
                }

                currentMeterToEdit.CurrentMeterName = currentMeterName;
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در ویرایش اطلاعات: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> DeleteCurrentMeterAsync(int currentMeterId)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                var currentMeterToDelete = await db.CurrentMeters.FindAsync(currentMeterId);
                if (currentMeterToDelete == null)
                {
                    _lastErrorMessage = "مولینه مورد نظر برای حذف یافت نشد.";
                    return false;
                }

                bool hasDependents = await db.Assessments.AnyAsync(a => a.CurrentMeterID == currentMeterId);
                if (hasDependents)
                {
                    _lastErrorMessage = "این مولینه دارای اندازه‌گیری‌های ثبت شده است و قابل حذف نیست.";
                    return false;
                }

                db.CurrentMeters.Remove(currentMeterToDelete);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در حذف مولینه: {ex.Message}";
                return false;
            }
        }

        public string GetLastErrorMessage() => _lastErrorMessage;
    }
}
