using TodoRESTApi.identity.Identity;

namespace TodoRESTApi.RepositoryContracts;

public interface IMetaRoleRepository
{
    /// <summary>
    /// Add MetaRole
    /// </summary>
    /// <param name="metaRole">The MetaRole to add</param>
    /// <returns>The added MetaRole</returns>
    public Task<MetaRole?> AddMetaRole(MetaRole metaRole);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metaRoleId"></param>
    /// <returns></returns>
    public Task<MetaRole?> GetMetaRoleBasedOnId(Guid metaRoleId);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="metaRoleName"></param>
    /// <returns></returns>
    public Task<MetaRole?> GetMetaRoleBasedOnName(string metaRoleName);

}