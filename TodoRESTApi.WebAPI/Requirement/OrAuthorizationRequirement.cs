using Microsoft.AspNetCore.Authorization;

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
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OrAuthorizationRequirement requirement)
    {
        foreach (var policy in requirement.Policies)
        {
            var parts = policy.Split(':');
            if (parts.Length == 3)
            {
                var claimType = parts[0] + ":" + parts[1];
                var claimValue = parts[2];
                
                if (context.User.HasClaim(c => c.Type == claimType && c.Value == claimValue))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }
        }
        return Task.CompletedTask;
    }
}