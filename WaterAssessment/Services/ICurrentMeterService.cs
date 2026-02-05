namespace WaterAssessment.Services
{
    public interface ICurrentMeterService
    {
        Task<IEnumerable<CurrentMeter>> GetAllCurrentMetersAsync();

        Task<bool> AddNewCurrentMeterAsync(string currentMeterName);

        Task<bool> UpdateCurrentMeterAsync(int currentMeterId, string currentMeterName);

        Task<bool> DeleteCurrentMeterAsync(int currentMeterId);

        string GetLastErrorMessage();
    }
}
