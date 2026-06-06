using System.ComponentModel.DataAnnotations;
using Literasi.Models.Enums;
namespace Literasi.Models.ViewModels;

public class StudentCreateViewModel
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Nisn { get; set; } = string.Empty;

    [Required]
    public Gender Gender { get; set; }

    [Required]
    public int ClassId { get; set; }
}