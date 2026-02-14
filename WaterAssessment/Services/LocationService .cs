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
        private readonly IDbContextFactory<WaterAssessmentContext> _dbFactory;
        private string _lastErrorMessage = string.Empty;

        public LocationService(IDbContextFactory<WaterAssessmentContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                return await db.Locations
                    .Include(l => l.LocationPumps)
                    .Include(l => l.HydraulicGates)
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
                using var db = _dbFactory.CreateDbContext();
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
                using var db = _dbFactory.CreateDbContext();
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
                using var db = _dbFactory.CreateDbContext();
                bool exists = await db.Locations.AnyAsync(l => l.LocationName == location.LocationName && l.AreaID == location.AreaID);
                if (exists)
                {
                    _lastErrorMessage = "مکان مورد نظر با همین نام قبلاً در این حوزه ثبت شده است.";
                    return false;
                }
                location.LocationPumps ??= new List<LocationPump>();
                location.HydraulicGates ??= new List<HydraulicGate>();

                if (!ValidateAndNormalizeLocation(location))
                {
                    return false;
                }

                db.Locations.Add(location);

                foreach (var pump in location.LocationPumps)
                {
                    db.LocationPumps.Add(pump);
                }

                foreach (var gate in location.HydraulicGates)
                {
                    db.HydraulicGates.Add(gate);
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
                using var db = _dbFactory.CreateDbContext();
                var locationToEdit = await db.Locations
                    .Include(l => l.LocationPumps)
                    .Include(l => l.HydraulicGates)
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

                location.LocationPumps ??= new List<LocationPump>();
                location.HydraulicGates ??= new List<HydraulicGate>();

                if (!ValidateAndNormalizeLocation(location))
                {
                    return false;
                }

                locationToEdit.LocationName = location.LocationName;
                locationToEdit.AreaID = location.AreaID;
                locationToEdit.LocationTypeID = location.LocationTypeID;
                locationToEdit.HasGate = location.HasGate;
                locationToEdit.HasPump = location.HasPump;
                locationToEdit.GateCount = location.GateCount;
                locationToEdit.PumpCount = location.PumpCount;
                locationToEdit.MeasurementFormType = location.MeasurementFormType;
                locationToEdit.SectionCount = location.SectionCount;

                if (location.PumpCount.HasValue && location.PumpCount > 0)
                {
                    var incomingPumps = location.LocationPumps ?? new List<LocationPump>();

                    var pumpsToDelete = locationToEdit.LocationPumps
                        .Where(pDB => incomingPumps.All(pVm => pVm.Id != pDB.Id))
                        .ToList();
                    if (pumpsToDelete.Any())
                    {
                        db.LocationPumps.RemoveRange(pumpsToDelete);

                        // ردیف‌های mark شده برای حذف را از کالکشن لوکال هم خارج می‌کنیم
                        // تا در مرحله merge دوباره به اشتباه match نشوند.
                        foreach (var pumpToDelete in pumpsToDelete)
                        {
                            locationToEdit.LocationPumps.Remove(pumpToDelete);
                        }
                    }

                    // ویرایش پمپ‌های موجود و افزودن پمپ‌های جدید
                    foreach (var pumpFromViewModel in incomingPumps)
                    {
                        LocationPump? pumpInDb = null;

                        if (pumpFromViewModel.Id > 0)
                        {
                            pumpInDb = locationToEdit.LocationPumps
                                .FirstOrDefault(p => p.Id == pumpFromViewModel.Id);
                        }

                        // برای پمپ‌های جدید (Id=0) بر اساس نام پمپ fallback می‌کنیم
                        pumpInDb ??= locationToEdit.LocationPumps
                            .FirstOrDefault(p => p.PumpName == pumpFromViewModel.PumpName);

                        if (pumpInDb != null)
                        {
                            // سناریوی ۱: پمپ موجود است -> فقط ویرایش کن
                            pumpInDb.PumpName = pumpFromViewModel.PumpName;
                            pumpInDb.NominalFlow = pumpFromViewModel.NominalFlow;
                        }
                        else
                        {
                            // سناریوی ۲: پمپ موجود نیست (Id=0) -> آن را اضافه کن
                            locationToEdit.LocationPumps.Add(new LocationPump
                            {
                                PumpName = pumpFromViewModel.PumpName,
                                NominalFlow = pumpFromViewModel.NominalFlow
                            });
                        }
                    }
                }
                else
                {
                    // اگر تیک "دارای پمپ" برداشته شد، همه پمپ‌های مرتبط را حذف کن
                    locationToEdit.LocationPumps.Clear();
                }

                var incomingGates = location.HydraulicGates ?? new List<HydraulicGate>();
                var gatesToDelete = locationToEdit.HydraulicGates
                    .Where(g => incomingGates.All(i => i.Id != g.Id))
                    .ToList();  // g is gate in db and i is incoming (new gate or deleted gate)

                if (gatesToDelete.Any())
                {
                    db.HydraulicGates.RemoveRange(gatesToDelete);

                    /// ردیف‌های mark شده برای حذف را از کالکشن لوکال هم خارج می‌کنیم
                    // تا در مرحله merge دوباره به اشتباه match نشوند.
                    foreach (var gateToDelete in gatesToDelete)
                    {
                        locationToEdit.HydraulicGates.Remove(gateToDelete);
                    }
                }

                foreach (var gate in incomingGates)
                {
                    // برای ردیف‌های جدید Id هنوز صفر است؛
                    // اگر صرفاً با Id جستجو کنیم، اولین ردیف جدیدِ اضافه‌شده دوباره پیدا می‌شود
                    // و ردیف‌های 5..n روی هم overwrite می‌شوند.
                    HydraulicGate? gateInDb = null;

                    if (gate.Id > 0)
                    {
                        gateInDb = locationToEdit.HydraulicGates.FirstOrDefault(g => g.Id == gate.Id);
                    }

                    gateInDb ??= locationToEdit.HydraulicGates.FirstOrDefault(g => g.GateNumber == gate.GateNumber);

                    if (gateInDb == null)
                    {
                        locationToEdit.HydraulicGates.Add(new HydraulicGate
                        {
                            GateNumber = gate.GateNumber,
                            DischargeCoefficient = gate.DischargeCoefficient,
                            Width = gate.Width
                        });
                    }
                    else
                    {
                        gateInDb.GateNumber = gate.GateNumber;
                        gateInDb.DischargeCoefficient = gate.DischargeCoefficient;
                        gateInDb.Width = gate.Width;
                    }
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

        // این متد مسئول اعتبارسنجی و نرمال‌سازی داده‌های مربوط به دریچه‌ها است
        private bool ValidateAndNormalizeLocation(Location location)
        {
            location.LocationPumps ??= new List<LocationPump>();
            location.HydraulicGates ??= new List<HydraulicGate>();

            // نرمال‌سازی پمپ‌ها: اگر تعداد معتبر نداریم، حالت «دارای پمپ» نباید ذخیره شود.
            if (!location.PumpCount.HasValue || location.PumpCount.Value <= 0)
            {
                location.HasPump = false;
                location.PumpCount = null;
                location.LocationPumps.Clear();
            }
            else
            {
                location.HasPump = true;
            }

            if (location.MeasurementFormType != MeasurementFormType.GateDischargeEquation)
            {
                 // برای فرم‌های غیر معادله دبی: اگر تعداد دریچه معتبر نداریم، «دارای دریچه» را false ذخیره کن.

                if (!location.GateCount.HasValue || location.GateCount.Value <= 0)
                {
                    location.HasGate = false;
                    location.GateCount = null;
                }
                else
                {
                    location.HasGate = true;
                }

                // فقط مکان‌های معادله دبی باید تنظیمات HydraulicGate داشته باشند.

                location.HydraulicGates.Clear();
                return true;
            }

            if (!location.GateCount.HasValue || location.GateCount.Value <= 0)
            {
                _lastErrorMessage = "برای فرم محسابات دبی دریچه ها باید تعداد دریچه بیشتر از صفر باشد.";
                return false;
            }

            var gateCount = location.GateCount.Value;

            if (location.HydraulicGates.Count != gateCount)
            {
                _lastErrorMessage = "تعداد ردیف‌های OrificeGate باید برابر تعداد دریچه‌ها باشد.";
                return false;
            }

            var gateNumbers = location.HydraulicGates.Select(g => g.GateNumber).ToList();
            if (gateNumbers.Distinct().Count() != gateNumbers.Count)
            {
                _lastErrorMessage = "شماره دریچه‌های OrificeGate نباید تکراری باشند.";
                return false;
            }

            var expected = Enumerable.Range(1, gateCount).ToHashSet();
            if (!gateNumbers.All(expected.Contains))
            {
                _lastErrorMessage = "شماره دریچه‌های OrificeGate باید در بازه 1 تا تعداد دریچه باشند.";
                return false;
            }

            if (location.HydraulicGates.Any(g => g.DischargeCoefficient <= 0 || g.Width <= 0))
            {
                _lastErrorMessage = "مقادیر Cd و عرض دریچه باید بزرگتر از صفر باشند.";
                return false;
            }

            location.HasGate = true;
            return true;
        }

        public async Task<bool> DeleteLocationAsync(int locationId)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
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
