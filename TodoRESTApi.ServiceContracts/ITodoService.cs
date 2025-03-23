using TodoRESTApi.ServiceContracts.DTO.Request;
using TodoRESTApi.ServiceContracts.DTO.Response;
using TodoRESTApi.ServiceContracts.Filters;

namespace TodoRESTApi.ServiceContracts;

public interface ITodoService
{
    /// <summary>
    /// Adds a todo object to the list of Todos
    /// </summary>
    /// <param name="todoAddRequest">Todos object to add</param>
    /// <returns>Returns the Todos object after adding it</returns>
    Task<TodoResponse> AddTodo(TodoAddRequest? todoAddRequest);

    /// <summary>
    /// Get a todo object based on the given todo filter
    /// </summary>
    /// <param name="todoFilter">Todo filter to be used for filter todo</param>
    /// <returns>Returns the Todo object with the matching todo</returns>
    Task<List<TodoResponse>?> GetTodoByTodoIdWithFilter(TodoFilters todoFilter);

    /// <summary>
    /// Update the todo with new data
    /// </summary>
    /// <param name="todoUpdateRequest">The new value for Todo</param>
    /// <returns>Return the Updated Todo in TodoResponse</returns>
    Task<TodoResponse?> UpdateTodo(TodoUpdateRequest? todoUpdateRequest);

    /// <summary>
    /// Soft Delete todo 
    /// </summary>
    /// <param name="todoId">The Id of the todo to soft delete</param>
    /// <returns>Return if the soft-delete is successful</returns>
    Task<bool> SoftDeleteTodo(int? todoId);
    
    /// <summary>
    /// Destroy todo 
    /// </summary>
    /// <param name="todoId">The Id of the todo to destroy</param>
    /// <returns>Return if the action destroy is successful</returns>
    Task<bool> DestroyTodo(int? todoId);
}