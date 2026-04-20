using FirstMvcApp.Models;
using FirstMvcApp.Services;
using MongoDB.Driver;

namespace FirstMvcApp.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _usersCollection;

    public UserRepository(MongoDbService mongoDbService)
    {
        // Use the existing MongoDbService to get the database/collection logic if preferred,
        // or just inject IMongoDatabase directly if we refactor Program.cs
        // For now, mirroring the MongoDbService logic
        _usersCollection = mongoDbService.GetCollection<User>("users");
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _usersCollection.Find(_ => true).ToListAsync();
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(User user)
    {
        await _usersCollection.InsertOneAsync(user);
    }

    public async Task<bool> UpdateAsync(string id, User user)
    {
        var result = await _usersCollection.ReplaceOneAsync(u => u.Id == id, user);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _usersCollection.DeleteOneAsync(u => u.Id == id);
        return result.DeletedCount > 0;
    }
}
