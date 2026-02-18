using Microsoft.EntityFrameworkCore;
using User = WaterAssessment.Models.User;

namespace WaterAssessment.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IDbContextFactory<WaterAssessmentContext> _dbFactory;
        private string _lastErrorMessage = string.Empty;

        public UserManagementService(IDbContextFactory<WaterAssessmentContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public string GetLastErrorMessage() => _lastErrorMessage;

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                return await db.Users
                    .AsNoTracking()
                    .OrderByDescending(u => u.UserID)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در بارگذاری لیست کاربران: {ex.Message}";
                return Enumerable.Empty<User>();
            }
        }

        public async Task<User?> AddUserAsync(string username, string fullName, string role, string password)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();

                var normalizedUsername = username.Trim().ToLower();
                var duplicateUsername = await db.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername);
                if (duplicateUsername)
                {
                    _lastErrorMessage = "این نام کاربری قبلاً توسط شخص دیگری انتخاب شده است.";
                    return null;
                }

                var normalizedFullName = fullName.Trim().ToLower();
                var duplicateFullName = await db.Users.AnyAsync(u => (u.FullName ?? string.Empty).ToLower() == normalizedFullName);
                if (duplicateFullName)
                {
                    _lastErrorMessage = "کاربری با این نام و نام خانوادگی قبلاً در سیستم ثبت شده است.";
                    return null;
                }

                var newUser = new User
                {
                    Username = username.Trim(),
                    FullName = fullName.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Role = role
                };

                db.Users.Add(newUser);
                await db.SaveChangesAsync();
                return newUser;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در افزودن کاربر: {ex.Message}";
                return null;
            }
        }

        public async Task<User?> UpdateUserAsync(int userId, string username, string fullName, string role, string? password)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();

                var userToUpdate = await db.Users.FirstOrDefaultAsync(u => u.UserID == userId);
                if (userToUpdate == null)
                {
                    _lastErrorMessage = "کاربر مورد نظر برای ویرایش یافت نشد.";
                    return null;
                }

                var normalizedUsername = username.Trim().ToLower();
                var duplicateUsername = await db.Users.AnyAsync(u =>
                    u.Username.ToLower() == normalizedUsername &&
                    u.UserID != userId);
                if (duplicateUsername)
                {
                    _lastErrorMessage = "این نام کاربری قبلاً توسط شخص دیگری انتخاب شده است.";
                    return null;
                }

                var normalizedFullName = fullName.Trim().ToLower();
                var duplicateFullName = await db.Users.AnyAsync(u =>
                    (u.FullName ?? string.Empty).ToLower() == normalizedFullName &&
                    u.UserID != userId);
                if (duplicateFullName)
                {
                    _lastErrorMessage = "کاربری با این نام و نام خانوادگی قبلاً در سیستم ثبت شده است.";
                    return null;
                }

                userToUpdate.Username = username.Trim();
                userToUpdate.FullName = fullName.Trim();
                userToUpdate.Role = role;

                if (!string.IsNullOrWhiteSpace(password))
                {
                    userToUpdate.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                }

                await db.SaveChangesAsync();
                return userToUpdate;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در ویرایش کاربر: {ex.Message}";
                return null;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId, int? currentUserId = null)
        {
            try
            {
                if (currentUserId.HasValue && userId == currentUserId.Value)
                {
                    _lastErrorMessage = "امکان حذف کاربر جاری وجود ندارد.";
                    return false;
                }

                using var db = _dbFactory.CreateDbContext();
                var user = await db.Users.FirstOrDefaultAsync(u => u.UserID == userId);
                if (user == null)
                {
                    _lastErrorMessage = "کاربر مورد نظر برای حذف یافت نشد.";
                    return false;
                }

                db.Users.Remove(user);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = $"خطا در حذف کاربر: {ex.Message}";
                return false;
            }
        }
    }
}
