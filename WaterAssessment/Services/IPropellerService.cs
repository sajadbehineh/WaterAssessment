namespace WaterAssessment.Services
{
    public interface IPropellerService
    {
        Task<IEnumerable<Propeller>> GetAllPropellersAsync();
        Task<bool> AddNewPropellerAsync(Propeller propeller);
        Task<bool> UpdatePropellerAsync(Propeller propeller);
        Task<bool> DeletePropellerAsync(int propellerId);
        string GetLastErrorMessage();
    }
}
