using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TodoRESTApi.identity.Identity;

public class MetaRoleClaimsPivot
{
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Column("RoleClaimId")]
    public int RoleClaimId { get; set; }
    
    [ForeignKey("RoleClaimId")]
    public IdentityRoleClaim<Guid> IdentityRoleClaim { get; set; } = null!;
    
    [Column("MetaRoleId")]
    public Guid MetaRoleId { get; set; }
    
    [ForeignKey("MetaRoleId")]
    public MetaRole MetaRole { get; set; } = null!;
    
}