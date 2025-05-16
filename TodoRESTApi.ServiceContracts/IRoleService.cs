using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TodoRESTApi.identity.DTO;
using TodoRESTApi.identity.Identity;
using TodoRESTApi.ServiceContracts.DTO.Response;
using TodoRESTApi.ServiceContracts.Filters;

namespace TodoRESTApi.ServiceContracts;

/// <summary>
/// Defines the contract for role-related operations in the application.
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Creates a new role based on the provided DTO.
    /// </summary>
    /// <param name="registerDto">The data transfer object containing role details.</param>
    /// <returns>
    /// An <see cref="IdentityResult"/> indicating the success or failure of the role creation.
    /// </returns>
    Task<IdentityResult> CreateRoleAsync(CreateRoleDto registerDto);

    /// <summary>
    /// Assigns a role to a specified user.
    /// </summary>
    /// <param name="assignRoleToUserDto">The DTO containing user and role assignment details.</param>
    /// <returns>
    /// An <see cref="IdentityResult"/> indicating whether the role was successfully assigned.
    /// </returns>
    Task<IdentityResult> AssignRoleToUser(AssignRoleToUserDto assignRoleToUserDto);
    
    /// <summary>
    /// Refreshes the claims and roles of the given user.
    /// </summary>
    /// <param name="applicationUser">The user whose claims need to be refreshed.</param>
    /// <returns>
    /// An <see cref="IdentityResult"/> indicating whether the refresh operation was successful.
    /// </returns>
    Task<IdentityResult> RefreshUser(ClaimsPrincipal applicationUser);

    /// <summary>
    /// Assigns or removes claims for a specified role based on provided permissions.
    /// </summary>
    /// <param name="assignClaimToRoleDtoRequest">The DTO containing role name and associated permissions.</param>
    /// <returns>
    /// An <see cref="IdentityResult"/> indicating whether the claim assignment was successful.
    /// </returns>
    Task<IdentityResult> AssignClaimToRole(AssignClaimToRoleDtoRequest assignClaimToRoleDtoRequest);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="claimType"></param>
    /// <param name="claimValue"></param>
    /// <returns></returns>
    Task<bool> UserHasMetaClaims(ClaimsPrincipal user, string claimType, string claimValue);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="roleFilters"></param>
    /// <returns></returns>
    Task<RoleServiceResponse> GetAllRole(RoleFilters roleFilters);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metaRoleName"></param>
    /// <returns></returns>
    Task<MetaRole?> FindMetaRoleBasedOnMetaRoleName(string metaRoleName);
}