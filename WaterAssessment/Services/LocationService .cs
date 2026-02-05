using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterAssessment.Services
{
    public class LocationService : ILocationService
    {
        private string _lastErrorMessage = string.Empty;

        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();
                return await db.Locations
                    .Include(l => l.LocationPumps)
                    .Include(l => l.Area)
                    .Include(l => l.LocationType)
                    .Include(l => l.CreatedBy)
                    .Include(l => l.UpdatedBy)
                    .AsNoTracking().ToListAsync();
            }
            catch (Exception e)
            {
                _lastErrorMessage = $"خطا در بارگذاری لیست مکان‌ها: {e.Message}";
                return Enumerable.Empty<Location>();
            }
        }

        public async Task<IEnumerable<Area>> GetAllAreasAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();
                return await db.Areas.AsNoTracking().ToListAsync();
            }
            catch (Exception e)
            {
                _lastErrorMessage = $"خطا در بارگذاری لیست حوزه‌ها: {e.Message}";
                return Enumerable.Empty<Area>();
            }
        }

        public async Task<IEnumerable<LocationType>> GetAllLocationTypesAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();
                return await db.LocationTypes.AsNoTracking().ToListAsync();
            }
            catch (Exception e)
            {
                _lastErrorMessage = $"خطا در بارگذاری لیست انواع مکان: {e.Message}";
                return Enumerable.Empty<LocationType>();
            }
        }

        public async Task<bool> AddNewLocationAsync(Location location)
        {
            try
            {
                using var db = new WaterAssessmentContext();
                bool exists = await db.Locations.AnyAsync(l => l.LocationName == location.LocationName && l.AreaID == location.AreaID);
                if (exists)
                {
                    _lastErrorMessage = "مکان مورد نظر با همین نام قبلاً در این حوزه ثبت شده است.";
                    return false;
                }
                location.LocationPumps ??= new List<LocationPump>();
                db.Locations.Add(location);
                foreach (var pump in location.LocationPumps)
                {
                    db.LocationPumps.Add(pump);
                }
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در ثبت مکان جدید: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> UpdateLocationAsync(int locationId, Location location)
        {
            try
            {
                using var db = new WaterAssessmentContext();
                var locationToEdit = await db.Locations
                    .Include(l => l.LocationPumps) // <-- بسیار مهم
                    .FirstOrDefaultAsync(l => l.LocationID == locationId);
                if (locationToEdit == null)
                {
                    _lastErrorMessage = "مکان مورد نظر برای ویرایش یافت نشد.";
                    return false;
                }

                bool isDuplicate = await db.Locations.AnyAsync(l =>
                    l.LocationName == location.LocationName &&
                    l.AreaID == location.AreaID &&
                    l.LocationID != locationId);

                if (isDuplicate)
                {
                    _lastErrorMessage = "نام مکان در این حوزه تکراری است.";
                    return false;
                }

                locationToEdit.LocationName = location.LocationName;
                locationToEdit.AreaID = location.AreaID;
                locationToEdit.LocationTypeID = location.LocationTypeID;
                locationToEdit.GateCount = location.GateCount;
                locationToEdit.PumpCount = location.PumpCount;

                if (location.PumpCount.HasValue && location.PumpCount > 0)
                {
                    // حذف پمپ‌هایی که دیگر در لیست جدید نیستند (اگر چنین سناریویی دارید)
                    var pumpsToDelete = locationToEdit.LocationPumps
                        .Where(p_db => !location.LocationPumps.Any(p_vm => p_vm.Id == p_db.Id))
                        .ToList();
                    if (pumpsToDelete.Any())
                    {
                        db.LocationPumps.RemoveRange(pumpsToDelete);
                    }

                    // ویرایش پمپ‌های موجود و افزودن پمپ‌های جدید
                    foreach (var pumpFromViewModel in location.LocationPumps)
                    {
                        var pumpInDb = locationToEdit.LocationPumps
                            .FirstOrDefault(p => p.Id == pumpFromViewModel.Id);

                        if (pumpInDb != null)
                        {
                            // سناریوی ۱: پمپ موجود است -> فقط ویرایش کن
                            pumpInDb.NominalFlow = pumpFromViewModel.NominalFlow;
                        }
                        else
                        {
                            // سناریوی ۲: پمپ موجود نیست (Id=0) -> آن را اضافه کن
                            // نکته: شیء جدید را به کالکشن موجودیت ردیابی‌شده اضافه می‌کنیم
                            locationToEdit.LocationPumps.Add(pumpFromViewModel); // <-- کد جدید و حیاتی
                        }
                    }
                }
                else
                {
                    // اگر تیک "دارای پمپ" برداشته شد، همه پمپ‌های مرتبط را حذف کن
                    locationToEdit.LocationPumps.Clear();
                }

                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در ویرایش اطلاعات مکان: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> DeleteLocationAsync(int locationId)
        {
            try
            {
                using var db = new WaterAssessmentContext();
                var locationToDelete = await db.Locations.FindAsync(locationId);
                if (locationToDelete == null)
                {
                    _lastErrorMessage = "مکان مورد نظر برای حذف یافت نشد.";
                    return false;
                }

                // بررسی وابستگی‌ها
                bool hasDependents = await db.Assessments.AnyAsync(a => a.LocationID == locationId);
                if (hasDependents)
                {
                    _lastErrorMessage = "این مکان دارای اندازه گیری‌های ثبت شده است. برای حذف، ابتدا باید اندازه گیری‌های مربوطه را حذف کنید.";
                    return false;
                }

                db.Locations.Remove(locationToDelete);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در حذف مکان: {ex.Message}";
                return false;
            }
        }

        public string GetLastErrorMessage() => _lastErrorMessage;
    }
}
