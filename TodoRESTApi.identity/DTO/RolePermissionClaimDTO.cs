using System.ComponentModel.DataAnnotations;

namespace TodoRESTApi.identity.DTO;

public class RolePermissionClaimDto
{
    [Required] public string RoleName { get; set; } = null!;
    [Required] public bool CanCreate { get; set; }
    [Required] public bool CanView { get; set; }
    [Required] public bool CanEdit { get; set; }
    [Required] public bool CanDelete { get; set; }
    [Required] public bool CanExport { get; set; }
}