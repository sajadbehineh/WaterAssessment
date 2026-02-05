namespace WaterAssessment.Services
{
    public interface ILocationTypeService
    {
        Task<IEnumerable<LocationType>> GetAllLocationTypesAsync();
        Task<LocationType?> AddNewLocationTypeAsync(string title);
        Task<LocationType?> UpdateLocationTypeAsync(int locationTypeID, string title);
        Task<bool> DeleteLocationTypeAsync(int locationTypeID);
        string GetLastErrorMessage();
    }
}