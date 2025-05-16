using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TodoRESTApi.identity.Identity;

namespace TodoRESTApi.RepositoryContracts;

public interface IUserRepository
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<ApplicationUser?> FindUserByIdAsync(string userId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationUser"></param>
    /// <returns></returns>
    public Task<IdentityResult?> UpdateUserAsync(ApplicationUser applicationUser);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationUser"></param>
    /// <param name="applicationRoleName"></param>
    /// <returns></returns>
    public Task<bool> IsUserInRoleAsync(ApplicationUser applicationUser, string applicationRoleName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationUser"></param>
    /// <param name="applicationRoleName"></param>
    /// <returns></returns>
    public Task<IdentityResult> AddUserToRoleAsync(ApplicationUser applicationUser, string applicationRoleName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="claimsPrincipal"></param>
    /// <returns></returns>
    public Task<ApplicationUser?> GetUserUsingClaimPrinciple(ClaimsPrincipal claimsPrincipal);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationUser"></param>
    /// <returns></returns>
    public Task<IdentityResult> UpdateSecurityStampAsync(ApplicationUser applicationUser);

    /*/// <summary>
    /// 
    /// </summary>
    /// <param name="applicationUser"></param>
    /// <param name="metaRoleId"></param>
    /// <returns></returns>
    public Task<bool> CheckIfUserHasMetaRole(ApplicationUser applicationUser, string metaRoleId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationUser"></param>
    /// <param name="metaRoldId"></param>
    /// <returns></returns>
    public Task<IdentityResult> AddMetaRoleToUser(ApplicationUser applicationUser, Guid metaRoldId);*/
}