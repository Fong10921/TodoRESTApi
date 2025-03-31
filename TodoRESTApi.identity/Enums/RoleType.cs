using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TodoRESTApi.identity.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))] 
public enum RoleType
{
    [Display(Name = "Administrator")]
    AdministratorConst,
    [Display(Name = "Page Level")]
    PageLevel,
    [Display(Name = "Community Level")]
    CommunityLevel,
    [Display(Name = "Meta Level")]
    MetaLevel,
}