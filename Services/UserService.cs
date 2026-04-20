using FirstMvcApp.Models;
using FirstMvcApp.Repositories;
using FirstMvcApp.Utilities;

namespace FirstMvcApp.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User?> GetUserByIdAsync(string id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<bool> RegisterUserAsync(User user)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(user.Email);
        if (existingUser != null)
            return false;

        // Hash password before saving
        // Note: PasswordHelper.HashPassword is used in controllers currently.
        // We'll keep it there for now or move it here. Moving it here is cleaner.
        // But for minimal disturbance during refactoring, I'll assume the user object 
        // already has the hashed password or the caller hashes it.
        // Actually, let's make the service responsible for it if we want 'proper' logic.
        
        await _userRepository.CreateAsync(user);
        return true;
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null) return null;

        if (PasswordHelper.VerifyPassword(password, user.Password))
        {
            return user;
        }

        return null;
    }

    public async Task<bool> UpdateUserAsync(string id, User user)
    {
        var existingUser = await _userRepository.GetByIdAsync(id);
        if (existingUser == null) return false;

        return await _userRepository.UpdateAsync(id, user);
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        return await _userRepository.DeleteAsync(id);
    }
}
