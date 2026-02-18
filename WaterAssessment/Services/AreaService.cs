using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using WaterAssessment.Messages;

namespace WaterAssessment.Services
{
    public class AreaService : IAreaService
    {
        private readonly IDbContextFactory<WaterAssessmentContext> _dbFactory;
        private string _lastErrorMessage = string.Empty;

        public AreaService(IDbContextFactory<WaterAssessmentContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public string GetLastErrorMessage() => _lastErrorMessage;

        public async Task<IEnumerable<Area>> GetAllAreasAsync()
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                return await db.Areas
                    .Include(a => a.CreatedBy)
                    .Include(a => a.UpdatedBy)
                    .OrderByDescending(a => a.AreaID)
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
                using var db = _dbFactory.CreateDbContext();
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
                using var db = _dbFactory.CreateDbContext();
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

            using var db = _dbFactory.CreateDbContext();

            db.Areas.Remove(new Area() { AreaID = areaID });
            await db.SaveChangesAsync();
            return true;
        }
    }
}
