using Microsoft.AspNetCore.Authorization;

namespace TheLightStore.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // Require authentication for all address operations
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;
    private readonly ILogger<AddressController> _logger;

    public AddressController(IAddressService addressService, ILogger<AddressController> logger)
    {
        _addressService = addressService;
        _logger = logger;
    }

    /// <summary>
    /// Get all addresses for the current user
    /// </summary>
    /// <returns>List of user addresses</returns>
    [HttpGet("my-addresses")]
    public async Task<IActionResult> GetMyAddresses()
    {
        try
        {
            // Get user ID from token claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var result = await _addressService.GetUserAddressesAsync(userId);
            
            if (result.Success)
            {
                return Ok(new
                {
                    message = result.Message,
                    data = result.Data
                });
            }

            return BadRequest(new
            {
                message = result.Message,
                errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user addresses");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all addresses for a specific user (Admin only)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of user addresses</returns>
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserAddresses(int userId)
    {
        try
        {
            var result = await _addressService.GetUserAddressesAsync(userId);
            
            if (result.Success)
            {
                return Ok(new
                {
                    message = result.Message,
                    data = result.Data
                });
            }

            return BadRequest(new
            {
                message = result.Message,
                errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting addresses for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get address by ID
    /// </summary>
    /// <param name="id">Address ID</param>
    /// <returns>Address details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAddressById(int id)
    {
        try
        {
            var result = await _addressService.GetAddressByIdAsync(id);
            
            if (result.Success)
            {
                // Verify that the address belongs to the current user (unless admin)
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin");
                
                if (!isAdmin && result.Data != null)
                {
                    if (int.TryParse(userIdClaim, out int userId) && result.Data.UserId != userId)
                    {
                        return Forbid("You can only access your own addresses");
                    }
                }

                return Ok(new
                {
                    message = result.Message,
                    data = result.Data
                });
            }

            return NotFound(new
            {
                message = result.Message,
                errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting address {AddressId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new address
    /// </summary>
    /// <param name="createDto">Address creation data</param>
    /// <returns>Created address</returns>
    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody] AddressCreateDto createDto)
    {
        try
        {
            // Get user ID from token claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            // Ensure the address is created for the current user (unless admin)
            if (!User.IsInRole("Admin"))
            {
                createDto.UserId = userId;
            }

            // Map DTO to Address model
            var address = new Address
            {
                UserId = createDto.UserId,
                AddressType = createDto.AddressType,
                RecipientName = createDto.RecipientName,
                Phone = createDto.Phone,
                AddressLine1 = createDto.AddressLine1,
                AddressLine2 = createDto.AddressLine2,
                Ward = createDto.Ward,
                District = createDto.District,
                City = createDto.City,
                Province = createDto.Province,
                PostalCode = createDto.PostalCode
            };

            var result = await _addressService.AddAddressAsync(address);
            
            if (result.Success)
            {
                return CreatedAtAction(
                    nameof(GetAddressById),
                    new { id = result.Data!.Id },
                    new
                    {
                        message = result.Message,
                        data = result.Data
                    });
            }

            return BadRequest(new
            {
                message = result.Message,
                errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating address");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update an existing address
    /// </summary>
    /// <param name="id">Address ID</param>
    /// <param name="updateDto">Address update data</param>
    /// <returns>Update result</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAddress(int id, [FromBody] AddressUpdateDto updateDto)
    {
        try
        {
            // Get the existing address first to verify ownership
            var existingResult = await _addressService.GetAddressByIdAsync(id);
            if (!existingResult.Success || existingResult.Data == null)
            {
                return NotFound(new { message = "Address not found" });
            }

            // Verify ownership (unless admin)
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin)
            {
                if (!int.TryParse(userIdClaim, out int userId) || existingResult.Data.UserId != userId)
                {
                    return Forbid("You can only update your own addresses");
                }
            }

            // Map DTO to Address model
            var address = new Address
            {
                Id = id,
                UserId = existingResult.Data.UserId, // Keep original user ID
                AddressType = updateDto.AddressType,
                RecipientName = updateDto.RecipientName,
                Phone = updateDto.Phone,
                AddressLine1 = updateDto.AddressLine1,
                AddressLine2 = updateDto.AddressLine2,
                Ward = updateDto.Ward,
                District = updateDto.District,
                City = updateDto.City,
                Province = updateDto.Province,
                PostalCode = updateDto.PostalCode
            };

            var result = await _addressService.UpdateAddressAsync(address);
            
            if (result.Success)
            {
                return Ok(new
                {
                    message = result.Message,
                    data = result.Data
                });
            }

            return BadRequest(new
            {
                message = result.Message,
                errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating address {AddressId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete an address
    /// </summary>
    /// <param name="id">Address ID</param>
    /// <returns>Delete result</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        try
        {
            // Get the existing address first to verify ownership
            var existingResult = await _addressService.GetAddressByIdAsync(id);
            if (!existingResult.Success || existingResult.Data == null)
            {
                return NotFound(new { message = "Address not found" });
            }

            // Verify ownership (unless admin)
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin)
            {
                if (!int.TryParse(userIdClaim, out int userId) || existingResult.Data.UserId != userId)
                {
                    return Forbid("You can only delete your own addresses");
                }
            }

            var result = await _addressService.DeleteAddressAsync(id);
            
            if (result.Success)
            {
                return Ok(new
                {
                    message = result.Message
                });
            }

            return BadRequest(new
            {
                message = result.Message,
                errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting address {AddressId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Set an address as default
    /// </summary>
    /// <param name="id">Address ID</param>
    /// <returns>Set default result</returns>
    [HttpPatch("{id}/set-default")]
    public async Task<IActionResult> SetDefaultAddress(int id)
    {
        try
        {
            // Get user ID from token claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            // Verify that the address belongs to the current user
            var existingResult = await _addressService.GetAddressByIdAsync(id);
            if (!existingResult.Success || existingResult.Data == null)
            {
                return NotFound(new { message = "Address not found" });
            }

            if (!User.IsInRole("Admin") && existingResult.Data.UserId != userId)
            {
                return Forbid("You can only set your own addresses as default");
            }

            var result = await _addressService.SetDefaultAddressAsync(userId, id);
            
            if (result.Success)
            {
                return Ok(new
                {
                    message = result.Message
                });
            }

            return BadRequest(new
            {
                message = result.Message,
                errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default address {AddressId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get the default address for current user
    /// </summary>
    /// <returns>Default address</returns>
    [HttpGet("default")]
    public async Task<IActionResult> GetDefaultAddress()
    {
        try
        {
            // Get user ID from token claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var result = await _addressService.GetDefaultAddressAsync(userId);
            
            if (result.Success)
            {
                return Ok(new
                {
                    message = result.Message,
                    data = result.Data
                });
            }

            return BadRequest(new
            {
                message = result.Message,
                errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default address");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get default address for a specific user (Admin only)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Default address</returns>
    [HttpGet("user/{userId}/default")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserDefaultAddress(int userId)
    {
        try
        {
            var result = await _addressService.GetDefaultAddressAsync(userId);
            
            if (result.Success)
            {
                return Ok(new
                {
                    message = result.Message,
                    data = result.Data
                });
            }

            return BadRequest(new
            {
                message = result.Message,
                errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default address for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}