using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using TodoRESTApi.identity.Enums;

namespace TodoRESTApi.identity.Identity;

public class ApplicationRole: IdentityRole<Guid>
{
    [Display(Name = "Role Type")]
    public RoleType RoleType { get; set; }
}