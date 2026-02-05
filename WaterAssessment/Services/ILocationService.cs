namespace WaterAssessment.Services
{
    public interface ILocationService
    {
        Task<IEnumerable<Location>> GetAllLocationsAsync();
        Task<IEnumerable<Area>> GetAllAreasAsync();
        Task<IEnumerable<LocationType>> GetAllLocationTypesAsync();
        Task<bool> AddNewLocationAsync(Location location);
        Task<bool> UpdateLocationAsync(int locationId, Location location);
        Task<bool> DeleteLocationAsync(int locationId);
        string GetLastErrorMessage();
    }
}
