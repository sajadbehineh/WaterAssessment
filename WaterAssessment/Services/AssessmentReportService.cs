using Microsoft.EntityFrameworkCore;

namespace WaterAssessment.Services
{
    public class AssessmentReportService: IAssessmentReportService
    {
        private readonly IDbContextFactory<WaterAssessmentContext> _dbFactory;
        private string _lastErrorMessage = string.Empty;

        public AssessmentReportService(IDbContextFactory<WaterAssessmentContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IReadOnlyList<Location>> GetLocationsAsync()
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                return await db.Locations.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در بارگذاری لیست مکان‌ها: {ex.Message}";
                return Array.Empty<Location>();
            }
        }

        public async Task<IReadOnlyList<LocationType>> GetLocationTypesAsync()
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                return await db.LocationTypes.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در بارگذاری لیست نوع مکان‌ها: {ex.Message}";
                return Array.Empty<LocationType>();
            }
        }

        public async Task<IReadOnlyList<Assessment>> GetAssessmentsAsync(
            int? locationId,
            int? locationTypeId,
            DateTime? startDate,
            DateTime? endDate)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                var query = db.Assessments
                    .Include(a => a.Propeller)
                    .Include(a => a.AssessmentEmployees)
                    .Include(a => a.Location)
                    .ThenInclude(l => l.LocationType)
                    .Include(a => a.GateOpenings)
                    .Include(a => a.FormValues)
                    .AsNoTracking()
                    .AsQueryable();

                if (locationId.HasValue)
                {
                    query = query.Where(a => a.LocationID == locationId.Value);
                }

                if (locationTypeId.HasValue)
                {
                    query = query.Where(a => a.Location.LocationTypeID == locationTypeId.Value);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(a => a.Date >= startDate.Value.Date);
                }

                if (endDate.HasValue)
                {
                    var nextDay = endDate.Value.Date.AddDays(1);
                    query = query.Where(a => a.Date < nextDay);
                }

                return await query.OrderByDescending(a => a.Date).ToListAsync();
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در بارگذاری اندازه گیری‌ها: {ex.Message}";
                return Array.Empty<Assessment>();
            }
        }

        public async Task<bool> DeleteAssessmentAsync(int assessmentId)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                var assessment = await db.Assessments.FindAsync(assessmentId);
                if (assessment == null)
                {
                    _lastErrorMessage = "اندازه گیری مورد نظر برای حذف یافت نشد.";
                    return false;
                }

                db.Assessments.Remove(assessment);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در حذف اندازه گیری: {ex.Message}";
                return false;
            }
        }

        public string GetLastErrorMessage() => _lastErrorMessage;
    }
}
