using MongoDB.Bson;
using MongoDB.Driver;
using FirstMvcApp.Models;

namespace FirstMvcApp.Services;

public class MongoDbService
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<User> _usersCollection;

    public MongoDbService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDb") ?? throw new InvalidOperationException("MongoDB connection string not found");
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase("FirstMvcAppDb");
        _usersCollection = _database.GetCollection<User>("users");
    }

    // Get all users
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _usersCollection.Find(_ => true).ToListAsync();
    }

    // Get user by email
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    // Get user by ID
    public async Task<User?> GetUserByIdAsync(string id)
    {
        return await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    // Create new user
    public async Task<bool> CreateUserAsync(User user)
    {
        try
        {
            // Check if user already exists
            var existingUser = await GetUserByEmailAsync(user.Email);
            if (existingUser != null)
                return false;

            await _usersCollection.InsertOneAsync(user);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating user: {ex.Message}");
            return false;
        }
    }

    // Update user
    public async Task<bool> UpdateUserAsync(string id, User user)
    {
        try
        {
            var result = await _usersCollection.ReplaceOneAsync(
                u => u.Id == id,
                user,
                new ReplaceOptions { IsUpsert = false }
            );
            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating user: {ex.Message}");
            return false;
        }
    }

    // Delete user
    public async Task<bool> DeleteUserAsync(string id)
    {
        try
        {
            var result = await _usersCollection.DeleteOneAsync(u => u.Id == id);
            return result.DeletedCount > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting user: {ex.Message}");
            return false;
        }
    }
}
