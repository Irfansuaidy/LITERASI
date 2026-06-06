using System.ComponentModel.DataAnnotations;

namespace Literasi.Models;

public class Grade
{
    [Key]
    public int GradeId { get; set; }

    [Required]
    public int SubmissionId { get; set; }

    [Required]
    public int GradedByTeacherId { get; set; }

    [Range(0,100)]
    public decimal Score { get; set; }

    [StringLength(1000)]
    public string? Feedback { get; set; }

    public DateTime GradedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Submission Submission { get; set; } = null!;

    public Teacher GradedByTeacher { get; set; } = null!;
}
