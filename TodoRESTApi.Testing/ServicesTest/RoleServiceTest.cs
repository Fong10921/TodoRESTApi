using System.Security.Claims;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoRESTApi.Entities.Context;
using TodoRESTApi.identity.DTO;
using TodoRESTApi.identity.Enums;
using TodoRESTApi.identity.Identity;
using TodoRESTApi.Repository;
using TodoRESTApi.RepositoryContracts;
using TodoRESTApi.Service;
using TodoRESTApi.ServiceContracts;
using TodoRESTApi.ServiceContracts.DTO.Response;
using TodoRESTApi.ServiceContracts.Filters;

namespace TodoRESTApi.Testing.ServicesTest;

public class RoleServiceTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly IRoleService _roleService;
    private readonly IRoleRepository _roleRepository;
    private readonly IMetaRoleRepository _metaRoleRepository;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserRepository _userRepository;
    private readonly SignInRepository _signInRepository;
    private readonly ILogger<RoleService> _logger;

    private readonly TodoDbContext _dbContext;
    private readonly HttpClient _client;

    private readonly IFixture _fixture;

    public RoleServiceTest(CustomWebApplicationFactory factory)
    {
        // Set environment to Test
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

        // Create scope from the factory
        var scope = factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        _client = factory.CreateClient();
        _logger = serviceProvider.GetRequiredService<ILogger<RoleService>>();


        _dbContext = serviceProvider.GetRequiredService<TodoDbContext>();
        _signInManager = serviceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        _userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        _roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        _fixture = new Fixture();

        _metaRoleRepository = new MetaRoleRepository(_dbContext);
        _roleRepository = new RoleRepository(_userManager, _roleManager, _signInManager, _dbContext);
        _userRepository =
            new UserRepository(_userManager, _roleManager, _signInManager, _dbContext, _metaRoleRepository);
        _signInRepository = new SignInRepository(_userManager, _roleManager, _signInManager);
        _roleService =
            new RoleService(_logger, _metaRoleRepository, _userRepository, _roleRepository, _signInRepository);
    }

    private async Task<(List<ApplicationUser>, List<IdentityResult>)> CreateUserDummyData()
    {
        List<ApplicationUser> applicationUserRequest = new List<ApplicationUser>
        {
            _fixture.Build<ApplicationUser>().Without(temp => temp.MetaRole).Without(temp => temp.MetaRoleId).Create(),
            _fixture.Build<ApplicationUser>().Without(temp => temp.MetaRole).Without(temp => temp.MetaRoleId).Create(),
            _fixture.Build<ApplicationUser>().Without(temp => temp.MetaRole).Without(temp => temp.MetaRoleId).Create()
        };

        List<IdentityResult> identityResults = new List<IdentityResult>();

        foreach (var request in applicationUserRequest)
        {
            IdentityResult identity = await _userManager.CreateAsync(request);
            identityResults.Add(identity);
        }

        return (applicationUserRequest, identityResults);
    }

    private async Task<(List<CreateRoleDto>, List<RoleServiceResponse>)> CreateRoleDummyData()
    {
        List<CreateRoleDto> createRoleDto = new List<CreateRoleDto>
        {
            _fixture.Build<CreateRoleDto>().With(temp => temp.RoleType, RoleType.PageLevel).Create(),
            _fixture.Build<CreateRoleDto>().With(temp => temp.RoleType, RoleType.PageLevel).Create(),
            _fixture.Build<CreateRoleDto>().With(temp => temp.RoleType, RoleType.MetaLevel).Create()
        };
        
        List<RoleServiceResponse> roleServiceResponses = new List<RoleServiceResponse>();

        foreach (var request in createRoleDto)
        {
            await _roleService.CreateRoleAsync(request);
            if (request.RoleType != RoleType.MetaLevel)
            {
                ApplicationRole role = (await _roleManager.FindByNameAsync(request.RoleName))!;
                var roleFilter = new RoleFilters()
                {
                    RoleId = role.Id,
                    RoleTypeToGet = RoleType.PageLevel
                };
                roleServiceResponses.Add(await _roleService.GetAllRole(roleFilter));
            } else if (request.RoleType == RoleType.MetaLevel)
            {
                MetaRole? metaRole =  await _roleService.FindMetaRoleBasedOnMetaRoleName(request.RoleName);
                var roleFilter = new RoleFilters()
                {
                    MetaRoleId = metaRole?.Id,
                    RoleTypeToGet = RoleType.MetaLevel
                };
                roleServiceResponses.Add(await _roleService.GetAllRole(roleFilter));
            }
        }

        return (createRoleDto, roleServiceResponses);
    }

    #region AddRole

    //When we supply null value in createRoleDto. it should throw ArgumentNullException
    [Fact]
    public async Task AddRole_NullCreateRoleDto_ToBeArgumentNullException()
    {
        //Arrange
        CreateRoleDto? createRoleDto = null;

        //Act
        Func<Task> action = async () => { await _roleService.CreateRoleAsync(createRoleDto!); };

        //Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    //When we supply null value in createRoleDto Role Name. it should throw ArgumentException
    [Fact]
    public async Task AddRole_CreateRoleDtoRoleNameNull_ToBeArgumentException()
    {
        //Arrange
        CreateRoleDto? createRoleDto =
            _fixture.Build<CreateRoleDto>().With(temp => temp.RoleName, null as string).Create();

        //Act
        Func<Task> action = async () => { await _roleService.CreateRoleAsync(createRoleDto); };

        //Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    //When we supply give all create role dto in page level. it should be able to create Role with 5 claims
    [Fact]
    public async Task AddRole_WithFullPageLevelDetails_ShouldCreateRoleWithFiveClaims()
    {
        //Arrange
        CreateRoleDto? createRoleDto =
            _fixture.Build<CreateRoleDto>().With(temp => temp.RoleType, RoleType.PageLevel).Create();

        //Act
        IdentityResult roleAddedIdentityResult = await _roleService.CreateRoleAsync(createRoleDto);
        ApplicationRole? role = await _roleManager.FindByNameAsync(createRoleDto.RoleName);

        RoleFilters roleFilters = new RoleFilters()
        {
            RoleId = role!.Id,
            RoleTypeToGet = RoleType.PageLevel
        };

        RoleServiceResponse? roleWithClaims = await _roleService.GetAllRole(roleFilters);

        //Assert
        roleAddedIdentityResult.Succeeded.Should().BeTrue();
        role.Should().NotBeNull();
        role!.Name.Should().Be(createRoleDto.RoleName);
        role!.RoleType.Should().Be(createRoleDto.RoleType);
        roleWithClaims!.RoleResponses.Count().Should().Be(1);
        roleWithClaims!.RoleResponses[0].RoleName.Should().Be(createRoleDto.RoleName);
        roleWithClaims!.RoleResponses[0].RoleType.Should().Be(createRoleDto.RoleType);
        roleWithClaims!.RoleResponses[0].Claims!.Count.Should().Be(5);
        roleWithClaims!.RoleResponses[0].Claims.Should().ContainSingle(c =>
            c.ClaimType == $"Permission:{createRoleDto.RoleName}" && c.ClaimValue == "CanView"
        );
        roleWithClaims!.RoleResponses[0].Claims.Should().ContainSingle(c =>
            c.ClaimType == $"Permission:{createRoleDto.RoleName}" && c.ClaimValue == "CanCreate"
        );
        roleWithClaims!.RoleResponses[0].Claims.Should().ContainSingle(c =>
            c.ClaimType == $"Permission:{createRoleDto.RoleName}" && c.ClaimValue == "CanEdit"
        );
        roleWithClaims!.RoleResponses[0].Claims.Should().ContainSingle(c =>
            c.ClaimType == $"Permission:{createRoleDto.RoleName}" && c.ClaimValue == "CanDelete"
        );
        roleWithClaims!.RoleResponses[0].Claims.Should().ContainSingle(c =>
            c.ClaimType == $"Permission:{createRoleDto.RoleName}" && c.ClaimValue == "CanExport"
        );
    }

    #endregion

    #region AssignRoleToUser

    //When we supply null value in assignRoleToUserDto. it should throw ArgumentNullException
    [Fact]
    public async Task AssignRoleToUser_NullAssignRoleToUserDto_ToBeArgumentNullException()
    {
        //Arrange
        AssignRoleToUserDto? assignRoleToUserDto = null;

        //Act
        Func<Task> action = async () => { await _roleService.AssignRoleToUser(assignRoleToUserDto!); };

        //Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    //When we supply null value in AssignRoleToUser UserId Null. it should throw ArgumentException
    [Fact]
    public async Task AssignRoleToUser_AssignRoleToUserUserIdNull_ToBeArgumentException()
    {
        //Arrange
        AssignRoleToUserDto? assignRoleToUserDto =
            _fixture.Build<AssignRoleToUserDto>().With(temp => temp.RoleId, null as string).Create();

        //Act
        Func<Task> action = async () => { await _roleService.AssignRoleToUser(assignRoleToUserDto); };

        //Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    //When we supply full detail in AssignRoleToUser With Normal Role. it should correctly assign the role to user
    [Fact]
    public async Task AssignRoleToUser_AssignRoleToUserFullDetailNormalRole_ToBeSuccessful()
    {
        //Arrange
        var (users, identityResultsUser) = await CreateUserDummyData();
        var (roles, applicationRoles) = await CreateRoleDummyData();

        ApplicationUser selectedApplicationUser = users[0];
        RoleResponse selectedApplicationRole = applicationRoles[0].RoleResponses[0];
        
        AssignRoleToUserDto assignRoleToUserDto = new AssignRoleToUserDto
        {
            UserId = selectedApplicationUser.Id.ToString(),
            RoleId = selectedApplicationRole.RoleId.ToString(),
        };
        
        //Act
        IdentityResult assignRoleIdentityResult = await _roleService.AssignRoleToUser(assignRoleToUserDto);
        ApplicationUser applicationUser = (await _userManager.FindByNameAsync(selectedApplicationUser.UserName!))!;
        IList<string> applicationRoleNames = await _userManager.GetRolesAsync(applicationUser);
        ApplicationRole applicationRole = (await _roleManager.FindByNameAsync(applicationRoleNames[0]))!;

        //Assert
        assignRoleIdentityResult.Succeeded.Should().BeTrue();
        applicationRole.Id.Should().Be(selectedApplicationRole.RoleId);
        applicationRole.RoleType.Should().Be(selectedApplicationRole.RoleType);
        applicationRole.Name.Should().Be(selectedApplicationRole.RoleName);
        applicationRole.NormalizedName.Should().Be(selectedApplicationRole.NormalizedName);
    }
    
    //When we supply full detail in AssignRoleToUser With Meta Role. it should correctly assign the role to user
    [Fact]
    public async Task AssignRoleToUser_AssignRoleToUserFullDetailMetaRole_ToBeSuccessful()
    {
        //Arrange
        var (users, identityResultsUser) = await CreateUserDummyData();
        var (rolesCreateRoleDto, applicationRoles) = await CreateRoleDummyData();

        ApplicationUser selectedApplicationUser = users[0];
        RoleResponse selectedApplicationRole = applicationRoles.SelectMany(temp => temp.RoleResponses).FirstOrDefault(temp => temp.RoleType == RoleType.MetaLevel)!;
        
        AssignRoleToUserDto assignRoleToUserDto = new AssignRoleToUserDto
        {
            UserId = selectedApplicationUser.Id.ToString(),
            RoleId = selectedApplicationRole.RoleId.ToString(),
        };
        
        //Act
        IdentityResult assignRoleIdentityResult = await _roleService.AssignRoleToUser(assignRoleToUserDto);
        ApplicationUser applicationUser = (await _userManager.FindByNameAsync(selectedApplicationUser.UserName!))!;

        RoleFilters roleFilter = new RoleFilters()
        {
            MetaRoleId = applicationUser.MetaRoleId,
            RoleTypeToGet = RoleType.MetaLevel,
        };
        RoleServiceResponse roleServiceResponse = await _roleService.GetAllRole(roleFilter);
        
        RoleResponse metaRole = roleServiceResponse.RoleResponses[0];

        //Assert
        assignRoleIdentityResult.Succeeded.Should().BeTrue();
        roleServiceResponse.RoleResponses.Count().Should().Be(1);
        metaRole.RoleId.Should().Be(selectedApplicationRole.RoleId);
        metaRole.RoleName.Should().Be(selectedApplicationRole.RoleName);
        metaRole.RoleType.Should().Be(selectedApplicationRole.RoleType);
    }
    
    //When we supply full detail in AssignRoleToUser but role id is wrong. it should failed assign the role to user
    [Fact]
    public async Task AssignRoleToUser_AssignRoleToUserWrongMetaRoleId_ToBeIdentityResultFalse()
    {
        //Arrange
        var (users, identityResultsUser) = await CreateUserDummyData();

        ApplicationUser selectedApplicationUser = users[0];
        
        AssignRoleToUserDto assignRoleToUserDto = new AssignRoleToUserDto
        {
            UserId = selectedApplicationUser.Id.ToString(),
            RoleId = "ABC"
        };
        
        //Act
        IdentityResult assignRoleIdentityResult = await _roleService.AssignRoleToUser(assignRoleToUserDto);
        

        //Assert
        assignRoleIdentityResult.Succeeded.Should().BeFalse();
    }
    
    //When we supply full detail in AssignRoleToUser but user id is wrong. it should failed assign the role to user
    [Fact]
    public async Task AssignRoleToUser_AssignRoleToUserWrongUserId_ToBeIdentityResultFalse()
    {
        //Arrange
        var (rolesCreateRoleDto, applicationRoles) = await CreateRoleDummyData();
        
        RoleResponse selectedApplicationRole = applicationRoles.SelectMany(temp => temp.RoleResponses).FirstOrDefault(temp => temp.RoleType == RoleType.MetaLevel)!;
        
        AssignRoleToUserDto assignRoleToUserDto = new AssignRoleToUserDto
        {
            UserId = "ABC",
            RoleId = selectedApplicationRole.RoleId.ToString(),
        };
        
        //Act
        IdentityResult assignRoleIdentityResult = await _roleService.AssignRoleToUser(assignRoleToUserDto);

        //Assert
        assignRoleIdentityResult.Succeeded.Should().BeFalse();

    }

    #endregion

    #region RefreshUser

    //When we supply null value in RefreshUser Claims Principal. it should throw ArgumentException
    [Fact]
    public async Task RefreshUser_WithNullClaimsPrincipal_ToBeArgumentException()
    {
        //Arrange
        ClaimsPrincipal? claimsPrincipal = null;

        //Act
        Func<Task> action = async () => { await _roleService.RefreshUser(claimsPrincipal!); };
        
        //Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }
    
    // When the user in the claims principal does not exist or lacks the required role, the method should return a failed IdentityResult.
    [Fact]
    public async Task RefreshUser_WithInvalidUserId_ToBeIdentityResultError()
    {
        //Arrange
        var (applicationUsers, identityResults) = await CreateUserDummyData();
        var (rolesCreateRoleDto, applicationRoles) = await CreateRoleDummyData();

        ApplicationUser applicationUser = applicationUsers[0];
        RoleResponse roleResponse = applicationRoles.SelectMany(temp => temp.RoleResponses)
            .FirstOrDefault(temp => temp.RoleType == RoleType.PageLevel)!;

        AssignRoleToUserDto assignRoleToUserDto = new AssignRoleToUserDto()
        {
            UserId = "Abc",
            RoleId = roleResponse.RoleId.ToString(),
        };
        
        //Act
        await _roleService.AssignRoleToUser(assignRoleToUserDto);
        
        var identity = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, "Abc")
        }, "Test");

        var principal = new ClaimsPrincipal(identity);
        
        IdentityResult identityResult =  await _roleService.RefreshUser(principal!); 
        
        //Assert
        identityResult.Succeeded.Should().BeFalse();
    }
    
    // When the claims principal user doesn't exist, the method should return a failed IdentityResult.
    [Fact]
    public async Task RefreshUser_WithInvalidClaims_ToBeIdentityResultError()
    {
        //Arrange
        var (applicationUsers, identityResults) = await CreateUserDummyData();
        var (rolesCreateRoleDto, applicationRoles) = await CreateRoleDummyData();

        ApplicationUser applicationUser = applicationUsers[0];
        RoleResponse roleResponse = applicationRoles.SelectMany(temp => temp.RoleResponses)
            .FirstOrDefault(temp => temp.RoleType == RoleType.PageLevel)!;

        AssignRoleToUserDto assignRoleToUserDto = new AssignRoleToUserDto()
        {
            UserId = applicationUser.Id.ToString(),
            RoleId = roleResponse.RoleId.ToString(),
        };
        
        //Act
        await _roleService.AssignRoleToUser(assignRoleToUserDto);
        
        var identity = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, "Abc")
        }, "Test");

        var principal = new ClaimsPrincipal(identity);
        
        IdentityResult identityResult =  await _roleService.RefreshUser(principal!); 
        
        //Assert
        identityResult.Succeeded.Should().BeFalse();
    }
    
    // When the claims principal user exist and is assign in role, and user id is in claims principle , the method should return a Succeed IdentityResult.
    [Fact]
    public async Task RefreshUser_WithValidClaims_ToBeIdentityResultSucceed()
    {
        //Arrange
        var (applicationUsers, identityResults) = await CreateUserDummyData();
        var (rolesCreateRoleDto, applicationRoles) = await CreateRoleDummyData();

        ApplicationUser applicationUser = applicationUsers[0];
        RoleResponse roleResponse = applicationRoles.SelectMany(temp => temp.RoleResponses)
            .FirstOrDefault(temp => temp.RoleType == RoleType.PageLevel)!;

        AssignRoleToUserDto assignRoleToUserDto = new AssignRoleToUserDto()
        {
            UserId = applicationUser.Id.ToString(),
            RoleId = roleResponse.RoleId.ToString(),
        };
        
        //Act
        await _roleService.AssignRoleToUser(assignRoleToUserDto);
        
        var identity = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, applicationUser.Id.ToString())
        }, "Test");

        var principal = new ClaimsPrincipal(identity);
        
        IdentityResult identityResult =  await _roleService.RefreshUser(principal!); 
        
        //Assert
        identityResult.Succeeded.Should().BeTrue();
    }

    
    

    #endregion

    #region AssignClaimToRole

    

    #endregion
}