using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace TodoRESTApi.WebAPI.Provider;

public class CompositeAuthorizationPolicyProvider: DefaultAuthorizationPolicyProvider
{
    private readonly OrAuthorizationPolicyProvider _orAuthorizationPolicyProvider;
    private readonly DynamicAuthorizationPolicyProvider _dynamicAuthorizationPolicyProvider;
    
    public CompositeAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
    {
        _orAuthorizationPolicyProvider = new OrAuthorizationPolicyProvider(options);
        _dynamicAuthorizationPolicyProvider = new DynamicAuthorizationPolicyProvider(options);
    }
    
    public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        // First, check if the policy matches the "Permission:" format
        if (policyName.StartsWith("Permission:", StringComparison.OrdinalIgnoreCase))
        {
            return await _dynamicAuthorizationPolicyProvider.GetPolicyAsync(policyName);
        }

        // Then, check if the policy matches the "OR:" format
        if (policyName.StartsWith("OR:", StringComparison.OrdinalIgnoreCase))
        {
            return await _orAuthorizationPolicyProvider.GetPolicyAsync(policyName);
        }

        // Fallback to default policy provider behavior
        return await base.GetPolicyAsync(policyName);
    }
}