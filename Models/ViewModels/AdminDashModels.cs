using System.ComponentModel.DataAnnotations;

public class AdminDashboardViewModel
{
    public int TotalTeachers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalClasses { get; set; }
    public int TotalSubjects { get; set; }
    public int TotalTeachingAssignments { get; set; }

    public string? ActiveAcademicYear { get; set; }
}