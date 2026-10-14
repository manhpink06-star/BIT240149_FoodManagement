namespace BIT240149_FoodManagement.Models;

public class DishCategory_BIT240149
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Dish_BIT240149> Dishes { get; set; } = new List<Dish_BIT240149>();
}
