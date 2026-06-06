using Literasi.Models;
using Literasi.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Literasi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ==========================
    // DbSets
    // ==========================

    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Student> Students { get; set; }

    public DbSet<AcademicYear> AcademicYears { get; set; }
    public DbSet<GradeLevel> GradeLevels { get; set; }
    public DbSet<Class> Classes { get; set; }

    public DbSet<Subject> Subjects { get; set; }
    public DbSet<TeachingAssignment> TeachingAssignments { get; set; }

    public DbSet<LearningMaterial> LearningMaterials { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<Grade> Grades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureProperties(modelBuilder);
        ConfigureRelationships(modelBuilder);
        ConfigureIndexes(modelBuilder);
        SeedData(modelBuilder);
    }

    private static void ConfigureProperties(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(u => u.Gender)
            .HasConversion<string>()
            .HasMaxLength(1);
    }

    private static void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        // ==========================
        // User & Role
        // ==========================

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Teacher)
            .WithOne(t => t.User)
            .HasForeignKey<Teacher>(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Student)
            .WithOne(s => s.User)
            .HasForeignKey<Student>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==========================
        // Academic Structure
        // ==========================

        modelBuilder.Entity<Class>()
            .HasOne(c => c.AcademicYear)
            .WithMany(a => a.Classes)
            .HasForeignKey(c => c.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Class>()
            .HasOne(c => c.GradeLevel)
            .WithMany(g => g.Classes)
            .HasForeignKey(c => c.GradeLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Student>()
            .HasOne(s => s.Class)
            .WithMany(c => c.Students)
            .HasForeignKey(s => s.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Class>()
            .HasOne(c => c.HomeroomTeacher)
            .WithMany(t => t.HomeroomClasses)
            .HasForeignKey(c => c.HomeroomTeacherId)
            .OnDelete(DeleteBehavior.SetNull);

        // ==========================
        // Teaching Assignment
        // ==========================

        modelBuilder.Entity<TeachingAssignment>()
            .HasOne(ta => ta.Teacher)
            .WithMany(t => t.TeachingAssignments)
            .HasForeignKey(ta => ta.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TeachingAssignment>()
            .HasOne(ta => ta.Subject)
            .WithMany(s => s.TeachingAssignments)
            .HasForeignKey(ta => ta.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TeachingAssignment>()
            .HasOne(ta => ta.Class)
            .WithMany(c => c.TeachingAssignments)
            .HasForeignKey(ta => ta.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        // ==========================
        // Learning Materials
        // ==========================

        modelBuilder.Entity<LearningMaterial>()
            .HasOne(m => m.TeachingAssignment)
            .WithMany(t => t.LearningMaterials)
            .HasForeignKey(m => m.TeachingAssignmentId);

        // ==========================
        // Assignments
        // ==========================

        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.TeachingAssignment)
            .WithMany(t => t.Assignments)
            .HasForeignKey(a => a.TeachingAssignmentId);

        // ==========================
        // Submissions
        // ==========================

        modelBuilder.Entity<Submission>()
            .HasOne(s => s.Assignment)
            .WithMany(a => a.Submissions)
            .HasForeignKey(s => s.AssignmentId);

        modelBuilder.Entity<Submission>()
            .HasOne(s => s.Student)
            .WithMany(st => st.Submissions)
            .HasForeignKey(s => s.StudentId);

        // ==========================
        // Grades
        // ==========================

        modelBuilder.Entity<Grade>()
            .HasOne(g => g.Submission)
            .WithOne(s => s.Grade)
            .HasForeignKey<Grade>(g => g.SubmissionId);

        modelBuilder.Entity<Grade>()
            .HasOne(g => g.GradedByTeacher)
            .WithMany()
            .HasForeignKey(g => g.GradedByTeacherId);
    }

    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Teacher>()
            .HasIndex(t => t.Nip)
            .IsUnique();

        modelBuilder.Entity<Student>()
            .HasIndex(s => s.Nisn)
            .IsUnique();

        modelBuilder.Entity<TeachingAssignment>()
            .HasIndex(ta => new
            {
                ta.TeacherId,
                ta.SubjectId,
                ta.ClassId
            })
            .IsUnique();

        modelBuilder.Entity<Submission>()
            .HasIndex(s => new
            {
                s.AssignmentId,
                s.StudentId
            })
            .IsUnique();
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                RoleId = 1,
                RoleName = "Admin"
            },
            new Role
            {
                RoleId = 2,
                RoleName = "Guru"
            },
            new Role
            {
                RoleId = 3,
                RoleName = "Siswa"
            }
        );

        modelBuilder.Entity<GradeLevel>().HasData(
            new GradeLevel
            {
                GradeLevelId = 1,
                LevelName = "X"
            },
            new GradeLevel
            {
                GradeLevelId = 2,
                LevelName = "XI"
            },
            new GradeLevel
            {
                GradeLevelId = 3,
                LevelName = "XII"
            }
        );

        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = 1,
                RoleId = 1,
                Username = "admin",
                PasswordHash = "$2a$12$0Yq0DlcmARr..onAnn.T9O2MZL3RXdOXp53c3rn.ACTNVwjOQ28Mq",
                FullName = "Administrator",
                Gender = Gender.L,
                IsActive = true
            }
        );
    }
}
