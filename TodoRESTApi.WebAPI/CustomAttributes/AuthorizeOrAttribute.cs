namespace TodoRESTApi.WebAPI.CustomAttributes;

using Microsoft.AspNetCore.Authorization;

public class AuthorizeOrAttribute : AuthorizeAttribute
{
    public AuthorizeOrAttribute(params string[] policies)
    {
        // Prefix the policy string to identify it later in the policy provider.
        Policy = $"OR:{string.Join(";", policies)}";
    }
}
