using BIT240149_FoodManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace BIT240149_FoodManagement.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DishCategory_BIT240149> DishCategories => Set<DishCategory_BIT240149>();
    public DbSet<Dish_BIT240149> Dishes => Set<Dish_BIT240149>();
    public DbSet<DishImage_BIT240149> DishImages => Set<DishImage_BIT240149>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DishCategory_BIT240149>(entity =>
        {
            entity.ToTable("BIT240149_DishCategories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("BIT240149_Id");
            entity.Property(x => x.Name).HasColumnName("BIT240149_Name").HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasColumnName("BIT240149_Description").HasMaxLength(1000);
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasMany(x => x.Dishes)
                .WithOne(x => x.DishCategory)
                .HasForeignKey(x => x.DishCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Dish_BIT240149>(entity =>
        {
            entity.ToTable("BIT240149_Dishes", table =>
            {
                table.HasCheckConstraint("CK_BIT240149_DishPrice", "[BIT240149_Price] > 0");
                table.HasCheckConstraint("CK_BIT240149_PreparationTime", "[BIT240149_PreparationTime] > 0");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("BIT240149_Id");
            entity.Property(x => x.Name).HasColumnName("BIT240149_Name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Price).HasColumnName("BIT240149_Price").HasPrecision(14, 2);
            entity.Property(x => x.PreparationTime).HasColumnName("BIT240149_PreparationTime");
            entity.Property(x => x.IsAvailable).HasColumnName("BIT240149_IsAvailable");
            entity.Property(x => x.Description).HasColumnName("BIT240149_Description").HasMaxLength(2000);
            entity.Property(x => x.DishCategoryId).HasColumnName("BIT240149_DishCategoryId");
            entity.HasIndex(x => new { x.DishCategoryId, x.Name }).IsUnique();
            entity.HasMany(x => x.Images)
                .WithOne(x => x.Dish)
                .HasForeignKey(x => x.DishId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DishImage_BIT240149>(entity =>
        {
            entity.ToTable("BIT240149_DishImages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("BIT240149_Id");
            entity.Property(x => x.ImageUrl).HasColumnName("BIT240149_ImageUrl").HasMaxLength(2000).IsRequired();
            entity.Property(x => x.IsThumbnail).HasColumnName("BIT240149_IsThumbnail");
            entity.Property(x => x.DishId).HasColumnName("BIT240149_DishId");
            // Database-level guarantee: each dish has at most one thumbnail.
            entity.HasIndex(x => x.DishId).IsUnique()
                .HasFilter("[BIT240149_IsThumbnail] = 1");
        });
    }
}
