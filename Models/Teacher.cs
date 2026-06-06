using System.ComponentModel.DataAnnotations;

namespace Literasi.Models;

public class Teacher
{
    [Key]
    public int TeacherId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(30)]
    public string Nip { get; set; } = string.Empty;

    // Navigation Properties
    public User User { get; set; } = null!;

    public ICollection<TeachingAssignment> TeachingAssignments { get; set; }
        = new List<TeachingAssignment>();

    public ICollection<Class> HomeroomClasses { get; set; }
        = new List<Class>();
}
