namespace WaterAssessment.Services
{
    public interface IAssessmentReportService
    {
        Task<IReadOnlyList<Location>> GetLocationsAsync();
        Task<IReadOnlyList<LocationType>> GetLocationTypesAsync();
        Task<IReadOnlyList<Assessment>> GetAssessmentsAsync(
            int? locationId,
            int? locationTypeId,
            DateTime? startDate,
            DateTime? endDate);
        Task<bool> DeleteAssessmentAsync(int assessmentId);
        string GetLastErrorMessage();
    }
}
