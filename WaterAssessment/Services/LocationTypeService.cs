using Microsoft.EntityFrameworkCore;

namespace WaterAssessment.Services
{
    public class LocationTypeService : ILocationTypeService
    {
        private string _lastErrorMessage = string.Empty;
        public async Task<IEnumerable<LocationType>> GetAllLocationTypesAsync()
        {
            try
            {
                using var db = new WaterAssessmentContext();
                return await db.LocationTypes
                    .Include(lt => lt.CreatedBy)
                    .Include(lt => lt.UpdatedBy)
                    .AsNoTracking().ToListAsync();
            }
            catch (Exception e)
            {
                _lastErrorMessage = $"خطا در بارگذاری لیست نوع مکان ها: {e.Message}";
                return Enumerable.Empty<LocationType>();
            }
        }

        public async Task<LocationType?> AddNewLocationTypeAsync(string title)
        {
            try
            {
                using var db = new WaterAssessmentContext();
                bool exists = await db.LocationTypes.AnyAsync(lt => lt.Title == title);
                if (exists)
                {
                    _lastErrorMessage = "نوع مکان مورد نظر قبلاً ثبت شده است.";
                    return null;
                }
                var newLocType = new LocationType { Title = title };
                db.LocationTypes.Add(newLocType);
                await db.SaveChangesAsync();
                return newLocType;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در ثبت نوع مکان جدید: {ex.Message}";
                return null;
            }
        }

        public async Task<LocationType?> UpdateLocationTypeAsync(int locationTypeID, string title)
        {
            try
            {
                using var db = new WaterAssessmentContext();
                var locTypeToEdit = await db.LocationTypes.FindAsync(locationTypeID);
                if (locTypeToEdit == null)
                {
                    _lastErrorMessage = "نوع مکان مورد نظر برای ویرایش یافت نشد.";
                    return null;
                }

                bool isDuplicate = await db.LocationTypes.AnyAsync(lt => lt.Title == title && lt.LocationTypeID != locationTypeID);
                if (isDuplicate)
                {
                    _lastErrorMessage = "نوع مکان تکراری است.";
                    return null;
                }

                locTypeToEdit.Title = title;
                await db.SaveChangesAsync();
                return locTypeToEdit;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در ویرایش اطلاعات: {ex.Message}";
                return null;
            }
        }

        public async Task<bool> DeleteLocationTypeAsync(int locationTypeID)
        {
            try
            {
                using var db = new WaterAssessmentContext();
                var locTypeToDelete = await db.LocationTypes.FindAsync(locationTypeID);
                if (locTypeToDelete == null)
                {
                    _lastErrorMessage = "نوع مکان مورد نظر برای حذف یافت نشد.";
                    return false;
                }

                // بررسی وابستگی‌ها
                bool hasDependents = await db.Locations.AnyAsync(l => l.LocationTypeID == locationTypeID);
                if (hasDependents)
                {
                    _lastErrorMessage = "این نوع مکان دارای مکان‌های ثبت شده است. برای حذف، ابتدا باید مکان‌های مربوطه را حذف کنید.";
                    return false;
                }

                db.LocationTypes.Remove(locTypeToDelete);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در حذف نوع مکان: {ex.Message}";
                return false;
            }
        }

        public string GetLastErrorMessage() => _lastErrorMessage;
    }
}
