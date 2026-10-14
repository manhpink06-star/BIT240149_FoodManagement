using BIT240149_FoodManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace BIT240149_FoodManagement.Data;

public static class DemoSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Only seed an initially empty database; user changes are preserved.
        if (await db.DishCategories.AnyAsync()) return;

        var mains = new DishCategory_BIT240149
        {
            Name = "Món chính", Description = "Món ăn dùng trong bữa chính"
        };
        var snacks = new DishCategory_BIT240149
        {
            Name = "Món ăn nhẹ", Description = "Món khai vị và món ăn nhẹ"
        };
        var drinks = new DishCategory_BIT240149
        {
            Name = "Đồ uống", Description = "Đồ uống nóng và lạnh"
        };
        db.DishCategories.AddRange(mains, snacks, drinks);

        db.Dishes.AddRange(
            new Dish_BIT240149
            {
                Name = "Phở bò", Price = 55000, PreparationTime = 20, IsAvailable = true,
                Description = "Phở bò truyền thống với nước dùng thơm.", DishCategory = mains,
                Images = new List<DishImage_BIT240149>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=900", IsThumbnail = true },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1555126634-323283e090fa?w=900" }
                }
            },
            new Dish_BIT240149
            {
                Name = "Cơm gà", Price = 65000, PreparationTime = 25, IsAvailable = true,
                Description = "Cơm nóng, gà mềm và rau ăn kèm.", DishCategory = mains,
                Images = new List<DishImage_BIT240149>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1512058564366-18510be2db19?w=900", IsThumbnail = true }
                }
            },
            new Dish_BIT240149
            {
                Name = "Bún chả", Price = 60000, PreparationTime = 18, IsAvailable = false,
                Description = "Bún chả Hà Nội.", DishCategory = mains
            },
            new Dish_BIT240149
            {
                Name = "Khoai tây chiên", Price = 30000, PreparationTime = 10, IsAvailable = true,
                Description = "Khoai chiên giòn.", DishCategory = snacks,
                Images = new List<DishImage_BIT240149>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=900", IsThumbnail = true }
                }
            },
            new Dish_BIT240149
            {
                Name = "Trà đào", Price = 35000, PreparationTime = 5, IsAvailable = true,
                Description = "Trà đào thơm mát.", DishCategory = drinks
            },
            new Dish_BIT240149
            {
                Name = "Cà phê sữa", Price = 32000, PreparationTime = 7, IsAvailable = true,
                Description = "Cà phê với sữa đặc.", DishCategory = drinks
            }
        );
        await db.SaveChangesAsync();
    }
}
