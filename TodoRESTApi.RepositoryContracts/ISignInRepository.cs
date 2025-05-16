using Microsoft.AspNetCore.Identity;
using TodoRESTApi.identity.Identity;

namespace TodoRESTApi.RepositoryContracts;

public interface ISignInRepository
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationUser"></param>
    /// <returns></returns>
    public Task RefreshSignInUser(ApplicationUser applicationUser);
}