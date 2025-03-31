using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using TodoRESTApi.WebAPI.Requirement;

namespace TodoRESTApi.WebAPI.Provider;

public class OrAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public OrAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        // Check if the policy name starts with our OR prefix.
        if (policyName.StartsWith("OR:"))
        {
            // Remove the prefix and split the policy names.
            var policies = policyName.Substring(3).Split(';');
            var policyBuilder = new AuthorizationPolicyBuilder();
            policyBuilder.AddRequirements(new OrAuthorizationRequirement(policies));
            return policyBuilder.Build();
        }

        return await base.GetPolicyAsync(policyName);
    }
}