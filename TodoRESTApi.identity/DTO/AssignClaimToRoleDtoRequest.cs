using System.ComponentModel.DataAnnotations;

namespace TodoRESTApi.identity.DTO;

public class AssignClaimToRoleDtoRequest
{
    public Guid? MetaRoleToUpdate { get; set; }
    
    [Required]
    public List<RolePermissionClaimDto> RolePermissionClaimDtos { get; set; } = new List<RolePermissionClaimDto>();
}