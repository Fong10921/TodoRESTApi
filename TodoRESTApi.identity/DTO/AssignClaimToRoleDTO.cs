using System.ComponentModel.DataAnnotations;

namespace TodoRESTApi.identity.DTO;

public class AssignClaimToRoleDto
{
    [Required]
    public IEnumerable<RolePermissionClaimDto> RolePermissionClaimDtos { get; set; } =
        new List<RolePermissionClaimDto>();
}