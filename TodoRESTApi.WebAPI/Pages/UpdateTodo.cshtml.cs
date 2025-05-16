using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TodoRESTApi.ServiceContracts.DTO.Request;
using TodoRESTApi.ServiceContracts.DTO.Response;

namespace TodoRESTApi.WebAPI.Pages;

[Authorize(Policy = "Permission:Todo:CanEdit")]
public class UpdateTodoModel : PageModel
{
    private readonly HttpClient _httpClient;

    public UpdateTodoModel(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("WithCookies");
    }

    [BindProperty] public TodoUpdateRequest TodoUpdateRequest { get; set; } = new TodoUpdateRequest();

    [BindProperty(SupportsGet = true)] public int Id { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        string getApiUrl = $"{baseUrl}/api/v1/GetTodo";
        
        var queryParams = new Dictionary<string, string?>
        {
            { "todoId", Id.ToString() },
        };

        getApiUrl = QueryHelpers.AddQueryString(getApiUrl, queryParams!);

        try
        {
            var response = await _httpClient.GetAsync(getApiUrl);

            if (response.IsSuccessStatusCode)
            {
                // Assign the value to the TodoUpdateRequest to auto fill the data
                var todoResponses = await response.Content.ReadFromJsonAsync<List<TodoResponse>>();
                if (todoResponses is { Count: > 0 })
                {
                    var todoResponse = todoResponses[0];
                    TodoUpdateRequest.Name = todoResponse.Name;
                    TodoUpdateRequest.Description = todoResponse.Description;
                    TodoUpdateRequest.Priority = todoResponse.Priority;
                    TodoUpdateRequest.Status = todoResponse.Status;
                    TodoUpdateRequest.DueDate = todoResponse.DueDate;
                    TodoUpdateRequest.Category = todoResponse.Category;
                    TodoUpdateRequest.TimeZone = todoResponse.TimeZone;
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching todos: {ex.Message}");
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        string apiUrl = $"{baseUrl}/api/v1/Role/AssignClaimToRole";

        TodoUpdateRequest.Id = Id;

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(TodoUpdateRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PatchAsync(apiUrl, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to update Todo.");
            return Page();
        }

        return LocalRedirect("/");
    }
}