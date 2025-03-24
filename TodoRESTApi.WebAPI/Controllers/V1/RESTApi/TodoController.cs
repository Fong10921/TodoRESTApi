using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TodoRESTApi.Core.Enums;
using TodoRESTApi.Core.Models;
using TodoRESTApi.ServiceContracts;
using TodoRESTApi.ServiceContracts.DTO.Request;
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
            TodoId = todoId, // If todoId is provided skip other filters
            Category = category,
            Priority = priority,
            Status = status,
            GetAll = !todoId.HasValue // If todoId is null, get all todos
        };

        var todos = await _todoService.GetTodoByTodoIdWithFilter(todoFilters);

        return Ok(todos);
    }

    [HttpPost("PostTodo")]
    public async Task<ActionResult<TodoResponse>> PostTodo([FromBody] TodoAddRequest todoData)
    {
        TodoResponse todoAddResponse = await _todoService.AddTodo(todoData);

        // Status 201 Created
        return CreatedAtAction(nameof(GetTodo), new { todoId = todoAddResponse.Id }, todoAddResponse);
    }
    
    [HttpPatch("PatchTodo")]
    public async Task<ActionResult<TodoResponse>> PatchTodo([FromBody] TodoUpdateRequest todoData)
    {
        TodoResponse? updatedTodo = await _todoService.UpdateTodo(todoData);

        if (updatedTodo == null)
        {
            return NotFound($"Todo with ID {todoData.Id} not found.");
        }

        // Status 200 Created
        return Ok(updatedTodo); 
    }
    
    [HttpPatch("SoftDeleteTodo")]
    public async Task<ActionResult> SoftDeleteTodo([FromQuery] int todoId)
    {
        bool successfullySoftDeleted = await _todoService.SoftDeleteTodo(todoId);

        if (!successfullySoftDeleted)
        {
            return NotFound($"Todo with ID {todoId} not found.");
        }

        return Ok($"Todo with ID {todoId} has been soft deleted.");
    }
    
    [HttpDelete("DestroyTodo")]
    public async Task<ActionResult> DestroyTodo([FromQuery] int todoId)
    {
        bool successfullyDestroyTodo = await _todoService.DestroyTodo(todoId);

        if (!successfullyDestroyTodo)
        {
            return NotFound($"Todo with ID {todoId} not found.");
        }

        return Ok($"Todo with ID {todoId} has been destroy.");
    }
    
}