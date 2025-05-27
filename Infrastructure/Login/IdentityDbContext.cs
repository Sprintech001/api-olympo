using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using olympo_webapi.Models;

public class ApplicationUser : IdentityUser
{
    public int? UserId { get; set; } 
    public User? User { get; set; } 
}

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public new DbSet<User> Users { get; set; } 

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
            .HasForeignKey<ApplicationUser>(au => au.UserId);

        builder.Entity<ApplicationUser>()
            .HasIndex(au => au.UserName)
            .IsUnique(false); 

        builder.Entity<User>().ToTable("Users");
    }
};
