using System.ComponentModel.DataAnnotations;

namespace Literasi.Models;

public class TeachingAssignment
{
    [Key]
    public int TeachingAssignmentId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    [Required]
    public int SubjectId { get; set; }

    [Required]
    public int ClassId { get; set; }

    // Navigation
    public Teacher Teacher { get; set; } = null!;

    public Subject Subject { get; set; } = null!;

    public Class Class { get; set; } = null!;

    public ICollection<LearningMaterial> LearningMaterials { get; set; }
        = new List<LearningMaterial>();

    public ICollection<Assignment> Assignments { get; set; }
        = new List<Assignment>();
}