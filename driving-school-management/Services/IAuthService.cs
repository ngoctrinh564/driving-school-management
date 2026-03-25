using driving_school_management.ViewModels;

namespace driving_school_management.Services
{
    public interface IAuthService
    {
        Task<LoginResult?> Login(string username, string password);
        int Register(string username, string password, string email, int roleId);
        UserProfileVM? GetUserProfile(int userId);
        int UpdateUserProfile(EditUserVM model);
        Task<int> ResetPassword(string username, string newPassword);
        Task<bool> IsProfileCompleted(int userId);
    }
}