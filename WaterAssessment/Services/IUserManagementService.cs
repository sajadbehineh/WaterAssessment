using User = WaterAssessment.Models.User;

namespace WaterAssessment.Services
{
    public interface IUserManagementService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();

        Task<User?> AddUserAsync(string username, string fullName, string role, string password);

        Task<User?> UpdateUserAsync(int userId, string username, string fullName, string role, string? password);

        Task<bool> DeleteUserAsync(int userId, int? currentUserId = null);

        string GetLastErrorMessage();
    }
}
