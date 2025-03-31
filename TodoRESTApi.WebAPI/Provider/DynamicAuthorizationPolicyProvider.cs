using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using TodoRESTApi.WebAPI.Requirement;

namespace TodoRESTApi.WebAPI.Provider;

public class DynamicAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public DynamicAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
    {
    }

    public override Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        // Expecting policy names like "Permission:Todo:CanView"
        if (policyName.StartsWith("Permission:", StringComparison.OrdinalIgnoreCase))
        {
            var parts = policyName.Split(':');
            if (parts.Length == 3)
            {
                var role = parts[1];
                var action = parts[2];
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new DynamicPermissionRequirement(role, action));
                return Task.FromResult(policy.Build());
            }
        }
        // Fallback to the default behavior
        return base.GetPolicyAsync(policyName);
    }
}