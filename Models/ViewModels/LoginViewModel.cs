using System.ComponentModel.DataAnnotations;

namespace Literasi.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Username wajib diisi")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

}

