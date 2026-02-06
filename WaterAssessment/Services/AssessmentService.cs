using Microsoft.EntityFrameworkCore;

namespace WaterAssessment.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IDbContextFactory<WaterAssessmentContext> _dbFactory;
        private string _lastErrorMessage = string.Empty;

        public AssessmentService(IDbContextFactory<WaterAssessmentContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<AssessmentReferenceData> GetReferenceDataAsync()
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                return new AssessmentReferenceData
                {
                    Locations = await db.Locations.AsNoTracking().ToListAsync(),
                    Propellers = await db.Propellers.AsNoTracking().ToListAsync(),
                    CurrentMeters = await db.CurrentMeters.AsNoTracking().ToListAsync(),
                    Employees = await db.Employees.AsNoTracking().ToListAsync()
                };
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در بارگذاری اطلاعات مرجع: {ex.Message}";
                return new AssessmentReferenceData();
            }
        }

        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                return await db.Locations.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در بارگذاری لیست مکان‌ها: {ex.Message}";
                return Enumerable.Empty<Location>();
            }
        }

        public async Task<IEnumerable<Propeller>> GetAllPropellersAsync()
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                return await db.Propellers.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در بارگذاری لیست پروانه‌ها: {ex.Message}";
                return Enumerable.Empty<Propeller>();
            }
        }

        public async Task<IEnumerable<CurrentMeter>> GetAllCurrentMetersAsync()
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                return await db.CurrentMeters.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در بارگذاری لیست مولینه‌ها: {ex.Message}";
                return Enumerable.Empty<CurrentMeter>();
            }
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                return await db.Employees.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در بارگذاری لیست کارمندان: {ex.Message}";
                return Enumerable.Empty<Employee>();
            }
        }

        public async Task<List<LocationPump>> GetLocationPumpsAsync(int locationId)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                return await db.LocationPumps
                    .Where(p => p.LocationID == locationId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در بارگذاری لیست پمپ‌ها: {ex.Message}";
                return new List<LocationPump>();
            }
        }

        public async Task<Assessment?> GetAssessmentForEditAsync(int assessmentId)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                return await db.Assessments
                    .AsNoTracking()
                    .Include(a => a.FormValues)
                    .Include(a => a.AssessmentEmployees).ThenInclude(ae => ae.Employee)
                    .Include(a => a.GateOpenings)
                    .Include(a => a.PumpStates)
                    .FirstOrDefaultAsync(a => a.AssessmentID == assessmentId);
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در بارگذاری اندازه‌گیری: {ex.Message}";
                return null;
            }
        }

        public async Task<bool> AddAssessmentAsync(
            Assessment model,
            IEnumerable<FormValue> formValues,
            IEnumerable<AssessmentPump> pumpStates,
            IEnumerable<Assessment_Employee> assessmentEmployees)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();

                model.Location = null;
                model.Propeller = null;
                model.CurrentMeter = null;

                foreach (var employee in assessmentEmployees)
                {
                    employee.Employee = null;
                }

                model.FormValues = formValues.ToList();
                model.AssessmentEmployees = assessmentEmployees.ToList();
                model.PumpStates = pumpStates.Select(ps => new AssessmentPump
                {
                    LocationPumpID = ps.LocationPumpID,
                    IsRunning = ps.IsRunning
                }).ToList();

                db.Assessments.Add(model);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در ثبت اطلاعات: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> UpdateAssessmentAsync(
            Assessment model,
            IEnumerable<FormValue> formValues,
            IEnumerable<AssessmentPump> pumpStates,
            IEnumerable<Assessment_Employee> assessmentEmployees,
            IEnumerable<AssessmentGate> gateOpenings)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();

                var existing = await db.Assessments
                    .Include(a => a.FormValues)
                    .Include(a => a.AssessmentEmployees)
                    .Include(a => a.GateOpenings)
                    .Include(a => a.PumpStates)
                    .FirstOrDefaultAsync(a => a.AssessmentID == model.AssessmentID);

                if (existing == null)
                {
                    _lastErrorMessage = "این رکورد در دیتابیس یافت نشد (شاید قبلاً حذف شده است).";
                    return false;
                }

                existing.Date = model.Date;
                existing.Timer = model.Timer;
                existing.Echelon = model.Echelon;
                existing.TotalFlow = model.TotalFlow;
                existing.PropellerID = model.PropellerID;
                existing.CurrentMeterID = model.CurrentMeterID;

                foreach (var formValue in formValues)
                {
                    if (formValue.FormValueID == 0)
                    {
                        formValue.AssessmentID = existing.AssessmentID;
                        existing.FormValues.Add(formValue);
                    }
                    else
                    {
                        var dbRow = existing.FormValues.FirstOrDefault(r => r.FormValueID == formValue.FormValueID);
                        if (dbRow != null)
                        {
                            dbRow.Distance = formValue.Distance;
                            dbRow.TotalDepth = formValue.TotalDepth;
                            dbRow.RowIndex = formValue.RowIndex;
                            dbRow.MeasureTime = formValue.MeasureTime;
                            dbRow.Rev02 = formValue.Rev02;
                            dbRow.Rev06 = formValue.Rev06;
                            dbRow.Rev08 = formValue.Rev08;
                            dbRow.SectionWidth = formValue.SectionWidth;
                            dbRow.SectionArea = formValue.SectionArea;
                            dbRow.SectionFlow = formValue.SectionFlow;
                            dbRow.VerticalMeanVelocity = formValue.VerticalMeanVelocity;
                        }
                    }
                }

                db.AssessmentPumps.RemoveRange(existing.PumpStates);
                foreach (var ps in pumpStates)
                {
                    existing.PumpStates.Add(new AssessmentPump
                    {
                        AssessmentID = existing.AssessmentID,
                        LocationPumpID = ps.LocationPumpID,
                        IsRunning = ps.IsRunning
                    });
                }

                db.AssessmentEmployees.RemoveRange(existing.AssessmentEmployees);
                foreach (var ae in assessmentEmployees)
                {
                    existing.AssessmentEmployees.Add(new Assessment_Employee
                    {
                        AssessmentID = existing.AssessmentID,
                        EmployeeID = ae.EmployeeID
                    });
                }

                db.AssessmentGates.RemoveRange(existing.GateOpenings);
                foreach (var gate in gateOpenings)
                {
                    existing.GateOpenings.Add(new AssessmentGate
                    {
                        AssessmentID = existing.AssessmentID,
                        GateNumber = gate.GateNumber,
                        Value = gate.Value
                    });
                }

                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در ویرایش اطلاعات: {ex.Message}";
                return false;
            }
        }

        public string GetLastErrorMessage() => _lastErrorMessage;
    }
}
