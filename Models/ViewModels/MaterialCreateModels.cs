using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class MaterialCreateViewModel
{
    [Required]
    public int TeachingAssignmentId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public string MaterialType { get; set; } = string.Empty;

    public IFormFile? File { get; set; }

    public string? ExternalLink { get; set; }
}