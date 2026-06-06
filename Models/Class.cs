using System.ComponentModel.DataAnnotations;

namespace Literasi.Models;

public class Class
{
    [Key]
    public int ClassId { get; set; }

    [Required]
    public int AcademicYearId { get; set; }

    [Required]
    public int GradeLevelId { get; set; }

    public int? HomeroomTeacherId { get; set; }

    [Required]
    [StringLength(20)]
    public string ClassName { get; set; } = string.Empty;

    // Navigation
    public AcademicYear AcademicYear { get; set; } = null!;

    public GradeLevel GradeLevel { get; set; } = null!;

    public Teacher? HomeroomTeacher { get; set; }

    public ICollection<Student> Students { get; set; }
        = new List<Student>();

    public ICollection<TeachingAssignment> TeachingAssignments { get; set; }
        = new List<TeachingAssignment>();
}
