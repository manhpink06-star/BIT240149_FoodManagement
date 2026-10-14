namespace BIT240149_FoodManagement.Models;

public class Dish_BIT240149
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int PreparationTime { get; set; }
    public bool IsAvailable { get; set; }
    public string? Description { get; set; }
    public int DishCategoryId { get; set; }
    public DishCategory_BIT240149 DishCategory { get; set; } = null!;
    public ICollection<DishImage_BIT240149> Images { get; set; } = new List<DishImage_BIT240149>();
}
