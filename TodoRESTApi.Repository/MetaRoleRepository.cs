using Microsoft.EntityFrameworkCore;
using TodoRESTApi.Entities.Context;
using TodoRESTApi.identity.Identity;
using TodoRESTApi.RepositoryContracts;

namespace TodoRESTApi.Repository;

public class MetaRoleRepository: IMetaRoleRepository
{
    private readonly TodoDbContext _db;
    
    public MetaRoleRepository(TodoDbContext db)
    {
        _db = db;
    }
    
    public async Task<MetaRole?> AddMetaRole(MetaRole metaRole)
    {
        _db.MetaRoles.Add(metaRole);
        await _db.SaveChangesAsync();

        return metaRole;
    }

    public async Task<MetaRole?> GetMetaRoleBasedOnId(Guid metaRoleId)
    {
        return await _db.MetaRoles
            .Include(m => m.MetaRoleClaimsPivots)
            .ThenInclude(p => p.IdentityRoleClaim)
            .FirstOrDefaultAsync(m => m.Id == metaRoleId);;
    }

    public async Task<MetaRole?> GetMetaRoleBasedOnName(string metaRoleName)
    {
        return await _db.MetaRoles
            .Include(m => m.MetaRoleClaimsPivots)
            .ThenInclude(p => p.IdentityRoleClaim)
            .FirstOrDefaultAsync(m => m.MetaRoleName == metaRoleName);;
    }
}