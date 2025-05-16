using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TodoRESTApi.identity.DTO;
using TodoRESTApi.identity.Enums;
using TodoRESTApi.identity.Identity;
using TodoRESTApi.ServiceContracts;
using TodoRESTApi.ServiceContracts.DTO.Response;
using TodoRESTApi.ServiceContracts.Filters;
using TodoRESTApi.WebAPI.Requirement;

namespace TodoRESTApi.WebAPI.Controllers.V1.RESTApi;

[Route("api/v1/[controller]/[action]")]
[ApiController]
[ApiVersion("1.0")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuthorizationOptions _authorizationOptions;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IRoleService _roleService;
    private readonly IAuthorizationPolicyProvider _policyProvider;
    private readonly ILogger<RoleController> _logger;

    public RoleController(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager,
        IAuthorizationPolicyProvider policyProvider, ILogger<RoleController> logger,
        IOptions<AuthorizationOptions> authorizationOptions, SignInManager<ApplicationUser> signInManager,
        IRoleService roleService)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _policyProvider = policyProvider;
        _logger = logger;
        _signInManager = signInManager;
        _roleService = roleService;
        _authorizationOptions = authorizationOptions.Value;
    }

    [HttpPost]
    public async Task<ActionResult> CreateRole([FromBody] CreateRoleDto registerDto)
    {
        if (ModelState.IsValid == false)
        {
            string errorMessage = string.Join(" | ",
                ModelState.Values.SelectMany(value => value.Errors).Select(e => e.ErrorMessage));

            return Problem(errorMessage);
        }

        var result = await _roleService.CreateRoleAsync(registerDto);

        if (result.Errors.Any())
        {
            var errorMessageAfterCreate = string.Join(" | ", result.Errors.Select(e => e.Description));
            return Problem(errorMessageAfterCreate);
        }

        var refreshResult = await _roleService.RefreshUser(User);

        if (refreshResult.Errors.Any())
        {
            var errorMessageAfterRefresh = string.Join(" | ", refreshResult.Errors.Select(e => e.Description));
            return Problem(errorMessageAfterRefresh);
        }

        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult> AssignRoleToUser([FromBody] AssignRoleToUserDto assignRoleToUserDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return BadRequest(new { Errors = errors });
        }

        var result = await _roleService.AssignRoleToUser(assignRoleToUserDto);

        if (result.Errors.Any())
        {
            var errorMessageAfterAssigning = string.Join(" | ", result.Errors.Select(e => e.Description));
            return Problem(errorMessageAfterAssigning);
        }

        var refreshResult = await _roleService.RefreshUser(User);

        if (refreshResult.Errors.Any())
        {
            var errorMessageAfterRefresh = string.Join(" | ", refreshResult.Errors.Select(e => e.Description));
            return Problem(errorMessageAfterRefresh);
        }

        return Ok(new { Message = "User has been added to role successfully." });
    }

    [HttpPatch]
    public async Task<ActionResult> AssignClaimToRole([FromBody] AssignClaimToRoleDtoRequest assignClaimToRoleDtoRequest)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return BadRequest(new { Errors = errors });
        }
        
        var result = await _roleService.AssignClaimToRole(assignClaimToRoleDtoRequest);

        if (result.Errors.Any())
        {
            var errorMessageAfterAssigning = string.Join(" | ", result.Errors.Select(e => e.Description));
            return Problem(errorMessageAfterAssigning);
        }

        var refreshUserResult = await _roleService.RefreshUser(User);
                    
        if (refreshUserResult.Errors.Any())
        {
            var errorMessageAfterRefresh = string.Join(" | ", refreshUserResult.Errors.Select(e => e.Description));
            return Problem(errorMessageAfterRefresh);
        }

        return Ok();
    }

    [HttpGet("policy/{policyName}")]
    public async Task<IActionResult> GetPolicy(string policyName)
    {
        var policy = await _policyProvider.GetPolicyAsync(policyName);

        if (policy == null)
        {
            return NotFound(new { message = $"Policy '{policyName}' not found." });
        }

        // Extract requirement details
        var requirementDetails = policy.Requirements
            .OfType<DynamicPermissionRequirement>() // Ensure we're handling our custom requirement
            .Select(r => new
            {
                Role = r.Role,
                Action = r.Action
            }).ToList();

        return Ok(new
        {
            PolicyName = policyName,
            AuthenticationSchemes = policy.AuthenticationSchemes,
            Requirements = requirementDetails
        });
    }

    [HttpGet("debug/user-claims")]
    public IActionResult GetUserClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(claims);
    }

    [HttpGet("list-policies")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public IActionResult GetAllPolicies()
    {
        var policyNames = _authorizationOptions.PolicyNames().ToList();
        return Ok(policyNames);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(RoleServiceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRoles([FromQuery] RoleType roleType, [FromQuery] Guid metaRoleId)
    {
        RoleFilters roleFilters = new RoleFilters()
        {
            RoleTypeToGet = roleType,
            MetaRoleId = metaRoleId
        };
         
        RoleServiceResponse roles = await _roleService.GetAllRole(roleFilters);
        return Ok(roles);
    }
}

public static class AuthorizationOptionsExtensions
{
    public static IEnumerable<string> PolicyNames(this AuthorizationOptions options)
    {
        return options.GetType()
            .GetProperty("PolicyMap",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(options) is Dictionary<string, AuthorizationPolicy> policies
            ? policies.Keys
            : new List<string>();
    }
}