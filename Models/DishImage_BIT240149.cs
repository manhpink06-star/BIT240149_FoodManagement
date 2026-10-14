namespace BIT240149_FoodManagement.Models;

public class DishImage_BIT240149
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsThumbnail { get; set; }
    public int DishId { get; set; }
    public Dish_BIT240149 Dish { get; set; } = null!;
}
