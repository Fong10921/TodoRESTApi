using System.ComponentModel.DataAnnotations;
using TodoRESTApi.identity.Enums;
using TodoRESTApi.identity.Identity;

namespace TodoRESTApi.identity.DTO;

public class CreateRoleDto
{
    [Required(ErrorMessage = "Role Name can't be blank")]
    [Display(Name = "Role Name")]
    public string RoleName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Role Type can't be empty")]
    [Display(Name = "Role Type")]
    public RoleType RoleType { get; set; }
    
    public MetaRole ToMetaRole()
    {
        return new MetaRole
        {
            MetaRoleName = this.RoleName
        };
    }
}

