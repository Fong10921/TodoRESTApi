using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TodoRESTApi.identity.DTO;
using TodoRESTApi.identity.Enums;
using TodoRESTApi.ServiceContracts.DTO.Response;

namespace TodoRESTApi.WebAPI.Pages.Role;

public class RoleModel : PageModel
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RoleModel> _logger;
    
    [BindProperty] // Require to preserve after Post Error
    public RoleResponse RoleResponse { get; set; } 

    [BindProperty(SupportsGet = true)] public Guid RoleId { get; set; }

    [BindProperty] 
    public required AssignClaimToRoleDtoRequest AssignClaimToRoleDtoRequest { get; set; }
    
    public RoleModel(IHttpClientFactory httpClientFactory, ILogger<RoleModel> logger)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("WithCookies");
    }
    
    private async Task LoadRoleResponseAsync()
    {
        string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        string apiUrl = $"{baseUrl}/api/v1/Role/GetAllRoles";

        var queryParams = new Dictionary<string, string?>
        {
            { "roleType", nameof(RoleType.MetaLevel) },
            { "metaRoleId", RoleId.ToString() }
        };

        apiUrl = QueryHelpers.AddQueryString(apiUrl, queryParams);

        var response = await _httpClient.GetAsync(apiUrl);
        if (response.IsSuccessStatusCode)
        {
            var roleServiceResponse = await response.Content.ReadFromJsonAsync<RoleServiceResponse>() ??
                                      new RoleServiceResponse();
            RoleResponse = roleServiceResponse.RoleResponses.FirstOrDefault() ?? new RoleResponse();
            
            var metaClaimsSet = RoleResponse.MetaClaims
                .Select(mc => (mc.ClaimType, mc.ClaimValue))
                .ToHashSet();
            
            foreach (var primeRole in RoleResponse.PrimeRoleWithClaim)
            {
                foreach (var primeRoleClaim in primeRole.Claims)
                {
                    primeRoleClaim.Checked = metaClaimsSet.Contains((primeRoleClaim.ClaimType, primeRoleClaim.ClaimValue));
                }
            }

        }
        else
        {
            RoleResponse = new RoleResponse();
        }
    }


    public async Task<IActionResult> OnGetAsync()
    {
        await LoadRoleResponseAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        AssignClaimToRoleDtoRequest.MetaRoleToUpdate = RoleId;
        
        if (!ModelState.IsValid)
        {
            await LoadRoleResponseAsync();  
            
            return Page();
        }
        
        string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        string apiUrl = $"{baseUrl}/api/v1/Role/AssignClaimToRole";
        
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(AssignClaimToRoleDtoRequest),
            Encoding.UTF8,
            "application/json"
        );
        
        var response = await _httpClient.PatchAsync(apiUrl, jsonContent);
        
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to update Permission.");
            return Page();
        }

        return LocalRedirect("/Role");
    }
}