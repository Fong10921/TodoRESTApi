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
    
    public async Task<MetaRole> AddMetaRole(MetaRole metaRole)
    {
        _db.MetaRoles.Add(metaRole);
        await _db.SaveChangesAsync();

        return metaRole;
    }
}