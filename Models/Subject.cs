using System.ComponentModel.DataAnnotations;

namespace Literasi.Models;

public class Subject
{
    [Key]
    public int SubjectId { get; set; }

    [Required]
    [StringLength(20)]
    public string SubjectCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string SubjectName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    // Navigation
    public ICollection<TeachingAssignment> TeachingAssignments { get; set; }
        = new List<TeachingAssignment>();
}
