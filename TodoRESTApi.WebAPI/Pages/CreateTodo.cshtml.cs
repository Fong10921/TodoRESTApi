using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoRESTApi.ServiceContracts.DTO.Request;

namespace TodoRESTApi.WebAPI.Pages;

[Authorize(Policy = "Permission:Todo:CanCreate")]
public class CreateTodoModel : PageModel
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CreateTodoModel> _logger;
    
    public CreateTodoModel(IHttpClientFactory httpClientFactory, ILogger<CreateTodoModel> logger)
    {
        _httpClient = httpClientFactory.CreateClient("WithCookies");
        _logger = logger;
    }
    
    [BindProperty]
    public TodoAddRequest TodoAddRequest { get; set; } = new TodoAddRequest();
    
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }
    
    public void OnGet()
    {
    }
    
    public async Task<IActionResult> OnPost()
    {
        string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        
        string apiUrl = $"{baseUrl}/api/v1/PostTodo";
        
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        await _httpClient.PostAsync(apiUrl, new StringContent(JsonSerializer.Serialize(TodoAddRequest), Encoding.UTF8, "application/json"));
        
        return LocalRedirect("/");
    }
}