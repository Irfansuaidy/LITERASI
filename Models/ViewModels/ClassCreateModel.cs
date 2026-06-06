using System.ComponentModel.DataAnnotations;

namespace Literasi.Models.ClassCreateModels;

public class ClassCreateModels
{
    [Required]
    public string ClassName { get; set; } = string.Empty;

    [Required]
    public int AcademicYearId { get; set; }

    [Required]
    public int GradeLevelId { get; set; }

    public int? HomeroomTeacherId { get; set; }
}