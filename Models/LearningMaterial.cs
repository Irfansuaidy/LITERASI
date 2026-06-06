using System.ComponentModel.DataAnnotations;
using Literasi.Models.Enums;

namespace Literasi.Models;

public class LearningMaterial
{
    [Key]
    public int MaterialId { get; set; }

    [Required]
    public int TeachingAssignmentId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public MaterialType MaterialType { get; set; }

    [StringLength(500)]
    public string? FilePath { get; set; }

    [StringLength(500)]
    public string? ExternalUrl { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public TeachingAssignment TeachingAssignment { get; set; } = null!;
}
