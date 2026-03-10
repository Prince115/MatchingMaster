
using Microsoft.AspNetCore.Identity;

namespace Inventory.Infrastructure.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}