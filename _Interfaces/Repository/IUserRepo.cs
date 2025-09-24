namespace TheLightStore.Interfaces.Repository;

public interface IUserRepo
{
    //crud
    Task<PagedResult<User>> GetCustomersAsync(PagedRequest request);
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
    Task<User?> GetUserByResetTokenAsync(string token);

    //otp
    Task<bool> SaveOtpAsync(string email, string otp, DateTime expiryTime);
    Task<bool> ValidateAndResetPasswordAsync(string email, string otp, string newPasswordHash);



    //CẦN THÊM cho Customer Stats:
    Task<int> GetTotalCustomersCountAsync(DateTime? fromDate = null, DateTime? toDate = null);

    // address
    // Lấy tất cả địa chỉ của user
    Task<IEnumerable<Address>> GetUserAddressesAsync(int userId);

    // Lấy chi tiết 1 địa chỉ theo id
    Task<Address?> GetAddressByIdAsync(int addressId);

    // Thêm địa chỉ mới
    Task<Address> AddAddressAsync(Address address);

    // Cập nhật địa chỉ
    Task<bool> UpdateAddressAsync(Address address);

    // Xóa địa chỉ
    Task<bool> DeleteAddressAsync(int addressId);

    // Đặt địa chỉ mặc định
    Task<bool> SetDefaultAddressAsync(int userId, int addressId);

    // Lấy địa chỉ mặc định
    Task<Address?> GetDefaultAddressAsync(int userId);
}
