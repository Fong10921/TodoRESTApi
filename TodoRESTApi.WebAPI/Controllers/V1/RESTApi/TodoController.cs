using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TodoRESTApi.Core.Enums;
using TodoRESTApi.Core.Models;
using TodoRESTApi.ServiceContracts;
using TodoRESTApi.ServiceContracts.DTO.Response;
using TodoRESTApi.ServiceContracts.Filters;

namespace TodoRESTApi.WebAPI.Controllers.V1.RESTApi;

[ApiController]
[Route("api/v{version:apiVersion}")]
[ApiVersion("1.0")]
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodoController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    [HttpGet("GetTodo")]
    public async Task<ActionResult<IEnumerable<TodoResponse>>> GetTodo([FromQuery] int? todoId,
        [FromQuery] string? category, [FromQuery] TodoStatus? status, [FromQuery] TodoPriority? priority)
    {
        TodoFilters todoFilters = new TodoFilters()
        {
            TodoId = todoId,
            Category = category,
            Priority = priority,
            Status = status,
            GetAll = !todoId.HasValue // If todoId is null, get all todos
        };

        var todos = await _todoService.GetTodoByTodoIdWithFilter(todoFilters);

        return Ok(todos);
    }
}