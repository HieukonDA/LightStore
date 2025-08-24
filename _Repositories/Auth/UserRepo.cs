
namespace TheLightStore.Repositories.Auth;

public class UserRepo : IUserRepo
{
    private readonly DBContext _context;

    public UserRepo(DBContext context)
    {
        _context = context;
    }

    //crud
    public Task<User> AddUserAsync(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
        return Task.FromResult(user);
    }

    public Task<bool> DeleteUserAsync(int userId)
    {
        var user = _context.Users.Find(userId);
        if (user == null) return Task.FromResult(false);

        _context.Users.Remove(user);
        _context.SaveChanges();
        return Task.FromResult(true);
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public Task<User> GetUserByIdAsync(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    // find user

    public async Task<User> FindUserByEmailAndPasswordAsync(string email, string passwordHash)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(passwordHash, user.PasswordHash);
        return isPasswordValid ? user : null;
    }

    // helper token
    public Task AddTokenAsync(string email, string token)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteTokenAsync(string token)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetTokenByEmailAsync(string email)
    {
        throw new NotImplementedException();
    }
    

}