using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TodoRESTApi.identity.DTO;
using TodoRESTApi.identity.Enums;
using TodoRESTApi.identity.Identity;
using TodoRESTApi.RepositoryContracts;
using TodoRESTApi.Service.Helpers;
using TodoRESTApi.ServiceContracts;
using TodoRESTApi.ServiceContracts.DTO.Response;
using TodoRESTApi.ServiceContracts.Filters;

namespace TodoRESTApi.Service;

public class RoleService : IRoleService
{
    private readonly IMetaRoleRepository _metaRoleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ISignInRepository _signInRepository;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        ILogger<RoleService> logger, IMetaRoleRepository metaRoleRepository, IUserRepository userRepository,
        IRoleRepository roleRepository, ISignInRepository signInRepository)
    {
        _logger = logger;
        _metaRoleRepository = metaRoleRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _signInRepository = signInRepository;
    }

    public async Task<IdentityResult> CreateRoleAsync(CreateRoleDto createRoleDto)
    {
        // Validate input
        if (createRoleDto == null)
        {
            throw new ArgumentNullException(nameof(createRoleDto), "Role data cannot be null.");
        }
        
        ValidationHelper.ModelValidation(createRoleDto);

        try
        {
            ApplicationRole role = new ApplicationRole()
            {
                Name = createRoleDto.RoleName,
                RoleType = createRoleDto.RoleType
            };
            
            if (createRoleDto.RoleType != RoleType.MetaLevel)
            {
                IdentityResult result = await _roleRepository.CreateRole(role);
                
                if (!result.Succeeded)
                {
                    return result; // Return the failure result with errors
                }
            }
            
            if (createRoleDto.RoleType == RoleType.PageLevel)
            {
                var claims = new List<Claim>
                {
                    new($"Permission:{createRoleDto.RoleName}", "CanView"),
                    new($"Permission:{createRoleDto.RoleName}", "CanCreate"),
                    new($"Permission:{createRoleDto.RoleName}", "CanEdit"),
                    new($"Permission:{createRoleDto.RoleName}", "CanDelete"),
                    new($"Permission:{createRoleDto.RoleName}", "CanExport")
                };

                foreach (var claim in claims)
                {
                    var claimResult = await _roleRepository.AddClaimAsync(role, claim);
                    if (!claimResult.Succeeded)
                    {
                        return claimResult; // Return the failure if adding claims fails
                    }
                }
            }
            else if (createRoleDto.RoleType == RoleType.MetaLevel)
            {
                MetaRole metaRole = createRoleDto.ToMetaRole();

                MetaRole? resultAddingMetaRole = await _metaRoleRepository.AddMetaRole(metaRole);

                if (resultAddingMetaRole == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Failed to create meta role." });
                }

                return IdentityResult.Success; // Successfully created a meta role
            }

            return IdentityResult.Success; // Return success if everything goes well
        }
        catch (Exception ex)
        { 
            Console.WriteLine($"Error creating role: {ex.Message}");

            // Return a failure result with exception message
            return IdentityResult.Failed(new IdentityError
                { Description = "An unexpected error occurred while creating the role." });
        }
    }

    public async Task<IdentityResult> AssignRoleToUser(AssignRoleToUserDto assignRoleToUserDto)
    {
        // Validate input
        if (assignRoleToUserDto == null)
        {
            throw new ArgumentNullException(nameof(assignRoleToUserDto), "Assign Role To User DTO data cannot be null.");
        }
        
        ValidationHelper.ModelValidation(assignRoleToUserDto);

        try
        {
            var user = await _userRepository.FindUserByIdAsync(assignRoleToUserDto.UserId);
        
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                    { Description = $"User with ID '{assignRoleToUserDto.UserId}' not found." });
            }

            var role = await _roleRepository.FindRoleByIdAsync(assignRoleToUserDto.RoleId);

            if (role == null)
            {
                MetaRole? metaRole = await _metaRoleRepository.GetMetaRoleBasedOnId(Guid.Parse(assignRoleToUserDto.RoleId));

                if (metaRole == null)
                {
                    return IdentityResult.Failed(new IdentityError
                        { Description = "Failed to convert CreateRoleDto to MetaRole." });
                }

                user.MetaRoleId = metaRole.Id;

                IdentityResult? resultUpdateUser = await _userRepository.UpdateUserAsync(user);

                if (resultUpdateUser == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Failed to assign meta role to user." });
                }
            }
            else
            {
                // Check if user is already in the role
                if (role.Name != null && await _userRepository.IsUserInRoleAsync(user, role.Name))
                {
                    return IdentityResult.Failed(new IdentityError
                        { Description = $"User is already in the role '{role.Name}'." });
                }

                // Assign the role
                if (role.Name != null)
                {
                    var result = await _userRepository.AddUserToRoleAsync(user, role.Name);
                    if (!result.Succeeded)
                    {
                        return IdentityResult.Failed(result.Errors.ToArray());
                    }
                }
            }

            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error assigning role to user: {ex.Message}");

            // Return a failure result with exception message
            return IdentityResult.Failed(new IdentityError
                { Description = "An unexpected error occurred while assigning role to user." });
        }
    }

    public async Task<IdentityResult> RefreshUser(ClaimsPrincipal applicationUser)
    {
        // Validate input
        if (applicationUser == null)
        {
            throw new ArgumentNullException(nameof(applicationUser), "Refresh User ClaimsPrincipal cannot be null.");
        }
        
        try
        {
            // Get user from claims
            var user = await _userRepository.GetUserUsingClaimPrinciple(applicationUser);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            // Update security stamp
            var securityStampResult = await _userRepository.UpdateSecurityStampAsync(user);
            if (!securityStampResult.Succeeded)
            {
                return IdentityResult.Failed(securityStampResult.Errors.ToArray());
            }

            // Refresh sign-in
            await _signInRepository.RefreshSignInUser(user);

            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while refreshing user.");

            return IdentityResult.Failed(new IdentityError { Description = $"An error occurred: {ex.Message}" });
        }
    }

    public async Task<IdentityResult> AssignClaimToRole(AssignClaimToRoleDtoRequest assignClaimToRoleDtoRequest)
    {
        try
        {
            var errors = new List<IdentityError>();

            Guid metaRoleId = Guid.Empty;
            foreach (var rolePermissionClaimDto in assignClaimToRoleDtoRequest.RolePermissionClaimDtos)
            {
                var role = await _roleRepository.FindRoleByNameAsync(rolePermissionClaimDto.RoleName);
                if (role == null)
                {
                    errors.Add(new IdentityError
                        { Description = $"Role '{rolePermissionClaimDto.RoleName}' not found." });
                    continue;
                }

                // Try parsing MetaRole ID
                if (assignClaimToRoleDtoRequest.MetaRoleToUpdate != null)
                {
                    if (Guid.TryParse(assignClaimToRoleDtoRequest.MetaRoleToUpdate.ToString(), out var parsedGuid))
                    {
                        metaRoleId = parsedGuid;
                    }
                }
                
                var metaRoleToUpdate = await _metaRoleRepository.GetMetaRoleBasedOnId(metaRoleId);
                var existingClaims = await _roleRepository.GetRoleClaims(role);

                var permissions = new Dictionary<string, bool>
                {
                    { "CanView", rolePermissionClaimDto.CanView },
                    { "CanCreate", rolePermissionClaimDto.CanCreate },
                    { "CanEdit", rolePermissionClaimDto.CanEdit },
                    { "CanDelete", rolePermissionClaimDto.CanDelete },
                    { "CanExport", rolePermissionClaimDto.CanExport }
                };

                foreach (var permission in permissions)
                {
                    var claimType = $"Permission:{role.Name}";

                    if (metaRoleToUpdate != null)
                    {
                        var roleClaims = await _roleRepository.FindClaimBasedOnRole(role);
                        if (roleClaims == null || !roleClaims.Any()) 
                        {
                            return IdentityResult.Failed(new IdentityError
                            { 
                                Description = $"Role '{role.Name}' has no claims." 
                            });
                        }
                        
                        var claim = roleClaims.FirstOrDefault(c => c.ClaimType == claimType && c.ClaimValue == permission.Key);

                        // if you want to add the claim
                        if (permission.Value && claim != null)
                        {
                            var existingMetaRoleBasedOnId = await _roleRepository.FindMetaRoleClaimsBasedOnId(metaRoleToUpdate.Id, claimType, permission.Key);

                            if (existingMetaRoleBasedOnId == null)
                            {
                                await _roleRepository.AddMetaRoleClaim(claim.Id, metaRoleToUpdate.Id);
                            }
                            
                        }
                        // if you want to remove the claim
                        else if (!permission.Value && claim != null)
                        {
                            var existingMetaRoleClaimsPivotBasedOnId = await _roleRepository.FindMetaRoleClaimsBasedOnId(metaRoleToUpdate.Id, claimType, permission.Key);

                            if (existingMetaRoleClaimsPivotBasedOnId != null)
                            {
                                await _roleRepository.DestroyMetaRolePivot(existingMetaRoleClaimsPivotBasedOnId.Id);
                            }
                        }
                    }
                    else
                    {
                        var claim = new Claim(claimType, permission.Key);
                        bool claimExists = existingClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value);

                        IdentityResult result;
                        if (permission.Value && !claimExists)
                        {
                            result = await _roleRepository.AddClaimAsync(role, claim);
                        }
                        else if (!permission.Value && claimExists)
                        {
                            result = await _roleRepository.RemoveClaimsFromRole(role, claim);
                        }
                        else
                        {
                            continue;
                        }

                        if (!result.Succeeded)
                        {
                            errors.Add(new IdentityError
                                { Description = $"Failed to modify claim '{permission.Key}' for role '{role.Name}'." });
                        }
                    }
                }
            }

            return errors.Any() ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning claims to role.");
            return IdentityResult.Failed(new IdentityError
                { Description = "An unexpected error occurred while assigning claims to the role." });
        }
    }

    public async Task<bool> UserHasMetaClaims(ClaimsPrincipal user, string claimType, string claimValue)
    {
        ApplicationUser? applicationUser = await _userRepository.GetUserUsingClaimPrinciple(user);

        if (applicationUser is { MetaRoleId: null })
        {
            return false;
        }

        if (applicationUser != null)
        {
            MetaRole? metaRole = await _metaRoleRepository.GetMetaRoleBasedOnId((Guid)applicationUser.MetaRoleId);

            if (metaRole is not null)
            {
                return _roleRepository.UserHasMetaClaims(applicationUser, claimType, claimValue, metaRole);
            }
        }

        return false;
    }

    public async Task<RoleServiceResponse> GetAllRole(RoleFilters roleFilter)
    {
        List<RoleResponse>? roleAndClaims = await _roleRepository.GetPageRoleIncludingMetaRoleAndClaims(roleFilter);

        if (roleAndClaims is not null)
        {
            return roleAndClaims.ToRoleServiceResponse();
        }
        else
        {
            return new RoleServiceResponse()
            {
                RoleResponses = new List<RoleResponse>()
            };
        }
    }

    public async Task<MetaRole?> FindMetaRoleBasedOnMetaRoleName(string metaRoleName)
    {
        MetaRole? metaRole = await _metaRoleRepository.GetMetaRoleBasedOnName(metaRoleName);

        return metaRole;
    }
}