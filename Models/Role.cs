using System.ComponentModel.DataAnnotations;

namespace Literasi.Models;

public class Role
{
    [Key]
    public int RoleId { get; set; }

    [Required]
    [StringLength(50)]
    public string RoleName { get; set; } = string.Empty;

    // Navigation Property
    public ICollection<User> Users { get; set; } = new List<User>();
}
