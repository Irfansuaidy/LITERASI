using System.ComponentModel.DataAnnotations;
using Literasi.Models.Enums;

namespace Literasi.Models.ViewModels;

public class TeacherCreateViewModel
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    public Gender Gender { get; set; }

    [Required]
    public string Nip { get; set; } = string.Empty;

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;
}