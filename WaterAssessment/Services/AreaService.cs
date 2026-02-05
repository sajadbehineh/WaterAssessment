using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using WaterAssessment.Messages;

namespace WaterAssessment.Services
{
    public class AreaService : IAreaService
    {
        private string _lastErrorMessage = string.Empty;
        public string GetLastErrorMessage() => _lastErrorMessage;

        public async Task<IEnumerable<Area>> GetAllAreasAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();
                return await db.Areas
                    .Include(a => a.CreatedBy)
                    .Include(a => a.UpdatedBy)
                    .AsNoTracking().ToListAsync();
            }
            catch (Exception e)
            {
                _lastErrorMessage = $"خطا در بارگذاری لیست حوزه ها: {e.Message}";
                return Enumerable.Empty<Area>();
            }
        }

        public async Task<Area?> AddNewAreaAsync(string areaName)
        {
            try
            {
                using var db = new WaterAssessmentContext();
                bool exists = await db.Areas.AnyAsync(a => a.AreaName == areaName);
                if (exists)
                {
                    _lastErrorMessage = "حوزه مورد نظر قبلاً ثبت شده است.";
                    return null;
                }
                var newArea = new Area { AreaName = areaName };
                db.Areas.Add(newArea);
                await db.SaveChangesAsync();
                return newArea;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در ثبت حوزه جدید: {ex.Message}";
                return null;
            }
        }

        public async Task<Area?> UpdateAreaAsync(int areaID, string areaName)
        {
            try
            {
                using var db = new WaterAssessmentContext();
                var areaToEdit = await db.Areas.FindAsync(areaID);
                if (areaToEdit == null)
                {
                    _lastErrorMessage = "حوزه مورد نظر برای ویرایش یافت نشد.";
                    return null;
                }

                bool isDuplicate = await db.Areas.AnyAsync(a => a.AreaName == areaName && a.AreaID != areaID);
                if (isDuplicate)
                {
                    _lastErrorMessage = "نام حوزه تکراری است.";
                    return null;
                }

                areaToEdit.AreaName = areaName;
                await db.SaveChangesAsync();
                return areaToEdit;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در ویرایش اطلاعات: {ex.Message}";
                return null;
            }
        }

        public async Task<bool> DeleteAreaAsync(int areaID)
        {
            try
            {
                using var db = new WaterAssessmentContext();
                var areaToDelete = await db.Areas.FindAsync(areaID);
                if (areaToDelete == null)
                {
                    _lastErrorMessage = "حوزه مورد نظر برای حذف یافت نشد.";
                    return false;
                }

                // بررسی وابستگی‌ها
                bool hasDependents = await db.Locations.AnyAsync(l => l.AreaID == areaID);
                if (hasDependents)
                {
                    _lastErrorMessage = "این حوزه دارای مکان‌های ثبت شده است. برای حذف، ابتدا باید مکان‌های مربوطه را حذف کنید.";
                    return false;
                }

                db.Areas.Remove(areaToDelete);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در حذف حوزه: {ex.Message}";
                return false;
            }
        }
    }
}
