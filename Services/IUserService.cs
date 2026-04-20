using FirstMvcApp.Models;

namespace FirstMvcApp.Services;

public interface IUserService
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(string id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> RegisterUserAsync(User user);
    Task<User?> AuthenticateAsync(string email, string password);
    Task<bool> UpdateUserAsync(string id, User user);
    Task<bool> DeleteUserAsync(string id);
}
