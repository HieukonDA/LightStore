namespace TheLightStore.Interfaces.Repository;

public interface IUserRepo
{
    //crud
    Task<User> GetUserByEmailAsync(string email);
    Task<User> GetUserByIdAsync(int userId);
    Task<User> AddUserAsync(User user);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(int userId);
    Task<User> FindUserByEmailAndPasswordAsync(string email, string passwordHash);

    //forgot password
    Task AddTokenAsync(string email, string token);
    Task<string> GetTokenByEmailAsync(string email);
    Task<bool> DeleteTokenAsync(string token);
}
