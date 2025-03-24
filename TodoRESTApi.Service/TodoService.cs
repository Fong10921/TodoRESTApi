using TodoRESTApi.Entities.Entities;
using TodoRESTApi.RepositoryContracts;
using TodoRESTApi.Service.Helpers;
using TodoRESTApi.ServiceContracts;
using TodoRESTApi.ServiceContracts.DTO.Request;
using TodoRESTApi.ServiceContracts.DTO.Response;
using TodoRESTApi.ServiceContracts.Filters;

namespace TodoRESTApi.Service;

public class TodoService: ITodoService
{
    private readonly ITodoRepository _todoRepository;
    public TodoService(ITodoRepository todoRepository)
    {
        _todoRepository = todoRepository;
    }
    
    public async Task<TodoResponse> AddTodo(TodoAddRequest? todoAddRequest)
    {
        if (todoAddRequest == null)
        {
            throw new ArgumentNullException(nameof(todoAddRequest));
        }
        
        // Model Validation
        ValidationHelper.ModelValidation(todoAddRequest);

        Todo todo = todoAddRequest.ToTodo();

        await _todoRepository.AddTodo(todo);

        return todo.ToTodoResponse();
    }

    public async Task<List<TodoResponse>?> GetTodoByTodoIdWithFilter(TodoFilters? todoFilter)
    {
        if (todoFilter == null)
        {
            throw new ArgumentNullException(nameof(todoFilter));
        }
        
        List<Todo>? todos = await _todoRepository.GetTodosBasedOnFilters(todoFilter);
        
        return todos.Select(temp => temp.ToTodoResponse()).ToList();
    }

    public async Task<TodoResponse?> UpdateTodo(TodoUpdateRequest? todoUpdateRequest)
    {
        if (todoUpdateRequest == null)
        {
            throw new ArgumentNullException(nameof(todoUpdateRequest));
        }
        
        // Model Validation
        ValidationHelper.ModelValidation(todoUpdateRequest);
        
        TodoResponse? todoResponse = await _todoRepository.UpdateTodo(todoUpdateRequest);

        if (todoResponse == null)
        {
            return null;
        }

        return todoResponse;
    }

    public async Task<bool> SoftDeleteTodo(int? todoId)
    {
        if (todoId == null)
        {
            throw new ArgumentNullException(nameof(todoId));
        }
        
        return await _todoRepository.SoftDeleteTodo(todoId.Value);
    }

    public async Task<bool> DestroyTodo(int? todoId)
    {
        if (todoId == null)
        {
            throw new ArgumentNullException(nameof(todoId));
        }
        
        return await _todoRepository.DestroyTodo(todoId.Value);
    }
}