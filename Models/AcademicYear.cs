using System.ComponentModel.DataAnnotations;

namespace Literasi.Models;

public class AcademicYear
{
    [Key]
    public int AcademicYearId { get; set; }

    [StringLength(20)]
    public string YearName { get; set; } = string.Empty;

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    public bool IsActive { get; set; }

    public ICollection<Class> Classes { get; set; }
        = new List<Class>();

    public bool IsValidPeriod()
    {
        return EndDate > StartDate;
    }
}