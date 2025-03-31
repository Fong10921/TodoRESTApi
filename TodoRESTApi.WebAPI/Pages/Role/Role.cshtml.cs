using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoRESTApi.identity.DTO;

namespace TodoRESTApi.WebAPI.Pages.Role;

public class RoleModel : PageModel
{
    public IEnumerable<RolePermissionClaimDto> PagePermissionViewModels = new List<RolePermissionClaimDto>();
    
    public void OnGet()
    {
        
    }

    public void OnPost()
    {
        
    }
}