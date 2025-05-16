using TodoRESTApi.identity.Enums;

namespace TodoRESTApi.ServiceContracts.DTO.Response;

public class RoleServiceResponse
{
    public List<RoleResponse> RoleResponses { get; set; } = new List<RoleResponse>();
}

public class RoleResponse
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? NormalizedName { get; set; }
    public RoleType RoleType { get; set; }
    public List<RoleClaimResponse>? Claims { get; set; }
    public List<MetaRoleClaimResponse>? MetaClaims { get; set; }
    public List<RoleResponse> PrimeRoleWithClaim { get; set; } = new List<RoleResponse>();
}

public class RoleClaimResponse: IRoleClaim
{
    public int ClaimId { get; set; }
    
    public required string ClaimType { get; set; }
    
    public required string ClaimValue { get; set; }

    public bool? Checked { get; set; } = false;
}

public class MetaRoleClaimResponse: IRoleClaim
{
    public int MetaRoleClaimId { get; set; }
    
    public required string ClaimType { get; set; }
    
    public required string ClaimValue { get; set; }
}

public interface IRoleClaim
{
    string ClaimType { get; }
    string ClaimValue { get; }
}

/// <summary>
/// Provides extension methods for the Role entity.
/// </summary>
public static class RoleResponseExtension
{
    public static RoleServiceResponse ToRoleServiceResponse(this List<RoleResponse> roleResponse)
    {
        return new RoleServiceResponse()
        {
            RoleResponses = roleResponse
        };
    }
}
