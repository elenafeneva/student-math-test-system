using MathTaskValidator.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MathTaskValidator.Infrastructure
{
    public class AppDbContext : DbContext
    {
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

        public DbSet<Teacher> Teachers { get; set; } = null!;
        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Exam> Exams { get; set; } = null!;
        public DbSet<ExamTask> ExamTasks { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Set up the Relationship
            base.OnModelCreating(modelBuilder);

            // Configuration for Teacher
            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.ToTable("Teachers");
                entity.HasMany(t => t.Students);
                // ExternalId must be provided for teachers and must be unique
                entity.Property(t => t.ExternalId).IsRequired();
                entity.HasIndex(t => t.ExternalId).IsUnique();
                entity.Property(t => t.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("Students");
                // ExternalId must be provided for students and must be unique
                entity.Property(s => s.ExternalId).IsRequired();
                entity.HasIndex(s => s.ExternalId).IsUnique();
                entity.HasOne<Teacher>()
                    .WithMany(t => t.Students)
                    .HasForeignKey("TeacherId")
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(s => s.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Exam>(entity =>
            {
                entity.ToTable("Exams");
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Exams)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ExamTask>(entity =>
            {
                entity.ToTable("ExamTasks");
                entity.HasOne(t => t.Exam)
                    .WithMany(e => e.Tasks)
                    .HasForeignKey(t => t.ExamId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(t => t.Id).ValueGeneratedOnAdd();
            });

        }
    }
}
