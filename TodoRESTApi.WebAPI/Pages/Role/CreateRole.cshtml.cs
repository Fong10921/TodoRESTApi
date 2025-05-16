using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoRESTApi.identity.DTO;

namespace TodoRESTApi.WebAPI.Pages.Role;

[Authorize]
public class CreateRole: PageModel
{
    private readonly HttpClient _httpClient;
    
    public CreateRole(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("WithCookies");
    }

    [BindProperty] public CreateRoleDto CreateRoleDto { get; set; } = new CreateRoleDto();
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            string apiUrl = $"{baseUrl}/api/v1/Role/CreateRole";

            var response = await _httpClient.PostAsJsonAsync(apiUrl, CreateRoleDto);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                try
                {
                    using var document = JsonDocument.Parse(errorResponse);
                    if (document.RootElement.TryGetProperty("detail", out JsonElement detail))
                    {
                        string errorMessage = detail.GetString();
                        ModelState.AddModelError(string.Empty, errorMessage);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "An unknown error occurred.");
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while processing your request.");
                }
                return Page();
            }
            
            return LocalRedirect("/Role");
        }
        
        return Page();
    }
}