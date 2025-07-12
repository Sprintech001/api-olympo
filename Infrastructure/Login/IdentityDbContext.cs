using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using olympo_webapi.Models;

namespace olympo_webapi.Infrastructure
{
    public class ApplicationUser : IdentityUser
    {
        public int? UserId { get; set; }
        public User? User { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public new DbSet<User> Users { get; set; }
        public DbSet<Gym> Gyms { get; set; }
        public DbSet<GymUser> GymUsers { get; set; }
        public DbSet<UserExercise> UserExercises { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<Session> Sessions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .HasOne(au => au.User)
                .WithOne()
                .HasForeignKey<ApplicationUser>(au => au.UserId)
                .IsRequired(false);

            builder.Entity<ApplicationUser>()
                .HasIndex(au => au.UserName)
                .IsUnique(false);

            builder.Entity<Exercise>().ToTable("Exercises");
            builder.Entity<Gym>().ToTable("Gyms");
            builder.Entity<GymUser>().ToTable("GymUsers");
            builder.Entity<UserExercise>().ToTable("UserExercises");
            builder.Entity<Session>().ToTable("Sessions");
            builder.Entity<User>().ToTable("Users");

            builder.Entity<Gym>(entity =>
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

            builder.Entity<GymUser>(entity =>
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

            builder.Entity<UserExercise>(entity =>
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

            builder.Entity<Exercise>(entity =>
            {
                entity.ToTable("Exercises");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ImagePath).IsRequired(false);
                entity.Property(e => e.VideoPath).IsRequired(false);
            });

            builder.Entity<Session>(entity =>
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
                    .WithMany(u => u.Sessions)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade); 
            });
        }
    };

}