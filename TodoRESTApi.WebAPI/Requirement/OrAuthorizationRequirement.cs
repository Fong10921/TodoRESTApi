using Microsoft.AspNetCore.Authorization;
using TodoRESTApi.ServiceContracts;

namespace TodoRESTApi.WebAPI.Requirement;

public class OrAuthorizationRequirement : IAuthorizationRequirement
{
    public string[] Policies { get; }

    public OrAuthorizationRequirement(string[] policies)
    {
        Policies = policies;
    }
}

public class OrAuthorizationHandler : AuthorizationHandler<OrAuthorizationRequirement>
{
    private readonly IRoleService _roleService;

    public OrAuthorizationHandler(IRoleService roleService)
    {
        _roleService = roleService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OrAuthorizationRequirement requirement)
    {
        foreach (var policy in requirement.Policies)
        {
            var parts = policy.Split(':');
            if (parts.Length == 3)
            {
                var claimType = parts[0] + ":" + parts[1];
                var claimValue = parts[2];

                var userHasNormalClaim = context.User.HasClaim(c => c.Type == claimType && c.Value == claimValue);
                var userHasMetaClaim = false;
                
                if (userHasNormalClaim == false)
                {
                    userHasMetaClaim = await _roleService.UserHasMetaClaims(context.User, claimType, claimValue);
                }

                if (userHasNormalClaim || userHasMetaClaim)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}