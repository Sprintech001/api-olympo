using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using olympo_webapi.Models;
using olympo_webapi.Services;

namespace olympo_webapi.Infrastructure
{
    public class ConnectionContext : DbContext
    {
        public DbSet<Gym> Gyms { get; set; }
        public DbSet<GymUser> GymUsers { get; set; }
        public DbSet<UserExercise> UserExercises { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<User> Users { get; set; }

        public ConnectionContext(DbContextOptions<ConnectionContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .EnableSensitiveDataLogging() 
                    .LogTo(Console.WriteLine, 
                           new[] { RelationalEventId.CommandExecuted }, 
                           LogLevel.Information);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");

            modelBuilder.Entity<Gym>(entity =>
            {
                entity.Property(g => g.Id).ValueGeneratedOnAdd();
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
                entity.Property(g => g.Address).IsRequired().HasMaxLength(200);
                entity.Property(g => g.PhoneNumber).IsRequired().HasMaxLength(15);
                entity.Property(g => g.Email).IsRequired().HasMaxLength(100);
                entity.Property(g => g.Website).IsRequired(false).HasMaxLength(100);
                entity.Property(g => g.Description).IsRequired(false).HasMaxLength(500);
                entity.Property(g => g.ImageUrl).IsRequired(false).HasMaxLength(200);
            });

            modelBuilder.Entity<GymUser>(entity =>
            {
                entity.HasKey(gu => new { gu.UserId, gu.GymId });

                entity.HasOne(gu => gu.User)
                    .WithMany(u => u.Gyms)
                    .HasForeignKey(gu => gu.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gu => gu.Gym)
                    .WithMany(g => g.GymUsers)
                    .HasForeignKey(gu => gu.GymId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserExercise>(entity =>
            {
                entity.HasKey(ue => new { ue.UserId, ue.ExerciseId });

                entity.HasOne(ue => ue.User)
                    .WithMany(u => u.Exercises)
                    .HasForeignKey(ue => ue.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ue => ue.Exercise)
                    .WithMany(e => e.Users)
                    .HasForeignKey(ue => ue.ExerciseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Exercise>(entity =>
            {
                entity.ToTable("Exercises");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ImagePath).IsRequired(false);
                entity.Property(e => e.VideoPath).IsRequired(false);
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.Property(s => s.Id).ValueGeneratedOnAdd();
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Repetitions).IsRequired();
                entity.Property(s => s.Series).IsRequired();
                entity.Property(s => s.Time).IsRequired();

                entity.HasOne(s => s.Exercise)
                    .WithMany(e => e.Sessions)
                    .HasForeignKey(s => s.ExerciseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
