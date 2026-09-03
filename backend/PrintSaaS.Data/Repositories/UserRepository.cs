using Microsoft.EntityFrameworkCore;
using PrintSaaS.Models;

namespace PrintSaaS.Data.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(int id);
    Task<List<User>> GetAllAsync(bool includeInactive = false);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
}

public class UserRepository : IUserRepository
{
    private readonly PrintContext _context;

    public UserRepository(PrintContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<List<User>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.Users.AsQueryable();

        if (!includeInactive)
            query = query.Where(u => u.IsActive);

        return await query.OrderBy(u => u.Username).ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}
