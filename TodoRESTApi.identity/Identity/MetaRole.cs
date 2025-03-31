using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TodoRESTApi.identity.Identity;

public class MetaRole
{
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public string MetaRoleName { get; set; }
    
    [Column("RoleClaimsId")]
    public List<int>? RoleClaimsId { get; set; }
    
    /*[ForeignKey("RoleClaimsId")]
    public IdentityRoleClaim<Guid> IdentityRoleClaim { get; set; } = null!;*/
    
    public virtual ICollection<IdentityRoleClaim<Guid>> RoleClaims { get; set; } = new List<IdentityRoleClaim<Guid>>();
}