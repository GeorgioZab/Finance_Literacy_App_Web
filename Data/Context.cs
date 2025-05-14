using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Finance_Literacy_App_Web.Models;

namespace Finance_Literacy_App_Web.Data
{
    public class Context : IdentityDbContext<User>
    {
        public Context(DbContextOptions options) : base(options) { }

        public DbSet<Module> Modules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Models.Task> Tasks { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupLessonDeadline> GroupLessonDeadlines { get; set; }
        public DbSet<UserLessonStatus> UserLessonStatuses { get; set; }

        public DbSet<UserTaskAnswer> UserTaskAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка отношений
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Module)
                .WithMany(m => m.Lessons)
                .HasForeignKey(l => l.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Models.Task>()
                .HasOne(t => t.Lesson)
                .WithMany(l => l.Tasks)
                .HasForeignKey(t => t.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasDefaultValue("user");

            modelBuilder.Entity<User>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Group)
                .WithMany(g => g.Users)
                .HasForeignKey(u => u.GroupId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Group>()
                .HasIndex(g => g.Name)
                .IsUnique();

            modelBuilder.Entity<GroupLessonDeadline>()
                .HasKey(gld => new { gld.GroupId, gld.LessonId })
                .HasName("PK_GroupLessonDeadline");

            modelBuilder.Entity<GroupLessonDeadline>()
                .HasOne(gld => gld.Group)
                .WithMany()
                .HasForeignKey(gld => gld.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupLessonDeadline>()
                .HasOne(gld => gld.Lesson)
                .WithMany()
                .HasForeignKey(gld => gld.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserLessonStatus>()
                .HasKey(uls => new { uls.UserId, uls.LessonId })
                .HasName("PK_UserLessonStatus");

            modelBuilder.Entity<UserLessonStatus>()
                .Property(uls => uls.Status)
                .IsRequired()
                .HasDefaultValue("NotStarted");

            modelBuilder.Entity<UserLessonStatus>()
                .HasOne(uls => uls.User)
                .WithMany()
                .HasForeignKey(uls => uls.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserLessonStatus>()
                .HasOne(uls => uls.Lesson)
                .WithMany()
                .HasForeignKey(uls => uls.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserTaskAnswer>()
            .HasOne(uta => uta.User)
            .WithMany()
            .HasForeignKey(uta => uta.UserId);

            modelBuilder.Entity<UserTaskAnswer>()
                .HasOne(uta => uta.Task)
                .WithMany()
                .HasForeignKey(uta => uta.TaskId);
        }
    }
}