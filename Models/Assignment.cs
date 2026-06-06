using System.ComponentModel.DataAnnotations;

namespace Literasi.Models;

public class Assignment
{
    [Key]
    public int AssignmentId { get; set; }

    [Required]
    public int TeachingAssignmentId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DateTime Deadline { get; set; }

    [Range(0,100)]
    public decimal MaxScore { get; set; } = 100;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public TeachingAssignment TeachingAssignment { get; set; } = null!;

    public ICollection<Submission> Submissions { get; set; }
        = new List<Submission>();
}

