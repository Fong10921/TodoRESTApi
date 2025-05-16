using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TodoRESTApi.Core.Enums;
using TodoRESTApi.Core.ExternalHelperInterface;
using TodoRESTApi.ServiceContracts.DTO.Response;

namespace TodoRESTApi.WebAPI.Pages;

[Authorize(Policy = "Permission:Todo:CanView")]
[ValidateAntiForgeryToken]
public class IndexModel : PageModel
{
    private readonly HttpClient _httpClient;
    private readonly INodaTimeHelper _nodaTimeHelper;

    public List<TodoResponse>? Todos { get; set; } = new();

    [BindProperty(SupportsGet = true)] public int? TodoId { get; set; }

    [BindProperty(SupportsGet = true)] public string? FilterCategory { get; set; }

    [BindProperty(SupportsGet = true)] public TodoStatus? FilterStatus { get; set; }

    [BindProperty(SupportsGet = true)] public TodoPriority? FilterPriority { get; set; }

    [BindProperty(SupportsGet = true)] public string? Name { get; set; }
    [BindProperty(SupportsGet = true)] public string TimeZone { get; set; } = null!;
    [BindProperty(SupportsGet = true)] public DateTime? FromDueDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? ToDueDate { get; set; }

    [BindProperty(SupportsGet = true)] public TodoSortField? SortBy { get; set; } = TodoSortField.Name;
    [BindProperty(SupportsGet = true)] public bool SortDescending { get; set; }

    public IndexModel(IHttpClientFactory httpClientFactory, INodaTimeHelper nodaTimeHelper)
    {
        _httpClient = httpClientFactory.CreateClient("WithCookies");
        _nodaTimeHelper = nodaTimeHelper;
    }

    public async Task OnGetAsync()
    {
        string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        string apiUrl = $"{baseUrl}/api/v1/GetTodo";

        // Create a dictionary for query parameters
        var queryParams = new Dictionary<string, string?>
        {
            { "name", Name },
            { "category", FilterCategory },
            { "status", FilterStatus.HasValue ? ((int)FilterStatus.Value).ToString() : null },
            { "priority", FilterPriority.HasValue ? ((int)FilterPriority.Value).ToString() : null },
            { "fromDueDate", _nodaTimeHelper.FormatDate(FromDueDate) },
            { "toDueDate", _nodaTimeHelper.FormatDate(ToDueDate) },
            { "timeZone", TimeZone },
            { "sortBy", SortBy.HasValue ? ((int)SortBy.Value).ToString() : null },
            { "sortDescending", SortDescending.ToString() }
        };

        // Append query parameters dynamically
        apiUrl = QueryHelpers.AddQueryString(apiUrl, queryParams);
        try
        {
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    Todos = await response.Content.ReadFromJsonAsync<List<TodoResponse>>();
                }
                catch (JsonException ex)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Log errorContent and ex.Message for debugging
                    throw new Exception($"Error parsing JSON. Response content: {errorContent}", ex);
                }
            }
            else
            {
                Todos = new List<TodoResponse>();
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching todos: {ex.Message}");
        }
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        string apiUrl = $"{baseUrl}/api/v1/SoftDeleteTodo";

        var queryParams = new Dictionary<string, string?>
        {
            { "todoId", TodoId.ToString() },
        };

        apiUrl = QueryHelpers.AddQueryString(apiUrl, queryParams);

        var response = await _httpClient.PatchAsync(apiUrl, null);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to delete Todo.");
            return Page();
        }

        return LocalRedirect("/");
    }
}