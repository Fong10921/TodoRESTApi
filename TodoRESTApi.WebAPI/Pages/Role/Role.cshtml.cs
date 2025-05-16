using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TodoRESTApi.identity.Enums;
using TodoRESTApi.ServiceContracts.DTO.Response;

namespace TodoRESTApi.WebAPI.Pages.Role;

[Authorize]
public class Role: PageModel
{
    private readonly HttpClient _httpClient;

    public required RoleServiceResponse RoleServiceResponse;
    
    [BindProperty(SupportsGet = true)] 
    public Guid RoleId { get; set; }
    
    public Role(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("WithCookies");
    }
    
    public async Task<IActionResult> OnGetAsync()
    {
        if (ModelState.IsValid)
        {
            string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            string apiUrl = $"{baseUrl}/api/v1/Role/GetAllRoles";
            
            var queryParams = new Dictionary<string, string?>
            {
                { "roleType", nameof(RoleType.MetaLevel) },
            };
            
            apiUrl = QueryHelpers.AddQueryString(apiUrl, queryParams);

            var response = await _httpClient.GetAsync(apiUrl);
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    RoleServiceResponse = await response.Content.ReadFromJsonAsync<RoleServiceResponse>() ?? new RoleServiceResponse();
                }
                catch (JsonException ex)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Log errorContent and ex.Message for debugging
                    throw new Exception($"Error parsing JSON. Response content: {errorContent}", ex);
                }
            } else
            {
                RoleServiceResponse = new RoleServiceResponse();
            }
        }

        return Page();
    }
}