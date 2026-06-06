using System.ComponentModel.DataAnnotations;

using Literasi.Models.Enums;
namespace Literasi.Models;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    public int RoleId { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public Gender Gender { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public Role Role { get; set; } = null!;

    public Teacher? Teacher { get; set; }

    public Student? Student { get; set; }
}
