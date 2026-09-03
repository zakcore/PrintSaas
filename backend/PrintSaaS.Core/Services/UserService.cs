using PrintSaaS.Data.Repositories;
using PrintSaaS.Models;
using PrintSaaS.Models.Enums;

namespace PrintSaaS.Core.Services;

public interface IUserService
{
    Task<List<User>> GetAllAsync(bool includeInactive = false);
    Task<User?> GetByIdAsync(int id);
    Task<User> CreateAsync(string username, string password, string displayName, UserRole role);
    Task<bool> UpdateAsync(int id, string? displayName, UserRole? role);
    Task<bool> DeactivateAsync(int id);
}

public class UserService : IUserService
{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<User>> GetAllAsync(bool includeInactive = false)
    {
        return await _repo.GetAllAsync(includeInactive);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _repo.GetByIdAsync(id);
    }

    public async Task<User> CreateAsync(string username, string password, string displayName, UserRole role)
    {
        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            DisplayName = displayName,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return await _repo.CreateAsync(user);
    }

    public async Task<bool> UpdateAsync(int id, string? displayName, UserRole? role)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user is null) return false;

        if (displayName is not null) user.DisplayName = displayName;
        if (role.HasValue) user.Role = role.Value;

        await _repo.UpdateAsync(user);
        return true;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user is null) return false;

        user.IsActive = false;
        await _repo.UpdateAsync(user);
        return true;
    }
}
