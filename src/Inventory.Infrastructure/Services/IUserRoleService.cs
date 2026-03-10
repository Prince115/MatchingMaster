namespace Inventory.Infrastructure.Services;

using Inventory.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;

public interface IUserRoleService
{
    Task<IdentityResult> CreateRoleAsync(string roleName);
    Task<IdentityResult> DeleteRoleAsync(string roleName);
    Task<bool> RoleExistsAsync(string roleName);

    Task<IdentityResult> CreateUserAsync(string email, string password, IEnumerable<string>? roles = null);
    Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
    Task<IdentityResult> DeleteUserAsync(string userId);

    Task<ApplicationUser?> FindUserByIdAsync(string userId);
    Task<IList<ApplicationUser>> GetAllUsersAsync();
    Task<IList<string>> GetUserRolesAsync(ApplicationUser user);

    Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string role);
    Task<IdentityResult> RemoveUserFromRoleAsync(ApplicationUser user, string role);
}
