using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TodoRESTApi.Entities.Context;
using TodoRESTApi.identity.Identity;
using TodoRESTApi.RepositoryContracts;

namespace TodoRESTApi.Repository;

public class UserRepository: IUserRepository
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMetaRoleRepository _metaRoleRepository;
    private readonly TodoDbContext _db;
    
    public UserRepository(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signInManager, TodoDbContext db, IMetaRoleRepository metaRoleRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _db = db;
        _metaRoleRepository = metaRoleRepository;
    }
    
    public async Task<ApplicationUser?> FindUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId.ToString());
    }
    public async Task<IdentityResult?> UpdateUserAsync(ApplicationUser applicationUser)
    {
        return await _userManager.UpdateAsync(applicationUser);
    }
    public async Task<bool> IsUserInRoleAsync(ApplicationUser applicationUser, string applicationRoleName)
    {
        return await _userManager.IsInRoleAsync(applicationUser, applicationRoleName);
    }
    
    public async Task<IdentityResult> AddUserToRoleAsync(ApplicationUser applicationUser, string applicationRoleName)
    {
        return await _userManager.AddToRoleAsync(applicationUser, applicationRoleName);
    }

    public async Task<ApplicationUser?> GetUserUsingClaimPrinciple(ClaimsPrincipal claimsPrincipal)
    {
        return await _userManager.GetUserAsync(claimsPrincipal);
    }

    public async Task<IdentityResult> UpdateSecurityStampAsync(ApplicationUser applicationUser)
    {
        return await _userManager.UpdateSecurityStampAsync(applicationUser);
    }
    



}