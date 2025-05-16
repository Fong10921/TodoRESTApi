using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using TodoRESTApi.identity.Attributes;

namespace TodoRESTApi.identity.Identity;

public class MetaRole
{
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    [Unique]
    public required string MetaRoleName { get; set; }

    public virtual ICollection<MetaRoleClaimsPivot> MetaRoleClaimsPivots { get; set; } =
        new List<MetaRoleClaimsPivot>();
    
    public virtual ICollection<IdentityRoleClaim<Guid>> RoleClaims { get; set; } = new List<IdentityRoleClaim<Guid>>();
}

