using Microsoft.AspNetCore.Mvc.Rendering;

namespace BIT240149_FoodManagement.ViewModels;

public class DishFormViewModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int PreparationTime { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? Description { get; set; }
    public int DishCategoryId { get; set; }
    public List<SelectListItem> Categories { get; set; } = new();
}
