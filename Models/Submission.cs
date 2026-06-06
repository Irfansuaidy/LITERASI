using System.ComponentModel.DataAnnotations;
using Literasi.Models.Enums;

namespace Literasi.Models;

public class Submission
{
    [Key]
    public int SubmissionId { get; set; }

    [Required]
    public int AssignmentId { get; set; }

    [Required]
    public int StudentId { get; set; }

    [Required]
    public SubmissionType SubmissionType { get; set; }

    [StringLength(500)]
    public string? FilePath { get; set; }

    [StringLength(500)]
    public string? ExternalUrl { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public SubmissionStatus Status { get; set; }
        = SubmissionStatus.Submitted;

    // Navigation
    public Assignment Assignment { get; set; } = null!;

    public Student Student { get; set; } = null!;

    public Grade? Grade { get; set; }
}
