using TodoRESTApi.identity.Enums;

namespace TodoRESTApi.ServiceContracts.Filters;

public class RoleFilters
{
    public RoleType RoleTypeToGet { get; set; }
    
    public Guid? MetaRoleId { get; set; }
    
    public Guid? RoleId { get; set; }
}