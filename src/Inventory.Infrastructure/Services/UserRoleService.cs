namespace Inventory.Infrastructure.Services;

using Inventory.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserRoleService : IUserRoleService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserRoleService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IdentityResult> CreateRoleAsync(string roleName)
    {
        if (await _roleManager.RoleExistsAsync(roleName))
            return IdentityResult.Success;

        return await _roleManager.CreateAsync(new IdentityRole(roleName));
    }

    public async Task<IdentityResult> DeleteRoleAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
            return IdentityResult.Success;

        return await _roleManager.DeleteAsync(role);
    }

    public Task<bool> RoleExistsAsync(string roleName) => _roleManager.RoleExistsAsync(roleName);

    public async Task<IdentityResult> CreateUserAsync(string email, string password, IEnumerable<string>? roles = null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return result;

        if (roles != null)
        {
            // Ensure roles exist
            foreach (var r in roles.Distinct())
            {
                if (!await _roleManager.RoleExistsAsync(r))
                    await _roleManager.CreateAsync(new IdentityRole(r));
            }

            result = await _userManager.AddToRolesAsync(user, roles);
        }

        return result;
    }

    public Task<IdentityResult> UpdateUserAsync(ApplicationUser user) => _userManager.UpdateAsync(user);

    public async Task<IdentityResult> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return IdentityResult.Success;

        return await _userManager.DeleteAsync(user);
    }

    public Task<ApplicationUser?> FindUserByIdAsync(string userId) => _userManager.FindByIdAsync(userId);

    public async Task<IList<ApplicationUser>> GetAllUsersAsync()
    {
        return await _userManager.Users.ToListAsync();
    }

    public Task<IList<string>> GetUserRolesAsync(ApplicationUser user) => _userManager.GetRolesAsync(user);

    public Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string role)
    {
        return _userManager.AddToRoleAsync(user, role);
    }

    public Task<IdentityResult> RemoveUserFromRoleAsync(ApplicationUser user, string role)
    {
        return _userManager.RemoveFromRoleAsync(user, role);
    }
}
