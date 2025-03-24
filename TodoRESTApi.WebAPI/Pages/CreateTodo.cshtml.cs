using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoRESTApi.ServiceContracts.DTO.Request;

namespace TodoRESTApi.WebAPI.Pages;

public class CreateTodoModel : PageModel
{
    private readonly HttpClient _httpClient;
    
    public CreateTodoModel(HttpClient httpClient)
    {
        _httpClient = httpClient;
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