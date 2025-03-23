using TodoRESTApi.Entities.Entities;
using TodoRESTApi.ServiceContracts.DTO.Request;
using TodoRESTApi.ServiceContracts.DTO.Response;
using TodoRESTApi.ServiceContracts.Filters;

namespace TodoRESTApi.RepositoryContracts;

public interface ITodoRepository
{
    /// <summary>
    /// Adds a todo object to the data store
    /// </summary>
    /// <param name="todo">Todo object to add</param>
    /// <returns>Returns the todo after adding it to the db</returns>
    Task<Todo> AddTodo(Todo todo);

    /// <summary>
    /// Returns a todo object based on the given todo filters class
    /// </summary>
    /// <param name="todoFilters">Todo filters for the db</param>
    /// <returns>A List of Todo object or null if there is no record</returns>
    Task<List<Todo>> GetTodosBasedOnFilters(TodoFilters todoFilters);

    /// <summary>
    /// Updates a todo object in the data store
    /// </summary>
    /// <param name="todoUpdateRequest">The new value for Todo</param>
    /// <returns>Return the Updated Todo in TodoResponse</returns>
    Task<TodoResponse?> UpdateTodo(TodoUpdateRequest todoUpdateRequest);

    /// <summary>
    /// Soft deletes a todo object in the data store
    /// </summary>
    /// <param name="todoId">The id to delete</param>
    /// <returns>Return if the soft-delete is successful</returns>
    Task<bool> SoftDeleteTodo(int todoId);
    
    /// <summary>
    /// Destroy todo 
    /// </summary>
    /// <param name="todoId">The Id of the todo to destroy</param>
    /// <returns>Return if the action destroy is successful</returns>
    Task<bool> DestroyTodo(int? todoId);
}