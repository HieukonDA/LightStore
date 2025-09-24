namespace TheLightStore.Interfaces.Addresses;

public interface IAddressService
{
    /// <summary>
    /// Get all addresses for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Service result with list of user addresses</returns>
    Task<ServiceResult<IEnumerable<Address>>> GetUserAddressesAsync(int userId);

    /// <summary>
    /// Get address by ID
    /// </summary>
    /// <param name="addressId">Address ID</param>
    /// <returns>Service result with address or error if not found</returns>
    Task<ServiceResult<Address>> GetAddressByIdAsync(int addressId);

    /// <summary>
    /// Add a new address
    /// </summary>
    /// <param name="address">Address to add</param>
    /// <returns>Service result with added address</returns>
    Task<ServiceResult<Address>> AddAddressAsync(Address address);

    /// <summary>
    /// Update an existing address
    /// </summary>
    /// <param name="address">Address with updated information</param>
    /// <returns>Service result indicating success or failure</returns>
    Task<ServiceResult<bool>> UpdateAddressAsync(Address address);

    /// <summary>
    /// Delete an address
    /// </summary>
    /// <param name="addressId">Address ID to delete</param>
    /// <returns>Service result indicating success or failure</returns>
    Task<ServiceResult<bool>> DeleteAddressAsync(int addressId);

    /// <summary>
    /// Set an address as default for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="addressId">Address ID to set as default</param>
    /// <returns>Service result indicating success or failure</returns>
    Task<ServiceResult<bool>> SetDefaultAddressAsync(int userId, int addressId);

    /// <summary>
    /// Get the default address for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Service result with default address or null if not found</returns>
    Task<ServiceResult<Address?>> GetDefaultAddressAsync(int userId);

    /// <summary>
    /// Validate address data
    /// </summary>
    /// <param name="address">Address to validate</param>
    /// <returns>Service result indicating validation success or failure with errors</returns>
    ServiceResult<bool> ValidateAddress(Address address);
}