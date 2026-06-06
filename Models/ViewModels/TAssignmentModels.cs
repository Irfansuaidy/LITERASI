using System.ComponentModel.DataAnnotations;

namespace Literasi.Models.ViewModels;

public class TeachingAssignmentCreateViewModel
{
    [Required]
    public int TeacherId { get; set; }

    [Required]
    public int SubjectId { get; set; }

    [Required]
    public int ClassId { get; set; }
}