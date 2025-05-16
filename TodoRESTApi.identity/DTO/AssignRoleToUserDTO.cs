using System.ComponentModel.DataAnnotations;

namespace TodoRESTApi.identity.DTO;

public class AssignRoleToUserDto
{
    [Required]
    public required string UserId { get; set; }
    
    [Required]
    public required string RoleId { get; set; }
}