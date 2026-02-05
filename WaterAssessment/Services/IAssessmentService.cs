namespace WaterAssessment.Services
{
    public interface IAssessmentService
    {
        Task<AssessmentReferenceData> GetReferenceDataAsync();
        Task<IEnumerable<Location>> GetAllLocationsAsync();
        Task<IEnumerable<Propeller>> GetAllPropellersAsync();
        Task<IEnumerable<CurrentMeter>> GetAllCurrentMetersAsync();
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<List<LocationPump>> GetLocationPumpsAsync(int locationId);
        Task<Assessment?> GetAssessmentForEditAsync(int assessmentId);
        Task<bool> AddAssessmentAsync(
            Assessment model,
            IEnumerable<FormValue> formValues,
            IEnumerable<AssessmentPump> pumpStates,
            IEnumerable<Assessment_Employee> assessmentEmployees);
        Task<bool> UpdateAssessmentAsync(
            Assessment model,
            IEnumerable<FormValue> formValues,
            IEnumerable<AssessmentPump> pumpStates,
            IEnumerable<Assessment_Employee> assessmentEmployees,
            IEnumerable<AssessmentGate> gateOpenings);
        string GetLastErrorMessage();
    }
}
