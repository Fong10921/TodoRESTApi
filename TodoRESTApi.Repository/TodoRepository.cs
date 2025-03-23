using Microsoft.EntityFrameworkCore;
using TodoRESTApi.Entities.Context;
using TodoRESTApi.Entities.Entities;
using TodoRESTApi.RepositoryContracts;
using TodoRESTApi.ServiceContracts.DTO.Request;
using TodoRESTApi.ServiceContracts.DTO.Response;
using TodoRESTApi.ServiceContracts.Filters;

namespace TodoRESTApi.Repository;

public class TodoRepository: ITodoRepository
{
    private readonly TodoDbContext _db;

    public TodoRepository(TodoDbContext db)
    {
        _db = db;
    }
    
    
    public async Task<Todo> AddTodo(Todo todo)
    {
        _db.Todo.Add(todo);
        await _db.SaveChangesAsync();

        return todo;
    }

    public async Task<List<Todo>> GetTodosBasedOnFilters(TodoFilters todoFilters)
    {
        IQueryable<Todo> query = _db.Todo.AsQueryable();

        // If TodoId is provided, return the specific todo
        if (todoFilters.TodoId.HasValue)
        {
            return await query.Where(todo => todo.Id == todoFilters.TodoId.Value).ToListAsync();
        }
        
        // if GetAll is false return empty list 
        if (todoFilters.GetAll == false &&
            string.IsNullOrEmpty(todoFilters.Category) &&
            !todoFilters.Priority.HasValue &&
            !todoFilters.Status.HasValue)
        {
            return new List<Todo>(); 
        }

        if (!string.IsNullOrEmpty(todoFilters.Category))
        {
            query = query.Where(todo => todo.Category == todoFilters.Category);
        }

        if (todoFilters.Priority.HasValue)
        {
            query = query.Where(todo => todo.Priority == todoFilters.Priority.Value);
        }

        if (todoFilters.Status.HasValue)
        {
            query = query.Where(todo => todo.Status == todoFilters.Status.Value);
        }

        query = query.Where(todo => todo.IsDeleted != true);

        // Return all todos if GetAll is true
        return await query.ToListAsync();
    }

    public async Task<TodoResponse?> UpdateTodo(TodoUpdateRequest todoUpdateRequest)
    {
        var existingTodo = await _db.Todo.FirstOrDefaultAsync(t => t.Id == todoUpdateRequest.Id);

        if (existingTodo == null)
        {
            throw new ArgumentException(nameof(todoUpdateRequest));
        }
        
        existingTodo.Name = todoUpdateRequest.Name;
        existingTodo.Description = todoUpdateRequest.Description;
        existingTodo.DueDate = todoUpdateRequest.DueDate;
        existingTodo.Status = todoUpdateRequest.Status;
        existingTodo.Priority = todoUpdateRequest.Priority;
        existingTodo.Category = todoUpdateRequest.Category;
        existingTodo.IsDeleted = todoUpdateRequest.IsDeleted;

        await _db.SaveChangesAsync();
        
        return existingTodo.ToTodoResponse();
    }

    public async Task<bool> SoftDeleteTodo(int todoId)
    {
        var existingTodo = await _db.Todo.FirstOrDefaultAsync(t => t.Id == todoId);
        
        if (existingTodo == null)
        {
            throw new ArgumentException($"Todo with ID {todoId} not found");
        }

        existingTodo.IsDeleted = true;
        
        return await _db.SaveChangesAsync() > 0;

    }

    public async Task<bool> DestroyTodo(int? todoId)
    {
        var existingTodo = await _db.Todo.FirstOrDefaultAsync(t => t.Id == todoId);

        if (existingTodo == null)
        {
            throw new ArgumentException($"Todo with ID {todoId} not found");
        }

        _db.Todo.Remove(existingTodo); 
        return await _db.SaveChangesAsync() > 0; 
    }

}