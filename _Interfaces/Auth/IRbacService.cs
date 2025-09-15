namespace TheLightStore.Interfaces.Auth;

public interface IRbacService
{
    Task<bool> HasPermissionAsync(int userId, string permission);
    Task<bool> HasRoleAsync(int userId, string roleName);
    Task<IEnumerable<string>> GetUserPermissionsAsync(int userId);
    Task<IEnumerable<string>> GetUserRolesAsync(int userId);
    Task<bool> AssignRoleToUserAsync(int userId, int roleId);
    Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
}