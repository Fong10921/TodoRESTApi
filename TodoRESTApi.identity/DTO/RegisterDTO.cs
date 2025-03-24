using System.ComponentModel.DataAnnotations;

namespace TodoRESTApi.identity.DTO;

public class RegisterDTO
{
    [Required(ErrorMessage = "Person Name can't be blank")]
    public string PersonName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email can't be blank")]
    [EmailAddress(ErrorMessage = "Email should be in a proper email address format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number can't be blank")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "Email should be in a proper phone number format")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password can't be blank")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Confirm Password can't be blank")]
    [Compare("Password", ErrorMessage = "Password and confirm password need to match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}