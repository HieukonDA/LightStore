
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
        return _context.Users.FindAsync(userId).AsTask();
    }

    public Task<bool> UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        _context.SaveChanges();
        return Task.FromResult(true);
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
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null) return Task.CompletedTask;

        user.RefreshToken = token;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Ví dụ: 7 ngày

        _context.SaveChanges();
        return Task.CompletedTask;
    }

    public Task<bool> DeleteTokenAsync(string token)
    {
        var user = _context.Users.FirstOrDefault(u => u.RefreshToken == token);
        if (user == null) return Task.FromResult(false);

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        _context.SaveChanges();
        return Task.FromResult(true);
    }

    public Task<string> GetTokenByEmailAsync(string email)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        return Task.FromResult(user?.RefreshToken);
    }

    public Task<User?> GetUserByResetTokenAsync(string token)
    {
        return Task.FromResult(_context.Users.FirstOrDefault(u => u.PasswordResetOtp == token));
    }

    // otp + reset password
    public async Task<bool> SaveOtpAsync(string email, string otp, DateTime expiryTime)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return false;

        user.PasswordResetOtp = otp;
        user.OtpExpiryTime = expiryTime;
        user.OtpAttempts = 0; // Reset attempts

        _context.SaveChanges();
        return true;
    }

    public async Task<bool> ValidateAndResetPasswordAsync(string email, string otp, string newPasswordHash)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return false;

        // Kiểm tra OTP tồn tại
        if (string.IsNullOrEmpty(user.PasswordResetOtp))
            return false;

        // Kiểm tra OTP hết hạn
        if (DateTime.UtcNow > user.OtpExpiryTime)
        {
            // Clear OTP hết hạn
            user.PasswordResetOtp = null;
            user.OtpExpiryTime = null;
            user.OtpAttempts = 0;
            _context.SaveChanges();
            return false;
        }

        // Kiểm tra quá nhiều lần thử
        if (user.OtpAttempts >= 5)
        {
            // Clear OTP khi quá nhiều lần thử
            user.PasswordResetOtp = null;
            user.OtpExpiryTime = null;
            user.OtpAttempts = 0;
            _context.SaveChanges();
            return false;
        }

        // Kiểm tra OTP sai
        if (user.PasswordResetOtp != otp)
        {
            // Tăng số lần thử
            user.OtpAttempts++;
            _context.SaveChanges();
            return false;
        }

        // ✅ OTP đúng -> Reset password
        user.PasswordHash = newPasswordHash;
        
        // Clear OTP sau khi dùng xong
        user.PasswordResetOtp = null;
        user.OtpExpiryTime = null;
        user.OtpAttempts = 0;

        _context.SaveChanges();
        return true;
    }

}