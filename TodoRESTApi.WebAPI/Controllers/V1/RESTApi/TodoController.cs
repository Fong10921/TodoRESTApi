﻿using System.Diagnostics;
using System.Diagnostics.Metrics;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoRESTApi.Core.Enums;
using TodoRESTApi.Core.TelemetryInterface;
using TodoRESTApi.ServiceContracts;
using TodoRESTApi.ServiceContracts.DTO.Request;
using TodoRESTApi.ServiceContracts.DTO.Response;
using TodoRESTApi.ServiceContracts.Filters;
using TodoRESTApi.WebAPI.CustomAttributes;

namespace TodoRESTApi.WebAPI.Controllers.V1.RESTApi;

[ApiController]
[Route("api/v{version:apiVersion}")]
[ApiVersion("1.0")]
[Authorize] 
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;
    private readonly ILogger<TodoController> _logger;
    private readonly IGreetingTelemetry _telemetry;

    public TodoController(ITodoService todoService, ILogger<TodoController> logger, IGreetingTelemetry telemetry)
    {
        _todoService = todoService;
        _logger = logger;
        _telemetry = telemetry;
    }
    
    [HttpGet("GetTodo")]
    [AuthorizeOr("Permission:Todo:CanView", "Permission:Todo:CanEdit")]
    public async Task<ActionResult<IEnumerable<TodoResponse>>> GetTodo([FromQuery] string? name,
        [FromQuery] int? todoId,
        [FromQuery] string? category, [FromQuery] TodoStatus? status, [FromQuery] TodoPriority? priority,
        [FromQuery] DateTime? fromDueDate, [FromQuery] DateTime? toDueDate, [FromQuery] TodoSortField? sortBy,
        [FromQuery] bool? sortDescending, [FromQuery] string? timeZone)
    {
        TodoFilters todoFilters = new TodoFilters()
        {
            TodoId = todoId, // If todoId is provided skip other filters
            Name = name,
            Category = category,
            FromDueDate = fromDueDate,
            ToDueDate = toDueDate,
            Priority = priority,
            SortBy = sortBy,
            TimeZone = timeZone,
            SortDescending = sortDescending != null && (bool)sortDescending,
            Status = status,
            GetAll = !todoId.HasValue // If todoId is null, get all todos
        };
        
        _telemetry.TrackGreeting();

        using var activity = _telemetry.StartGreetingActivity("GreetedUser");

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

        // Status 200 
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
    
    [HttpGet("TestAuth")]
    public IActionResult TestAuth()
    {
        return Ok(new { User = User.Identity.Name, IsAuthenticated = User.Identity.IsAuthenticated });
    }
}