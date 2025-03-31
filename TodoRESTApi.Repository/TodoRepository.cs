using Microsoft.EntityFrameworkCore;
using TodoRESTApi.Core.Enums;
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
            !todoFilters.Status.HasValue &&
            string.IsNullOrEmpty(todoFilters.Name) &&
            !todoFilters.FromDueDate.HasValue &&
            !todoFilters.ToDueDate.HasValue)
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
        
        if (!string.IsNullOrEmpty(todoFilters.Name))
        {
            query = query.Where(todo => todo.Name.Contains(todoFilters.Name));
        }

        if (todoFilters.FromDueDate.HasValue)
        {
            DateTime fromDate = todoFilters.FromDueDate.Value; 
            query = query.Where(todo => todo.DueDate >= fromDate);
        }

        if (todoFilters.ToDueDate.HasValue)
        {
            DateTime toDate = todoFilters.ToDueDate.Value; 
            query = query.Where(todo => todo.DueDate < toDate);
        }
        
        query = query.Where(todo => todo.IsDeleted != true);
        
        if (todoFilters.SortBy.HasValue)
        {
            switch (todoFilters.SortBy)
            {
                case TodoSortField.Name:
                    query = todoFilters.SortDescending 
                        ? query.OrderByDescending(todo => todo.Name) 
                        : query.OrderBy(todo => todo.Name);
                    break;
                case TodoSortField.Description:
                    query = todoFilters.SortDescending 
                        ? query.OrderByDescending(todo => todo.Description) 
                        : query.OrderBy(todo => todo.Description);
                    break;
                case TodoSortField.Category:
                    query = todoFilters.SortDescending 
                        ? query.OrderByDescending(todo => todo.Category) 
                        : query.OrderBy(todo => todo.Category);
                    break;
                case TodoSortField.Status:
                    query = todoFilters.SortDescending 
                        ? query.OrderByDescending(todo => todo.Status) 
                        : query.OrderBy(todo => todo.Status);
                    break;
                case TodoSortField.DueDate:
                    query = todoFilters.SortDescending 
                        ? query.OrderByDescending(todo => todo.DueDate) 
                        : query.OrderBy(todo => todo.DueDate);
                    break;
                case TodoSortField.Priority:
                    query = todoFilters.SortDescending 
                        ? query.OrderByDescending(todo => todo.Priority) 
                        : query.OrderBy(todo => todo.Priority);
                    break;
                default:
                    query = query.OrderBy(todo => todo.Id);
                    break;
            }
        }
        else
        {
            // Default sort if no valid SortBy is provided.
            query = query.OrderByDescending(todo => todo.Name);
        }
        
        
        // Return all todos if GetAll is true
        return await query.ToListAsync();
    }

    public async Task<TodoResponse?> UpdateTodo(TodoUpdateRequest todoUpdateRequest)
    {
        var existingTodo = await _db.Todo.FirstOrDefaultAsync(t => t.Id == todoUpdateRequest.Id);

        if (existingTodo == null)
        {
            return null; // Return null if the todo doesn't exist
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
            return false;
        }

        existingTodo.IsDeleted = true;
        
        return await _db.SaveChangesAsync() > 0;

    }

    public async Task<bool> DestroyTodo(int? todoId)
    {
        var existingTodo = await _db.Todo.FirstOrDefaultAsync(t => t.Id == todoId);

        if (existingTodo == null)
        {
            return false;
        }

        _db.Todo.Remove(existingTodo); 
        return await _db.SaveChangesAsync() > 0; 
    }

}