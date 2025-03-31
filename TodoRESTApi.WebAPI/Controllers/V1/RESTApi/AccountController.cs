using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoRESTApi.identity.DTO;
using TodoRESTApi.identity.Identity;

namespace TodoRESTApi.WebAPI.Controllers.V1.RESTApi;

[Route("api/v1/[controller]/[action]")]
[ApiController]
[ApiVersion("1.0")]
[AllowAnonymous]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost]
    public async Task<ActionResult<ApplicationUser>> PostRegister(RegisterDto registerDto)
    {
        if (ModelState.IsValid == false)
        {
            string errorMessage = string.Join(" | ",
                ModelState.Values.SelectMany(value => value.Errors).Select(e => e.ErrorMessage));

            return Problem(errorMessage);
        }

        ApplicationUser user = new ApplicationUser()
        {
            Email = registerDto.Email,
            PhoneNumber = registerDto.PhoneNumber,
            UserName = registerDto.Email,
            PersonName = registerDto.PersonName
        };

        IdentityResult result = await _userManager.CreateAsync(user, registerDto.Password);

        if (result.Succeeded == true)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            
            return Ok();
        }
        else
        {
            string errorMessage = string.Join(" | ", result.Errors.Select(e => e.Description));
            return Problem(errorMessage);
        }
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> PostLogin(LoginDto loginDto)
    {
        if (ModelState.IsValid == false)
        {
            string errorMessage = string.Join(" | ",
                ModelState.Values.SelectMany(value => value.Errors).Select(e => e.ErrorMessage));

            return Problem(errorMessage);
        }

        var result = await _signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (result.Succeeded == true)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return NoContent();
            }
                
            return Ok(user);
        }
        else
        {
            return Problem("Invalid email or password");
        }
    }
    
    [HttpGet("logout")]
    public async Task<ActionResult> GetLogout()
    {
        await _signInManager.SignOutAsync();

        return NoContent();
    }
}