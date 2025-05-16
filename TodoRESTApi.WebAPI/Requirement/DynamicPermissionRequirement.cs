using Microsoft.AspNetCore.Authorization;
using TodoRESTApi.ServiceContracts;

namespace TodoRESTApi.WebAPI.Requirement;

public class DynamicPermissionRequirement : IAuthorizationRequirement
{
    public string Role { get; }
    public string Action { get; }

    public DynamicPermissionRequirement(string role, string action)
    {
        Role = role;
        Action = action;
    }
}

public class DynamicPermissionHandler : AuthorizationHandler<DynamicPermissionRequirement>
{
    private readonly IRoleService _roleService;

    public DynamicPermissionHandler(IRoleService roleService)
    {
        _roleService = roleService;
    }
    
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DynamicPermissionRequirement requirement)
    {
        // Construct the claim type based on the role (scope)
        var claimType = $"Permission:{requirement.Role}";
        // Check if the user has a claim with this type and the matching action as value

        var userHasNormalClaim = context.User.HasClaim(c =>
            c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase) &&
            c.Value.Equals(requirement.Action, StringComparison.OrdinalIgnoreCase));

        var userHasMetaClaim = false;
                
        if (userHasNormalClaim == false)
        {
            userHasMetaClaim = await _roleService.UserHasMetaClaims(context.User, claimType, requirement.Action);
        }        
        
        if (userHasNormalClaim || userHasMetaClaim)
        {
            context.Succeed(requirement);
        }
    }
}