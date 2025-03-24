using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TodoRESTApi.Entities.Context;
using TodoRESTApi.Entities.Entities;
using TodoRESTApi.Repository;
using TodoRESTApi.RepositoryContracts;
using TodoRESTApi.Service;
using TodoRESTApi.ServiceContracts;
using TodoRESTApi.ServiceContracts.DTO.Request;
using TodoRESTApi.ServiceContracts.DTO.Response;
using TodoRESTApi.ServiceContracts.Filters;

namespace TodoRESTApi.Testing.ServicesTest;

public class TodoServiceTest
{
    private readonly ITodoService _todoService;
    private readonly ITodoRepository _todoRepository;

    private readonly List<TodoAddRequest> _todoAddRequests;
    private readonly List<TodoResponse> _todoResponses;

    private readonly IFixture _fixture;

    public TodoServiceTest()
    {
        // Set environment to Test
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase").Options;

        var dbContext = new TodoDbContext(options);
        _fixture = new Fixture();
        _todoRepository = new TodoRepository(dbContext);
        _todoService = new TodoService(_todoRepository);

        // Call the function and store the data on startup
        var task = CreateTodoDummyData();
        task.Wait();

        (_todoAddRequests, _todoResponses) = task.Result;
    }

    private async Task<(List<TodoAddRequest>, List<TodoResponse>)> CreateTodoDummyData()
    {
        List<TodoAddRequest> todoAddRequests = new List<TodoAddRequest>
        {
            _fixture.Build<TodoAddRequest>().Create(),
            _fixture.Build<TodoAddRequest>().Create(),
            _fixture.Build<TodoAddRequest>().Create()
        };

        List<TodoResponse> todoResponses = new List<TodoResponse>();

        foreach (var request in todoAddRequests)
        {
            TodoResponse response = await _todoService.AddTodo(request);
            todoResponses.Add(response);
        }

        return (todoAddRequests, todoResponses);
    }


    #region AddTodo

    //When we supply null value as TodoAddRequest. it should throw ArgumentNullException
    [Fact]
    public async Task AddTodo_NullTodo_ToBeArgumentNullException()
    {
        //Arrange
        TodoAddRequest? todoAddRequest = null;

        //Act
        Func<Task> action = async () => { await _todoService.AddTodo(todoAddRequest); };

        //Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    //When we supply null value in TodoAddRequest Name. it should throw ArgumentException
    [Fact]
    public async Task AddTodo_NullTodoName_ToBeArgumentException()
    {
        //Arrange Set Name as null
        TodoAddRequest? todoAddRequest =
            _fixture.Build<TodoAddRequest>().With(temp => temp.Name, null as string).Create();

        //Act
        Func<Task> action = async () => { await _todoService.AddTodo(todoAddRequest); };

        //Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    //When we supply proper TodoAddRequest. it should insert into the todo table successfully and return Todo Response 
    [Fact]
    public async Task AddTodo_FullTodoDetail_ToBeSuccessful()
    {
        //Arrange 
        TodoAddRequest? todoAddRequest =
            _fixture.Build<TodoAddRequest>().Create();

        Todo todo = todoAddRequest.ToTodo();
        TodoResponse todoResponseExpected = todo.ToTodoResponse();

        //Act
        TodoResponse todoResponseFromAdd = await _todoService.AddTodo(todoAddRequest);

        // When adding new todo the Id is not provided so we set the Id in expected to match the Todo response which contain Id generated from db
        todoResponseExpected.Id = todoResponseFromAdd.Id;

        //Assert
        todoResponseFromAdd.Id.Should().NotBe(null);

        todoResponseFromAdd.Should().BeEquivalentTo(todoResponseExpected);
    }

    #endregion

    #region GetTodoByWithFilter

    // if we supply Todo Id as null, it should return null as TodoResponse
    [Fact]
    public async Task FilterTodoByTodoId_NullTodoId_ToBeNull()
    {
        // Arrange 
        int? todoId = null;

        TodoFilters todoFilters = new TodoFilters()
        {
            TodoId = todoId
        };

        // Act
        List<TodoResponse>? todoResponse = await _todoService.GetTodoByTodoIdWithFilter(todoFilters);

        // Assert
        todoResponse.Should().BeEmpty();
    }

    // if we supply Todo Id, it should return matching TodoResponse
    [Fact]
    public async Task FilterTodoByTodoId_TodoIdProvided_ToBeSuccessfully()
    {
        // Arrange
        TodoAddRequest selectedTodo = _todoAddRequests[1];

        Todo todo = selectedTodo.ToTodo();
        TodoResponse todoResponseExpected = todo.ToTodoResponse();

        // Act

        // When adding new todo the Id is not provided so we set the Id in expected to match the Todo response which contain Id generated from db
        todoResponseExpected.Id = _todoResponses[1].Id;

        TodoFilters todoFilters = new TodoFilters()
        {
            TodoId = _todoResponses[1].Id
        };

        List<TodoResponse>? todoResponseFromSearch = await _todoService.GetTodoByTodoIdWithFilter(todoFilters);

        todoResponseFromSearch.Should().NotBeNull();
        todoResponseFromSearch.First().Should().BeEquivalentTo(todoResponseExpected);
    }

    // if we supply Todo Status, it should return matching TodoResponse
    [Fact]
    public async Task FilterTodoByTodoStatus_TodoStatusProvided_ToBeSuccessfully()
    {
        // Arrange
        TodoAddRequest selectedTodo = _todoAddRequests[1];

        Todo todo = selectedTodo.ToTodo();
        TodoResponse todoResponseExpected = todo.ToTodoResponse();

        // Act

        // When adding new todo the Id is not provided so we set the Id in expected to match the Todo response which contain Id generated from db
        todoResponseExpected.Id = _todoResponses[1].Id;

        TodoFilters todoFilters = new TodoFilters()
        {
            Status = _todoResponses[1].Status
        };

        List<TodoResponse>? todoResponseFromSearch = await _todoService.GetTodoByTodoIdWithFilter(todoFilters);

        todoResponseFromSearch.Should().NotBeNull();
        todoResponseFromSearch.First().Should().BeEquivalentTo(todoResponseExpected);
    }

    // if we supply Todo Priority, it should return matching TodoResponse
    [Fact]
    public async Task FilterTodoByTodoPriority_TodoPriorityProvided_ToBeSuccessfully()
    {
        // Arrange
        TodoAddRequest selectedTodo = _todoAddRequests[1];

        Todo todo = selectedTodo.ToTodo();
        TodoResponse todoResponseExpected = todo.ToTodoResponse();

        // Act

        // When adding new todo the Id is not provided so we set the Id in expected to match the Todo response which contain Id generated from db
        todoResponseExpected.Id = _todoResponses[1].Id;

        TodoFilters todoFilters = new TodoFilters()
        {
            Priority = _todoResponses[1].Priority
        };

        List<TodoResponse>? todoResponseFromSearch = await _todoService.GetTodoByTodoIdWithFilter(todoFilters);

        todoResponseFromSearch.Should().NotBeNull();
        todoResponseFromSearch.Should().ContainEquivalentOf(todoResponseExpected);
    }

    // if we supply Todo Categories, it should return matching TodoResponse
    [Fact]
    public async Task FilterTodoByTodoCategories_TodoCategoriesProvided_ToBeSuccessfully()
    {
        // Arrange
        TodoAddRequest selectedTodo = _todoAddRequests[1];

        Todo todo = selectedTodo.ToTodo();
        TodoResponse todoResponseExpected = todo.ToTodoResponse();

        // Act


        // When adding new todo the Id is not provided so we set the Id in expected to match the Todo response which contain Id generated from db
        todoResponseExpected.Id = _todoResponses[1].Id;

        TodoFilters todoFilters = new TodoFilters()
        {
            Category = _todoResponses[1].Category
        };

        List<TodoResponse>? todoResponseFromSearch = await _todoService.GetTodoByTodoIdWithFilter(todoFilters);

        todoResponseFromSearch.Should().NotBeNull();
        todoResponseFromSearch.Should().ContainEquivalentOf(todoResponseExpected);
    }

    // if we supply Todo Categories while having multiple Todo in the same Categories, it should return matching TodoResponse List
    [Fact]
    public async Task FilterTodoByTodoCategoriesWithMultiple_TodoCategoriesProvided_ToBeSuccessfully()
    {
        // Arrange
        TodoAddRequest todoAddRequest1 = _todoAddRequests[1];
        TodoResponse todoResponse1 = _todoResponses[1];

        TodoAddRequest todoAddRequest2 = _fixture.Build<TodoAddRequest>()
            .With(temp => temp.Category, todoAddRequest1.Category)
            .Create();

        Todo todo1 = todoAddRequest1.ToTodo();
        TodoResponse todoResponseExpected1 = todo1.ToTodoResponse();

        Todo todo2 = todoAddRequest2.ToTodo();
        TodoResponse todoResponseExpected2 = todo2.ToTodoResponse();

        // Act Add Both Todo so they can get two result 
        TodoResponse todoResponse2 = await _todoService.AddTodo(todoAddRequest2);

        // When adding new todo the Id is not provided so we set the Id in expected to match the Todo response which contain Id generated from db
        todoResponseExpected1.Id = todoResponse1.Id;
        todoResponseExpected2.Id = todoResponse2.Id;

        TodoFilters todoFilters = new TodoFilters()
        {
            Category = todoResponse2.Category
        };

        List<TodoResponse>? todoResponseFromSearch = await _todoService.GetTodoByTodoIdWithFilter(todoFilters);

        todoResponseFromSearch.Should().NotBeNull();
        todoResponseFromSearch.Should().ContainEquivalentOf(todoResponseExpected1);
        todoResponseFromSearch.Should().ContainEquivalentOf(todoResponseExpected2);
    }

    #endregion

    #region UpdateTodo

    //When we supply null value as TodoUpdateRequest. it should throw ArgumentNullException
    [Fact]
    public async Task UpdateTodo_NullTodoUpdateRequest_ToBeArgumentNullException()
    {
        //Arrange
        TodoUpdateRequest? todoUpdateRequest = null;

        //Act
        Func<Task> action = async () => { await _todoService.UpdateTodo(todoUpdateRequest); };

        //Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    // When we supply invalid PersonId as PersonUpdateRequest, it should return null
    [Fact]
    public async Task UpdatePerson_InvalidPersonId_ToBeNull()
    {
        // Arrange
        TodoUpdateRequest todoUpdateRequest = _fixture.Build<TodoUpdateRequest>().Create();

        // Act
        TodoResponse? todoResponse = await _todoService.UpdateTodo(todoUpdateRequest);

        // Assert
        todoResponse.Should().BeNull();
    }


    //When we supply value in TodoUpdateRequest. it should update to the todo table successfully and return Todo Response
    [Fact]
    public async Task UpdateTodo_ProperTodoUpdateRequest_ToBeSuccessfully()
    {
        //Arrange
        TodoResponse todoResponseToUpdate = _todoResponses[1];
        TodoUpdateRequest todoUpdateRequest = _fixture.Build<TodoUpdateRequest>()
            .With(temp => temp.Id, todoResponseToUpdate.Id).Create();

        //Act
        TodoResponse? todoResponseAfterUpdate = await _todoService.UpdateTodo(todoUpdateRequest);

        //Assert
        todoResponseAfterUpdate.Should().NotBeNull();
        todoResponseAfterUpdate.Should().BeEquivalentTo(todoUpdateRequest.ToTodo().ToTodoResponse());
    }

    #endregion

    #region DeleteTodo

    //When we supply null value as todoID. it should throw ArgumentNullException
    [Fact]
    public async Task SoftDeleteTodo_NullTodoId_ToBeArgumentNullException()
    {
        //Arrange
        int? todoId = null;

        //Act
        Func<Task> action = async () => { await _todoService.SoftDeleteTodo(todoId); };

        //Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }
    
    //When we supply invalid value as todoID. it should return false
    [Fact]
    public async Task SoftDeleteTodo_InvalidTodoId_ToBeFalse()
    {
        //Arrange
        int? todoId = -1;

        //Act
        bool todoResponse = await _todoService.SoftDeleteTodo(todoId);

        //Assert
        todoResponse.Should().BeFalse();
    }
    
    //When we supply valid value as todoID. it should successful soft-delete the todo
    [Fact]
    public async Task SoftDeleteTodo_ValidTodoId_ToBeSuccessful()
    {
        //Arrange
        TodoResponse todoResponse = _todoResponses[1];

        TodoFilters todoFilters = new TodoFilters()
        {
            TodoId = todoResponse.Id
        };
        
        //Act
        bool softDeleteTodoSuccessful = await _todoService.SoftDeleteTodo(todoResponse.Id);
        List<TodoResponse>? todoResponseFromDb = await _todoService.GetTodoByTodoIdWithFilter(todoFilters);

        //Assert
        todoResponseFromDb.Should().NotBeNull();
        softDeleteTodoSuccessful.Should().BeTrue();
        todoResponseFromDb.First().IsDeleted.Should().BeTrue();
    }
    
    //When we supply null value as todoID. it should throw ArgumentNullException
    [Fact]
    public async Task DestroyTodo_NullTodoId_ToBeArgumentNullException()
    {
        //Arrange
        int? todoId = null;

        //Act
        Func<Task> action = async () => { await _todoService.DestroyTodo(todoId); };

        //Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }
    
    //When we supply invalid value as todoID. it should return false
    [Fact]
    public async Task DestroyTodo_InvalidTodoId_ToBeFalse()
    {
        //Arrange
        int? todoId = -1;

        //Act
        bool todoResponse = await _todoService.DestroyTodo(todoId);

        //Assert
        todoResponse.Should().BeFalse();
    }
    
    //When we supply valid value as todoID. it should successful destroy the todo
    [Fact]
    public async Task DestroyTodo_ValidTodoId_ToBeSuccessful()
    {
        //Arrange
        TodoResponse todoResponse = _todoResponses[1];

        TodoFilters todoFilters = new TodoFilters()
        {
            TodoId = todoResponse.Id
        };
        
        //Act
        bool destroyTodoSuccessful = await _todoService.DestroyTodo(todoResponse.Id);
        List<TodoResponse>? todoResponseFromDb = await _todoService.GetTodoByTodoIdWithFilter(todoFilters);

        //Assert
        todoResponseFromDb.Should().BeEmpty();
        destroyTodoSuccessful.Should().BeTrue();
    }

    #endregion
}