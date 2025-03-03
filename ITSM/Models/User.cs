using Microsoft.AspNetCore.Identity;

namespace ITSM.Models;

public class User:IdentityUser
{
    public string? Role { get; set; }
}