using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TodoRESTApi.identity.Identity;
using TodoRESTApi.ServiceContracts.DTO.Response;
using TodoRESTApi.ServiceContracts.Filters;

namespace TodoRESTApi.RepositoryContracts;

public interface IRoleRepository
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationRole"></param>
    /// <returns></returns>
    public Task<IdentityResult> CreateRole(ApplicationRole applicationRole);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationRole"></param>
    /// <param name="claim"></param>
    /// <returns></returns>
    public Task<IdentityResult> AddClaimAsync(ApplicationRole applicationRole, Claim claim);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public Task<ApplicationRole?> FindRoleByIdAsync(string roleId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="roleName"></param>
    /// <returns></returns>
    public Task<ApplicationRole?> FindRoleByNameAsync(string roleName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationRole"></param>
    /// <returns></returns>
    public Task<List<Claim>> GetRoleClaims(ApplicationRole applicationRole);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationRole"></param>
    /// <param name="claims"></param>
    /// <returns></returns>
    public Task<IdentityResult> RemoveClaimsFromRole(ApplicationRole applicationRole, Claim claims);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationRole"></param>
    /// <returns></returns>
    public Task<List<IdentityRoleClaim<Guid>>?> FindClaimBasedOnRole(ApplicationRole applicationRole);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="claimsId"></param>
    /// <param name="metaRoleId"></param>
    /// <returns></returns>
    public Task AddMetaRoleClaim(int claimsId, Guid metaRoleId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metaRoleId"></param>
    /// <returns></returns>
    public Task<MetaRoleClaimsPivot?> FindMetaRoleClaimsBasedOnId(Guid metaRoleId, string claimType, string claimValue);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metaRoleClaimId"></param>
    /// <returns></returns>
    public Task<bool> DestroyMetaRolePivot(Guid metaRoleClaimId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationUser"></param>
    /// <param name="claimType"></param>
    /// <param name="claimValue"></param>
    /// <param name="metaRole"></param>
    /// <returns></returns>
    public bool UserHasMetaClaims(ApplicationUser applicationUser, string claimType, string claimValue,
        MetaRole metaRole);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="roleFilters"></param>
    /// <returns></returns>
    public Task<List<RoleResponse>?> GetPageRoleIncludingMetaRoleAndClaims(RoleFilters roleFilters);
}