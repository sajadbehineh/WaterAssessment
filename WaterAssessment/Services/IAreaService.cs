namespace WaterAssessment.Services
{
    public interface IAreaService
    {
        Task<IEnumerable<Area>> GetAllAreasAsync();

        Task<Area?> AddNewAreaAsync(string areaName);

        Task<Area?> UpdateAreaAsync(int areaID, string areaName);

        Task<bool> DeleteAreaAsync(int areaID);

        string GetLastErrorMessage();
    }
}
