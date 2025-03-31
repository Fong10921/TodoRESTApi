using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TodoRESTApi.identity.DTO;
using TodoRESTApi.identity.Enums;
using TodoRESTApi.identity.Identity;
using TodoRESTApi.identity.ServiceContracts;
using TodoRESTApi.RepositoryContracts;

namespace TodoRESTApi.Service;

public class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMetaRoleRepository _metaRoleRepository;
    private readonly ILogger<RoleService> _logger;

    public RoleService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, ILogger<RoleService> logger, IMetaRoleRepository metaRoleRepository)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _metaRoleRepository = metaRoleRepository;
    }

    public async Task<IdentityResult> CreateRoleAsync(CreateRoleDto createRoleDto)
    {
        // Validate input
        if (createRoleDto == null)
        {
            throw new ArgumentNullException(nameof(createRoleDto), "Role data cannot be null.");
        }

        try
        {
            ApplicationRole role = new ApplicationRole()
            {
                Name = createRoleDto.RoleName,
                RoleType = createRoleDto.RoleType
            };

            IdentityResult result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                return result; // Return the failure result with errors
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
                    var claimResult = await _roleManager.AddClaimAsync(role, claim);
                    if (!claimResult.Succeeded)
                    {
                        return claimResult; // Return the failure if adding claims fails
                    }
                }
            }  else if (createRoleDto.RoleType == RoleType.MetaLevel)
            {
                MetaRole metaRole = createRoleDto.ToMetaRole();

                if (metaRole == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Failed to convert CreateRoleDto to MetaRole." });
                }

                MetaRole resultAddingMetaRole = await _metaRoleRepository.AddMetaRole(metaRole);

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
            // Log the exception if you have a logging framework (e.g., Serilog, ILogger)
            Console.WriteLine($"Error creating role: {ex.Message}");

            // Return a failure result with exception message
            return IdentityResult.Failed(new IdentityError
                { Description = "An unexpected error occurred while creating the role." });
        }
    }
    public async Task<IdentityResult> AssignRoleToUser(AssignRoleToUserDto assignRoleToUserDto)
    {
        var user = await _userManager.FindByIdAsync(assignRoleToUserDto.UserId);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError
                { Description = $"User with ID '{assignRoleToUserDto.UserId}' not found." });
        }

        var role = await _roleManager.FindByIdAsync(assignRoleToUserDto.RoleId);
        if (role == null)
        {
            return IdentityResult.Failed(new IdentityError
                { Description = $"Role with ID '{assignRoleToUserDto.RoleId}' not found." });
        }

        // Check if user is already in the role
        if (role.Name != null && await _userManager.IsInRoleAsync(user, role.Name))
        {
            return IdentityResult.Failed(new IdentityError
                { Description = $"User is already in the role '{role.Name}'." });
        }

        // Assign the role
        if (role.Name != null)
        {
            var result = await _userManager.AddToRoleAsync(user, role.Name);
            if (!result.Succeeded)
            {
                return IdentityResult.Failed(result.Errors.ToArray());
            }
        }

        return IdentityResult.Success;
    }
    public async Task<IdentityResult> RefreshUser(ClaimsPrincipal applicationUser)
    {
        try
        {
            // Get user from claims
            var user = await _userManager.GetUserAsync(applicationUser);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            // Update security stamp
            var securityStampResult = await _userManager.UpdateSecurityStampAsync(user);
            if (!securityStampResult.Succeeded)
            {
                return IdentityResult.Failed(securityStampResult.Errors.ToArray());
            }

            // Refresh sign-in
            await _signInManager.RefreshSignInAsync(user);

            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while refreshing user.");

            return IdentityResult.Failed(new IdentityError { Description = $"An error occurred: {ex.Message}" });
        }
    }
    public async Task<IdentityResult> AssignClaimToRole(AssignClaimToRoleDto assignClaimToRoleDto)
    {
        if (assignClaimToRoleDto?.RolePermissionClaimDtos == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "Invalid request data." });
        }

        try
        {
            List<IdentityError> errors = new List<IdentityError>();

            foreach (var rolePermissionClaimDto in assignClaimToRoleDto.RolePermissionClaimDtos)
            {
                ApplicationRole? role = await _roleManager.FindByNameAsync(rolePermissionClaimDto.RoleName);
                if (role == null)
                {
                    errors.Add(new IdentityError
                        { Description = $"Role '{rolePermissionClaimDto.RoleName}' not found." });
                    continue;
                }

                // Retrieve existing claims for the role
                var existingClaims = await _roleManager.GetClaimsAsync(role);

                // Dictionary to map permission names to their respective boolean properties
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
                    var claim = new Claim($"Permission:{role.Name}", permission.Key);
                    bool claimExists = existingClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value);

                    if (permission.Value)
                    {
                        // Add claim if it does not exist
                        if (!claimExists)
                        {
                            var result = await _roleManager.AddClaimAsync(role, claim);
                            if (!result.Succeeded)
                            {
                                errors.Add(new IdentityError
                                {
                                    Description = $"Failed to assign '{permission.Key}' claim to role '{role.Name}'."
                                });
                            }
                        }
                    }
                    else
                    {
                        // Remove claim if it exists
                        if (claimExists)
                        {
                            var result = await _roleManager.RemoveClaimAsync(role, claim);
                            if (!result.Succeeded)
                            {
                                errors.Add(new IdentityError
                                {
                                    Description = $"Failed to remove '{permission.Key}' claim from role '{role.Name}'."
                                });
                            }
                        }
                    }
                }
            }

            return errors.Count() > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error assigning claims to role: {ex.Message}");
            return IdentityResult.Failed(new IdentityError
                { Description = "An unexpected error occurred while assigning claims to the role." });
        }
    }
}