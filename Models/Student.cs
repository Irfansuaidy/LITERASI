using System.ComponentModel.DataAnnotations;

namespace Literasi.Models;

public class Student
{
    [Key]
    public int StudentId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int ClassId { get; set; }

    [Required]
    [StringLength(30)]
    public string Nisn { get; set; } = string.Empty;

    public DateOnly? BirthDate { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Aktif";

    // Navigation Properties
    public User User { get; set; } = null!;

    public Class Class { get; set; } = null!;

    public ICollection<Submission> Submissions { get; set; }
        = new List<Submission>();
}
