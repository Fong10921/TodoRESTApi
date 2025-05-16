using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TodoRESTApi.Entities.Context;
using TodoRESTApi.identity.Enums;
using TodoRESTApi.identity.Identity;
using TodoRESTApi.RepositoryContracts;
using TodoRESTApi.ServiceContracts.DTO.Response;
using TodoRESTApi.ServiceContracts.Filters;

namespace TodoRESTApi.Repository;

public class RoleRepository : IRoleRepository
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    private readonly TodoDbContext _db;

    public RoleRepository(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
        SignInManager<ApplicationUser> signInManager, TodoDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _db = db;
    }

    public async Task<List<RoleResponse>?> GetPageRoleIncludingMetaRoleAndClaims(RoleFilters roleFilters)
    {
        List<RoleResponse> primeRoleWithClaim;

        IQueryable<ApplicationRole> rolesQuery = _db.Roles.Where(temp => temp.RoleType == RoleType.PageLevel);
        var claims = await _db.RoleClaims.ToListAsync();

        if (roleFilters.RoleId is not null)
        {
            rolesQuery = rolesQuery.Where(temp => temp.Id == roleFilters.RoleId);
        }

        List<ApplicationRole> roles = await rolesQuery.ToListAsync();

        var rolesWithClaims = roles.Select(role => new RoleResponse
        {
            RoleName = role.Name ?? string.Empty,
            RoleType = role.RoleType,
            RoleId = role.Id,
            NormalizedName = role.NormalizedName,
            Claims = claims
                .Where(c => c.RoleId == role.Id)
                .Select(c => new RoleClaimResponse
                {
                    ClaimId = c.Id,
                    ClaimType = c.ClaimType ?? string.Empty,
                    ClaimValue = c.ClaimValue ?? string.Empty
                }).ToList()
        }).ToList();

        IQueryable<MetaRole> metaRoleWithClaims = _db.MetaRoles;

        if (roleFilters.MetaRoleId != Guid.Empty)
        {
            metaRoleWithClaims = metaRoleWithClaims.Where(temp => temp.Id == roleFilters.MetaRoleId);
        }
        
        await metaRoleWithClaims.Include(metaRole => metaRole.MetaRoleClaimsPivots)
            .ThenInclude(p => p.IdentityRoleClaim)
            .ToListAsync();
        
        primeRoleWithClaim = rolesWithClaims.Select(roleWithClaim => new RoleResponse
        {
            RoleName = roleWithClaim.RoleName,
            RoleId = roleWithClaim.RoleId,
            NormalizedName = roleWithClaim.NormalizedName,
            RoleType = roleWithClaim.RoleType,
            Claims = roleWithClaim.Claims.Select(roleClaim => new RoleClaimResponse
            {
                ClaimId = roleClaim.ClaimId,
                ClaimType = roleClaim.ClaimType,
                ClaimValue = roleClaim.ClaimValue
            }).ToList()
        }).ToList();

        if (roleFilters.RoleTypeToGet != RoleType.MetaLevel)
        {
            return primeRoleWithClaim;
        }
        else if (roleFilters.RoleTypeToGet == RoleType.MetaLevel)
        {
            return metaRoleWithClaims.Select(metaRoleWithClaim => new RoleResponse()
            {
                RoleName = metaRoleWithClaim.MetaRoleName,
                RoleId = metaRoleWithClaim.Id,
                RoleType = RoleType.MetaLevel,
                MetaClaims = metaRoleWithClaim.MetaRoleClaimsPivots.Select(temp => new MetaRoleClaimResponse()
                {
                    MetaRoleClaimId = temp.IdentityRoleClaim.Id,
                    ClaimType = temp.IdentityRoleClaim.ClaimType ?? string.Empty,
                    ClaimValue = temp.IdentityRoleClaim.ClaimValue ?? string.Empty
                }).ToList(),
                PrimeRoleWithClaim = primeRoleWithClaim
            }).ToList();
        }
        else
        {
            return null;
        }
    }

    public async Task<IdentityResult> CreateRole(ApplicationRole applicationRole)
    {
        return await _roleManager.CreateAsync(applicationRole);
    }

    public async Task<IdentityResult> AddClaimAsync(ApplicationRole applicationRole, Claim claim)
    {
        return await _roleManager.AddClaimAsync(applicationRole, claim);
    }

    public async Task<ApplicationRole?> FindRoleByIdAsync(string roleId)
    {
        return await _roleManager.FindByIdAsync(roleId);
    }

    public async Task<ApplicationRole?> FindRoleByNameAsync(string roleName)
    {
        return await _roleManager.FindByNameAsync(roleName);
    }

    public async Task<List<Claim>> GetRoleClaims(ApplicationRole applicationRole)
    {
        return (await _roleManager.GetClaimsAsync(applicationRole)).ToList();
    }

    public async Task<IdentityResult> RemoveClaimsFromRole(ApplicationRole applicationRole, Claim claims)
    {
        return await _roleManager.RemoveClaimAsync(applicationRole, claims);
    }

    public async Task<List<IdentityRoleClaim<Guid>>?> FindClaimBasedOnRole(ApplicationRole applicationRole)
    {
        return await _db.IdentityRoleClaims
            .Where(temp => temp.RoleId == applicationRole.Id)
            .ToListAsync();
    }

    public async Task AddMetaRoleClaim(int claimsId, Guid metaRoleId)
    {
        var metaRolePivot = new MetaRoleClaimsPivot
        {
            RoleClaimId = claimsId,
            MetaRoleId = metaRoleId
        };

        _db.MetaRoleClaimsPivots.Add(metaRolePivot);
        await _db.SaveChangesAsync();
    }

    public async Task<MetaRoleClaimsPivot?> FindMetaRoleClaimsBasedOnId(Guid metaRoleId, string claimType,
        string claimValue)
    {
        return await _db.MetaRoleClaimsPivots
            .Where(temp => temp.MetaRoleId == metaRoleId && temp.IdentityRoleClaim.ClaimType == claimType &&
                           temp.IdentityRoleClaim.ClaimValue == claimValue).FirstOrDefaultAsync();
    }

    public async Task<bool> DestroyMetaRolePivot(Guid metaRoleClaimId)
    {
        var metaRoleClaimsPivot =
            await _db.MetaRoleClaimsPivots.FirstOrDefaultAsync(temp => temp.Id == metaRoleClaimId);

        if (metaRoleClaimsPivot == null)
        {
            return false;
        }

        _db.MetaRoleClaimsPivots.Remove(metaRoleClaimsPivot);
        await _db.SaveChangesAsync();

        return true;
    }

    public bool UserHasMetaClaims(ApplicationUser applicationUser, string claimType, string claimValue,
        MetaRole metaRole)
    {
        List<MetaRoleClaimsPivot> metaRoleClaimsPivots =
            metaRole.MetaRoleClaimsPivots.Where(temp => temp.MetaRoleId == metaRole.Id).ToList();

        foreach (var metaRoleClaimsPivot in metaRoleClaimsPivots)
        {
            if (metaRoleClaimsPivot.IdentityRoleClaim.ClaimType == claimType &&
                metaRoleClaimsPivot.IdentityRoleClaim.ClaimValue == claimValue)
            {
                return true;
            }
        }

        return false;
    }
}