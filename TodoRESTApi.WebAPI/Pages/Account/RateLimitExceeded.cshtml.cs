using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace TodoRESTApi.WebAPI.Pages.Account;

[AllowAnonymous]
[DisableRateLimiting]
public class RateLimitExceeded: PageModel
{
    
}