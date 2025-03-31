using System.ComponentModel.DataAnnotations;

namespace TodoRESTApi.identity.DTO;

public class AssignRoleToUserDto
{
    [Required]
    public string UserId { get; set; }
    
    [Required]
    public string RoleId { get; set; }
}