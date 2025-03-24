using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TodoRESTApi.Core.Enums;
using TodoRESTApi.ServiceContracts.DTO.Response;

namespace TodoRESTApi.WebAPI.Pages;

public class IndexModel : PageModel
{
    private readonly HttpClient _httpClient;
    public List<TodoResponse>? Todos { get; set; } = new();

    [BindProperty(SupportsGet = true)] public int? TodoId { get; set; }

    [BindProperty(SupportsGet = true)] public string? FilterCategory { get; set; }

    [BindProperty(SupportsGet = true)] public TodoStatus? FilterStatus { get; set; }

    [BindProperty(SupportsGet = true)] public TodoPriority? FilterPriority { get; set; }

    [BindProperty(SupportsGet = true)] public string? Name { get; set; }

    [BindProperty(SupportsGet = true)] public DateTime? FromDueDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? ToDueDate { get; set; }

    [BindProperty(SupportsGet = true)] public TodoSortField? SortBy { get; set; } = TodoSortField.Name;
    [BindProperty(SupportsGet = true)] public bool SortDescending { get; set; }

    public IndexModel(HttpClient httpClient)
    {
        _httpClient = httpClient;
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
            { "fromDueDate", FromDueDate?.ToString("yyyy-MM-dd HH:mm:ss") },
            { "toDueDate", ToDueDate?.ToString("yyyy-MM-dd HH:mm:ss") },
            { "sortBy", SortBy.HasValue ? ((int)SortBy.Value).ToString() : null },
            { "sortDescending", SortDescending.ToString() }
        };

        // Append query parameters dynamically
        apiUrl = QueryHelpers.AddQueryString(apiUrl, queryParams!);

        try
        {
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                Todos = await response.Content.ReadFromJsonAsync<List<TodoResponse>>();
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

        apiUrl = QueryHelpers.AddQueryString(apiUrl, queryParams!);

        var response = await _httpClient.PatchAsync(apiUrl, null);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to delete Todo.");
            return Page();
        }

        return LocalRedirect("/");
    }
}