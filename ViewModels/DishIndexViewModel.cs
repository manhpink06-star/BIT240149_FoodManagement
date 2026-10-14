using BIT240149_FoodManagement.Models;

namespace BIT240149_FoodManagement.ViewModels;

public class DishIndexViewModel
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string Status { get; set; } = "all";
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string Sort { get; set; } = "name";
    public List<Dish_BIT240149> Dishes { get; set; } = new();
    public List<DishCategory_BIT240149> Categories { get; set; } = new();
}
