using System.ComponentModel.DataAnnotations;

namespace Literasi.Models;

public class GradeLevel
{
    [Key]
    public int GradeLevelId { get; set; }

    [Required]
    [StringLength(10)]
    public string LevelName { get; set; } = string.Empty;

    // Navigation
    public ICollection<Class> Classes { get; set; }
        = new List<Class>();
}
