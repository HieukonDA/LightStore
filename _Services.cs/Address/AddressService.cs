namespace TheLightStore.Services.Addresses;

public class AddressService : IAddressService
{
    private readonly IUserRepo _userRepo;
    private readonly ILogger<AddressService> _logger;

    public AddressService(IUserRepo userRepo, ILogger<AddressService> logger)
    {
        _userRepo = userRepo;
        _logger = logger;
    }

    /// <summary>
    /// Get all addresses for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Service result with list of user addresses</returns>
    public async Task<ServiceResult<IEnumerable<Address>>> GetUserAddressesAsync(int userId)
    {
        try
        {
            if (userId <= 0)
            {
                return ServiceResult<IEnumerable<Address>>.FailureResult(
                    "ID người dùng không hợp lệ", 
                    new List<string> { "User ID must be greater than 0" });
            }

            var addresses = await _userRepo.GetUserAddressesAsync(userId);
            return ServiceResult<IEnumerable<Address>>.SuccessResult(
                addresses, 
                "Lấy danh sách địa chỉ thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user addresses for user {UserId}", userId);
            return ServiceResult<IEnumerable<Address>>.FailureResult(
                "Lỗi khi lấy danh sách địa chỉ", 
                new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Get address by ID
    /// </summary>
    /// <param name="addressId">Address ID</param>
    /// <returns>Service result with address or error if not found</returns>
    public async Task<ServiceResult<Address>> GetAddressByIdAsync(int addressId)
    {
        try
        {
            if (addressId <= 0)
            {
                return ServiceResult<Address>.FailureResult(
                    "ID địa chỉ không hợp lệ", 
                    new List<string> { "Address ID must be greater than 0" });
            }

            var address = await _userRepo.GetAddressByIdAsync(addressId);
            if (address == null)
            {
                return ServiceResult<Address>.FailureResult(
                    "Không tìm thấy địa chỉ", 
                    new List<string> { $"Address with ID {addressId} not found" });
            }

            return ServiceResult<Address>.SuccessResult(address, "Lấy thông tin địa chỉ thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting address {AddressId}", addressId);
            return ServiceResult<Address>.FailureResult(
                "Lỗi khi lấy thông tin địa chỉ", 
                new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Add a new address
    /// </summary>
    /// <param name="address">Address to add</param>
    /// <returns>Service result with added address</returns>
    public async Task<ServiceResult<Address>> AddAddressAsync(Address address)
    {
        try
        {
            // Validate address
            var validationResult = ValidateAddress(address);
            if (!validationResult.Success)
            {
                return ServiceResult<Address>.FailureResult(
                    validationResult.Message, 
                    validationResult.Errors);
            }

            var addedAddress = await _userRepo.AddAddressAsync(address);
            return ServiceResult<Address>.SuccessResult(
                addedAddress, 
                "Thêm địa chỉ thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding address for user {UserId}", address?.UserId);
            return ServiceResult<Address>.FailureResult(
                "Lỗi khi thêm địa chỉ", 
                new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Update an existing address
    /// </summary>
    /// <param name="address">Address with updated information</param>
    /// <returns>Service result indicating success or failure</returns>
    public async Task<ServiceResult<bool>> UpdateAddressAsync(Address address)
    {
        try
        {
            // Validate address
            var validationResult = ValidateAddress(address);
            if (!validationResult.Success)
            {
                return ServiceResult<bool>.FailureResult(
                    validationResult.Message, 
                    validationResult.Errors);
            }

            var success = await _userRepo.UpdateAddressAsync(address);
            if (!success)
            {
                return ServiceResult<bool>.FailureResult(
                    "Không tìm thấy địa chỉ để cập nhật", 
                    new List<string> { $"Address with ID {address.Id} not found" });
            }

            return ServiceResult<bool>.SuccessResult(true, "Cập nhật địa chỉ thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating address {AddressId}", address?.Id);
            return ServiceResult<bool>.FailureResult(
                "Lỗi khi cập nhật địa chỉ", 
                new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Delete an address
    /// </summary>
    /// <param name="addressId">Address ID to delete</param>
    /// <returns>Service result indicating success or failure</returns>
    public async Task<ServiceResult<bool>> DeleteAddressAsync(int addressId)
    {
        try
        {
            if (addressId <= 0)
            {
                return ServiceResult<bool>.FailureResult(
                    "ID địa chỉ không hợp lệ", 
                    new List<string> { "Address ID must be greater than 0" });
            }

            var success = await _userRepo.DeleteAddressAsync(addressId);
            if (!success)
            {
                return ServiceResult<bool>.FailureResult(
                    "Không tìm thấy địa chỉ để xóa", 
                    new List<string> { $"Address with ID {addressId} not found" });
            }

            return ServiceResult<bool>.SuccessResult(true, "Xóa địa chỉ thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting address {AddressId}", addressId);
            return ServiceResult<bool>.FailureResult(
                "Lỗi khi xóa địa chỉ", 
                new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Set an address as default for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="addressId">Address ID to set as default</param>
    /// <returns>Service result indicating success or failure</returns>
    public async Task<ServiceResult<bool>> SetDefaultAddressAsync(int userId, int addressId)
    {
        try
        {
            if (userId <= 0)
            {
                return ServiceResult<bool>.FailureResult(
                    "ID người dùng không hợp lệ", 
                    new List<string> { "User ID must be greater than 0" });
            }

            if (addressId <= 0)
            {
                return ServiceResult<bool>.FailureResult(
                    "ID địa chỉ không hợp lệ", 
                    new List<string> { "Address ID must be greater than 0" });
            }

            var success = await _userRepo.SetDefaultAddressAsync(userId, addressId);
            if (!success)
            {
                return ServiceResult<bool>.FailureResult(
                    "Không thể đặt địa chỉ mặc định", 
                    new List<string> { "Address not found or does not belong to user" });
            }

            return ServiceResult<bool>.SuccessResult(true, "Đặt địa chỉ mặc định thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default address {AddressId} for user {UserId}", addressId, userId);
            return ServiceResult<bool>.FailureResult(
                "Lỗi khi đặt địa chỉ mặc định", 
                new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Get the default address for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Service result with default address or null if not found</returns>
    public async Task<ServiceResult<Address?>> GetDefaultAddressAsync(int userId)
    {
        try
        {
            if (userId <= 0)
            {
                return ServiceResult<Address?>.FailureResult(
                    "ID người dùng không hợp lệ", 
                    new List<string> { "User ID must be greater than 0" });
            }

            var defaultAddress = await _userRepo.GetDefaultAddressAsync(userId);
            return ServiceResult<Address?>.SuccessResult(
                defaultAddress, 
                defaultAddress != null ? "Lấy địa chỉ mặc định thành công" : "Người dùng chưa có địa chỉ mặc định");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default address for user {UserId}", userId);
            return ServiceResult<Address?>.FailureResult(
                "Lỗi khi lấy địa chỉ mặc định", 
                new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Validate address data
    /// </summary>
    /// <param name="address">Address to validate</param>
    /// <returns>Service result indicating validation success or failure with errors</returns>
    public ServiceResult<bool> ValidateAddress(Address address)
    {
        var errors = new List<string>();

        if (address == null)
        {
            errors.Add("Address cannot be null");
            return ServiceResult<bool>.FailureResult("Dữ liệu địa chỉ không hợp lệ", errors);
        }

        if (address.UserId <= 0)
        {
            errors.Add("User ID is required and must be greater than 0");
        }

        if (string.IsNullOrWhiteSpace(address.AddressType))
        {
            errors.Add("Address type is required");
        }

        if (string.IsNullOrWhiteSpace(address.RecipientName))
        {
            errors.Add("Recipient name is required");
        }

        if (string.IsNullOrWhiteSpace(address.Phone))
        {
            errors.Add("Phone number is required");
        }
        else if (address.Phone.Length < 10 || address.Phone.Length > 15)
        {
            errors.Add("Phone number must be between 10 and 15 characters");
        }

        if (string.IsNullOrWhiteSpace(address.AddressLine1))
        {
            errors.Add("Address line 1 is required");
        }

        if (string.IsNullOrWhiteSpace(address.Ward))
        {
            errors.Add("Ward is required");
        }

        if (string.IsNullOrWhiteSpace(address.District))
        {
            errors.Add("District is required");
        }

        if (string.IsNullOrWhiteSpace(address.City))
        {
            errors.Add("City is required");
        }

        if (string.IsNullOrWhiteSpace(address.Province))
        {
            errors.Add("Province is required");
        }

        if (errors.Any())
        {
            return ServiceResult<bool>.FailureResult("Dữ liệu địa chỉ không hợp lệ", errors);
        }

        return ServiceResult<bool>.SuccessResult(true, "Address validation passed");
    }
}