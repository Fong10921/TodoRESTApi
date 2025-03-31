using Microsoft.AspNetCore.Authorization;

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
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DynamicPermissionRequirement requirement)
    {
        // Construct the claim type based on the role (scope)
        var claimType = $"Permission:{requirement.Role}";
        // Check if the user has a claim with this type and the matching action as value
        if (context.User.HasClaim(c =>
                c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase) &&
                c.Value.Equals(requirement.Action, StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}