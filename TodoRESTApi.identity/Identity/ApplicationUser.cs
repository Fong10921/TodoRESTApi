using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TodoRESTApi.identity.Identity;

public class ApplicationUser: IdentityUser<Guid>
{
    public string? PersonName { get; set; }
    
    public string? RefreshToken { get; set; }
    
    public DateTime RefreshTokenExpirationDateTime { get; set; }
    
    [Column("MetaRoleId")]
    public Guid? MetaRoleId { get; set; }
    
    [ForeignKey("MetaRoleId")]
    public MetaRole MetaRole { get; set; } = null!;
}