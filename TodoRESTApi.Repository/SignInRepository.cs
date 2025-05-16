using Microsoft.AspNetCore.Identity;
using TodoRESTApi.identity.Identity;
using TodoRESTApi.RepositoryContracts;

namespace TodoRESTApi.Repository;

public class SignInRepository: ISignInRepository
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    
    public SignInRepository(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
    }
    
    public async Task RefreshSignInUser(ApplicationUser applicationUser)
    {
        await _signInManager.RefreshSignInAsync(applicationUser);
    }

}