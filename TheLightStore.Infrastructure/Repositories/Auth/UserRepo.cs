using Microsoft.EntityFrameworkCore;
using TheLightStore.Application.DTOs.Paging;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Interfaces;
using TheLightStore.Domain.Entities;
using TheLightStore.Domain.Entities.Auth;
using TheLightStore.Domain.Entities.Shared;
using TheLightStore.Infrastructure.Persistence;
using TheLightStore.Application.DTOs.Search;

namespace TheLightStore.Infrastructure.Repositories.Auth;

public class UserRepo : IUserRepo
{
    private readonly DBContext _context;
    private readonly IEncryptionService _encryptionService;

    public UserRepo(DBContext context, IEncryptionService encryptionService)
    {
        _context = context;
        _encryptionService = encryptionService;
    }

    //crud
    /// <summary>
    /// Get paged customers with optional search and sorting
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<PagedResult<User>> GetCustomersAsync(PagedRequest request)
    {
        var query = _context.Users.AsQueryable();

        // Filter search
        if (!string.IsNullOrEmpty(request.Search))
        {
            string searchLower = request.Search.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(searchLower) ||
                u.LastName.ToLower().Contains(searchLower) ||
                u.Email.ToLower().Contains(searchLower) ||
                (u.Phone != null && u.Phone.Contains(request.Search))
            );
        }

        // Sort
        if (!string.IsNullOrEmpty(request.Sort))
        {
            // Ví dụ: "createdAt_desc" hoặc "email_asc"
            var sortParts = request.Sort.Split('_', StringSplitOptions.RemoveEmptyEntries);
            if (sortParts.Length == 2)
            {
                string sortField = sortParts[0].ToLower();
                string sortDir = sortParts[1].ToLower();

                query = (sortField, sortDir) switch
                {
                    ("createdat", "asc") => query.OrderBy(u => u.CreatedAt),
                    ("createdat", "desc") => query.OrderByDescending(u => u.CreatedAt),
                    ("email", "asc") => query.OrderBy(u => u.Email),
                    ("email", "desc") => query.OrderByDescending(u => u.Email),
                    ("firstname", "asc") => query.OrderBy(u => u.FirstName),
                    ("firstname", "desc") => query.OrderByDescending(u => u.FirstName),
                    _ => query.OrderByDescending(u => u.CreatedAt) // default
                };
            }
            else
            {
                query = query.OrderByDescending(u => u.CreatedAt);
            }
        }
        else
        {
            query = query.OrderByDescending(u => u.CreatedAt); // default sort
        }

        // Total count trước khi phân trang
        int totalCount = await query.CountAsync();

        // Paging
        var users = await query
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync();

        // Decrypt sensitive data before returning
        DecryptUsersData(users);

        // Trả về PagedResult
        return new PagedResult<User>
        {
            Items = users,
            Page = request.Page,
            PageSize = request.Size,
            TotalCount = totalCount
        };
    }


    public async Task<User> AddUserAsync(User user)
    {
        try
        {
            Console.WriteLine($"🔐 REPO: AddUserAsync called for email: {user.Email}");
            
            // Validate before encryption
            if (string.IsNullOrEmpty(user.Email))
                throw new ArgumentException("Email is required");
            
            if (string.IsNullOrEmpty(user.FirstName))
                throw new ArgumentException("FirstName is required");
            
            if (string.IsNullOrEmpty(user.LastName))
                throw new ArgumentException("LastName is required");

            Console.WriteLine($"🔐 REPO: Validation passed. Phone: {user.Phone ?? "null"}");

            // Encrypt sensitive phone data
            if (!string.IsNullOrEmpty(user.Phone))
            {
                Console.WriteLine($"🔐 REPO: Original phone: {user.Phone}");
                user.Phone = _encryptionService.Encrypt(user.Phone);
                Console.WriteLine($"🔐 REPO: Encrypted phone length: {user.Phone.Length}");
            }

            Console.WriteLine($"🔐 REPO: Adding user to context");
            _context.Users.Add(user);
            
            Console.WriteLine($"🔐 REPO: Calling SaveChangesAsync");
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"🔐 REPO: User saved successfully with ID: {user.Id}");
            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔐 REPO ERROR: Exception type: {ex.GetType().Name}");
            Console.WriteLine($"🔐 REPO ERROR: Message: {ex.Message}");
            Console.WriteLine($"🔐 REPO ERROR: Inner exception: {ex.InnerException?.Message}");
            Console.WriteLine($"🔐 REPO ERROR: Stack trace: {ex.StackTrace}");
            
            // Log to file as well
            var logPath = Path.Combine("logs", $"auth-{DateTime.Now:yyyyMMdd}.txt");
            Directory.CreateDirectory("logs");
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] REPO ERROR: {ex}\n");
            
            throw new InvalidOperationException($"Failed to add user: {ex.Message}", ex);
        }
    }

    public Task<bool> DeleteUserAsync(int userId)
    {
        var user = _context.Users.Find(userId);
        if (user == null) return Task.FromResult(false);

        _context.Users.Remove(user);
        _context.SaveChanges();
        return Task.FromResult(true);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null)
        {
            DecryptUserData(user);
        }
        return user;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            DecryptUserData(user);
        }
        return user;
    }

    public Task<bool> UpdateUserAsync(User user)
    {
        // Encrypt only truly sensitive data (phone number)
        if (!string.IsNullOrEmpty(user.Phone))
            user.Phone = _encryptionService.Encrypt(user.Phone);

        _context.Users.Update(user);
        _context.SaveChanges();
        return Task.FromResult(true);
    }

    // find user

    public async Task<User?> FindUserByEmailAndPasswordAsync(string email, string passwordHash)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(passwordHash, user.PasswordHash);
        if (isPasswordValid)
        {
            DecryptUserData(user);
            return user;
        }
        return null;
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

    public Task<string?> GetTokenByEmailAsync(string email)
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


    //admin
    /// <summary>
    /// Get total customers count within an optional date range
    /// </summary>
    /// <param name="fromDate"></param>
    /// <param name="toDate"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<int> GetTotalCustomersCountAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Users.AsQueryable();

        // Apply date filters if provided
        if (fromDate.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= toDate.Value);
        }

        // Count total customers
        var totalCount = await query.CountAsync();

        return totalCount;
    }

    #region Address Methods

    /// <summary>
    /// Get all addresses for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of user addresses</returns>
    public async Task<IEnumerable<Address>> GetUserAddressesAsync(int userId)
    {
        var addresses = await _context.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
        
        // Decrypt sensitive data before returning
        DecryptAddressesData(addresses);
        
        return addresses;
    }

    /// <summary>
    /// Get address by ID
    /// </summary>
    /// <param name="addressId">Address ID</param>
    /// <returns>Address or null if not found</returns>
    public async Task<Address?> GetAddressByIdAsync(int addressId)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId);
        
        if (address != null)
        {
            DecryptAddressData(address);
        }
        
        return address;
    }

    /// <summary>
    /// Add a new address
    /// </summary>
    /// <param name="address">Address to add</param>
    /// <returns>Added address</returns>
    public async Task<Address> AddAddressAsync(Address address)
    {
        try
        {
            Console.WriteLine($"🔐 ADDRESS: AddAddressAsync called for UserId: {address.UserId}");
            Console.WriteLine($"🔐 ADDRESS: Phone length: {address.Phone?.Length ?? 0}");
            Console.WriteLine($"🔐 ADDRESS: AddressLine1 length: {address.AddressLine1?.Length ?? 0}");
            
            // Set created date
            address.CreatedAt = DateTime.UtcNow;
            
            // Encrypt sensitive address data before saving
            EncryptAddressData(address);
            
            // If this is the first address for the user, make it default
            var existingAddressesCount = await _context.Addresses
                .CountAsync(a => a.UserId == address.UserId);
            
            if (existingAddressesCount == 0)
            {
                address.IsDefault = true;
            }
            
            Console.WriteLine($"🔐 ADDRESS: Adding to context...");
            _context.Addresses.Add(address);
            
            Console.WriteLine($"🔐 ADDRESS: Saving to database...");
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"🔐 ADDRESS: Address saved successfully with ID: {address.Id}");
            return address;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔐 ADDRESS ERROR: Exception type: {ex.GetType().Name}");
            Console.WriteLine($"🔐 ADDRESS ERROR: Message: {ex.Message}");
            Console.WriteLine($"🔐 ADDRESS ERROR: Inner exception: {ex.InnerException?.Message}");
            
            // Log to file
            var logPath = Path.Combine("logs", $"address-{DateTime.Now:yyyyMMdd}.txt");
            Directory.CreateDirectory("logs");
            try
            {
                await File.AppendAllTextAsync(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ADDRESS ERROR: {ex}\n");
            }
            catch
            {
                // Ignore file write errors to prevent recursive issues
            }
            
            throw new InvalidOperationException($"Failed to add address: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Update an existing address
    /// </summary>
    /// <param name="address">Address with updated information</param>
    /// <returns>True if successful, false if address not found</returns>
    public async Task<bool> UpdateAddressAsync(Address address)
    {
        var existingAddress = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == address.Id);
        
        if (existingAddress == null)
            return false;
        
        // Encrypt sensitive data before updating
        EncryptAddressData(address);
        
        // Update properties
        existingAddress.AddressType = address.AddressType;
        existingAddress.RecipientName = address.RecipientName;
        existingAddress.Phone = address.Phone;
        existingAddress.AddressLine1 = address.AddressLine1;
        existingAddress.AddressLine2 = address.AddressLine2;
        existingAddress.Ward = address.Ward;
        existingAddress.District = address.District;
        existingAddress.City = address.City;
        existingAddress.Province = address.Province;
        existingAddress.PostalCode = address.PostalCode;
        
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Delete an address
    /// </summary>
    /// <param name="addressId">Address ID to delete</param>
    /// <returns>True if successful, false if address not found</returns>
    public async Task<bool> DeleteAddressAsync(int addressId)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId);
        
        if (address == null)
            return false;
        
        // If deleting default address, set another address as default
        if (address.IsDefault == true)
        {
            var nextAddress = await _context.Addresses
                .Where(a => a.UserId == address.UserId && a.Id != addressId)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();
            
            if (nextAddress != null)
            {
                nextAddress.IsDefault = true;
            }
        }
        
        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();
        
        return true;
    }

    /// <summary>
    /// Set an address as default for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="addressId">Address ID to set as default</param>
    /// <returns>True if successful</returns>
    public async Task<bool> SetDefaultAddressAsync(int userId, int addressId)
    {
        // Verify the address belongs to the user
        var targetAddress = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
        
        if (targetAddress == null)
            return false;
        
        // Remove default from all user addresses
        var userAddresses = await _context.Addresses
            .Where(a => a.UserId == userId)
            .ToListAsync();
        
        foreach (var addr in userAddresses)
        {
            addr.IsDefault = addr.Id == addressId;
        }
        
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Get the default address for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Default address or null if not found</returns>
    public async Task<Address?> GetDefaultAddressAsync(int userId)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault == true);
        
        if (address != null)
        {
            DecryptAddressData(address);
        }
        
        return address;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Decrypt sensitive user data after retrieving from database
    /// </summary>
    /// <param name="user">User object to decrypt</param>
    private void DecryptUserData(User user)
    {
        try
        {
            // Only decrypt truly sensitive data (phone number)
            // Names and email remain unencrypted for search functionality
            if (!string.IsNullOrEmpty(user.Phone))
            {
                Console.WriteLine($"🔐 DECRYPT: Decrypting phone for user {user.Email}");
                user.Phone = _encryptionService.Decrypt(user.Phone);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔐 DECRYPT ERROR: {ex.Message}");
            // If decryption fails, leave data as is (might be legacy unencrypted data)
        }
    }

    /// <summary>
    /// Decrypt sensitive data for a list of users
    /// </summary>
    /// <param name="users">List of users to decrypt</param>
    private void DecryptUsersData(IEnumerable<User> users)
    {
        foreach (var user in users)
        {
            DecryptUserData(user);
        }
    }

    /// <summary>
    /// Encrypt sensitive address data before saving to database
    /// </summary>
    /// <param name="address">Address object to encrypt</param>
    private void EncryptAddressData(Address address)
    {
        try
        {
            // Encrypt sensitive address data: phone and address lines
            // Keep recipient name unencrypted for better UX and search
            if (!string.IsNullOrEmpty(address.Phone))
            {
                Console.WriteLine($"🔐 ADDRESS: Original phone: {address.Phone}");
                address.Phone = _encryptionService.Encrypt(address.Phone);
                Console.WriteLine($"🔐 ADDRESS: Encrypted phone length: {address.Phone.Length}");
            }
            
            if (!string.IsNullOrEmpty(address.AddressLine1))
            {
                Console.WriteLine($"🔐 ADDRESS: Original AddressLine1: {address.AddressLine1}");
                address.AddressLine1 = _encryptionService.Encrypt(address.AddressLine1);
                Console.WriteLine($"🔐 ADDRESS: Encrypted AddressLine1 length: {address.AddressLine1.Length}");
            }
            
            if (!string.IsNullOrEmpty(address.AddressLine2))
            {
                address.AddressLine2 = _encryptionService.Encrypt(address.AddressLine2);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔐 ADDRESS ERROR: {ex.Message}");
            throw; // Re-throw to prevent saving unencrypted data
        }
    }

    /// <summary>
    /// Decrypt sensitive address data after retrieving from database
    /// </summary>
    /// <param name="address">Address object to decrypt</param>
    private void DecryptAddressData(Address address)
    {
        try
        {
            // Decrypt sensitive address data: phone and address lines
            // Recipient name remains unencrypted
            if (!string.IsNullOrEmpty(address.Phone))
            {
                Console.WriteLine($"🔐 ADDRESS DECRYPT: Decrypting phone");
                address.Phone = _encryptionService.Decrypt(address.Phone);
            }
            
            if (!string.IsNullOrEmpty(address.AddressLine1))
            {
                Console.WriteLine($"🔐 ADDRESS DECRYPT: Decrypting AddressLine1");
                address.AddressLine1 = _encryptionService.Decrypt(address.AddressLine1);
            }
            
            if (!string.IsNullOrEmpty(address.AddressLine2))
            {
                address.AddressLine2 = _encryptionService.Decrypt(address.AddressLine2);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔐 ADDRESS DECRYPT ERROR: {ex.Message}");
            // If decryption fails, leave data as is (might be legacy unencrypted data)
        }
    }

    /// <summary>
    /// Decrypt sensitive data for a list of addresses
    /// </summary>
    /// <param name="addresses">List of addresses to decrypt</param>
    private void DecryptAddressesData(IEnumerable<Address> addresses)
    {
        foreach (var address in addresses)
        {
            DecryptAddressData(address);
        }
    }

    #endregion

}
