using BIT240149_FoodManagement.Data;
using BIT240149_FoodManagement.Models;
using BIT240149_FoodManagement.Services;
using BIT240149_FoodManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BIT240149_FoodManagement.Controllers;

public class DishesController : Controller
{
    private readonly AppDbContext _db;
    private readonly UiText _t;

    public DishesController(AppDbContext db, UiText t)
    {
        _db = db;
        _t = t;
    }

    public async Task<IActionResult> Index(
        string? search, int? categoryId, string status = "all",
        decimal? minPrice = null, decimal? maxPrice = null, string sort = "name")
    {
        var vm = new DishIndexViewModel
        {
            Search = search,
            CategoryId = categoryId,
            Status = status,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Sort = sort,
            Categories = await _db.DishCategories.AsNoTracking().OrderBy(c => c.Name).ToListAsync()
        };

        // IQueryable: filters are translated to SQL WHERE, before ToListAsync().
        IQueryable<Dish_BIT240149> query = _db.Dishes.AsNoTracking()
            .Include(d => d.DishCategory)
            .Include(d => d.Images)
            .AsSplitQuery();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(d => d.Name.Contains(term));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
            query = query.Where(d => d.DishCategoryId == categoryId.Value);

        if (status == "available") query = query.Where(d => d.IsAvailable);
        else if (status == "unavailable") query = query.Where(d => !d.IsAvailable);

        var invalidRange = (minPrice.HasValue && minPrice < 0)
            || (maxPrice.HasValue && maxPrice < 0)
            || (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice);
        if (invalidRange)
        {
            ModelState.AddModelError(string.Empty, _t["InvalidPriceRange"]);
        }
        else
        {
            if (minPrice.HasValue) query = query.Where(d => d.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(d => d.Price <= maxPrice.Value);
        }

        query = sort switch
        {
            "price_asc" => query.OrderBy(d => d.Price).ThenBy(d => d.Name),
            "price_desc" => query.OrderByDescending(d => d.Price).ThenBy(d => d.Name),
            "prep_asc" => query.OrderBy(d => d.PreparationTime).ThenBy(d => d.Name),
            _ => query.OrderBy(d => d.Name)
        };

        vm.Dishes = await query.ToListAsync();
        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var dish = await _db.Dishes.AsNoTracking()
            .Include(d => d.DishCategory)
            .Include(d => d.Images)
            .FirstOrDefaultAsync(d => d.Id == id);
        return dish == null ? Missing() : View(dish);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new DishFormViewModel();
        await FillCategoriesAsync(vm);
        return View("Form", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DishFormViewModel vm)
    {
        await ValidateFormAsync(vm);
        if (!ModelState.IsValid)
        {
            await FillCategoriesAsync(vm);
            return View("Form", vm);
        }

        var dish = new Dish_BIT240149();
        Apply(vm, dish);
        _db.Dishes.Add(dish);
        try
        {
            await _db.SaveChangesAsync();
            TempData["Success"] = _t["DishSaved"];
            return RedirectToAction(nameof(Details), new { id = dish.Id });
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, _t["SaveFailed"]);
            await FillCategoriesAsync(vm);
            return View("Form", vm);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var dish = await _db.Dishes.FindAsync(id);
        if (dish == null) return Missing();
        var vm = new DishFormViewModel
        {
            Id = dish.Id,
            Name = dish.Name,
            Price = dish.Price,
            PreparationTime = dish.PreparationTime,
            IsAvailable = dish.IsAvailable,
            Description = dish.Description,
            DishCategoryId = dish.DishCategoryId
        };
        await FillCategoriesAsync(vm);
        return View("Form", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DishFormViewModel vm)
    {
        if (id != vm.Id) return Missing();
        var dish = await _db.Dishes.FindAsync(id);
        if (dish == null) return Missing();
        await ValidateFormAsync(vm);
        if (!ModelState.IsValid)
        {
            await FillCategoriesAsync(vm);
            return View("Form", vm);
        }

        Apply(vm, dish);
        try
        {
            await _db.SaveChangesAsync();
            TempData["Success"] = _t["DishSaved"];
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, _t["SaveFailed"]);
            await FillCategoriesAsync(vm);
            return View("Form", vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var dish = await _db.Dishes.FindAsync(id);
        if (dish == null) return Missing();
        _db.Dishes.Remove(dish);
        try
        {
            await _db.SaveChangesAsync(); // images are deleted via FK cascade
            TempData["Success"] = _t["DishDeleted"];
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = _t["SaveFailed"];
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateFormAsync(DishFormViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.Name))
            ModelState.AddModelError(nameof(vm.Name), _t["RequiredName"]);
        else if (vm.Name.Trim().Length > 150)
            ModelState.AddModelError(nameof(vm.Name), _t["SaveFailed"]);

        if (vm.Price <= 0) ModelState.AddModelError(nameof(vm.Price), _t["PricePositive"]);
        if (vm.PreparationTime <= 0)
            ModelState.AddModelError(nameof(vm.PreparationTime), _t["TimePositive"]);
        if (!await _db.DishCategories.AnyAsync(c => c.Id == vm.DishCategoryId))
            ModelState.AddModelError(nameof(vm.DishCategoryId), _t["CategoryRequired"]);

        if (!string.IsNullOrWhiteSpace(vm.Name) && vm.DishCategoryId > 0)
        {
            var trimmed = vm.Name.Trim();
            if (await _db.Dishes.AnyAsync(d => d.Name == trimmed
                && d.DishCategoryId == vm.DishCategoryId && d.Id != vm.Id))
                ModelState.AddModelError(nameof(vm.Name), _t["DuplicateDish"]);
        }
    }

    private async Task FillCategoriesAsync(DishFormViewModel vm)
    {
        vm.Categories = await _db.DishCategories.AsNoTracking().OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();
    }

    private static void Apply(DishFormViewModel vm, Dish_BIT240149 dish)
    {
        dish.Name = vm.Name!.Trim();
        dish.Price = vm.Price;
        dish.PreparationTime = vm.PreparationTime;
        dish.IsAvailable = vm.IsAvailable;
        dish.Description = vm.Description?.Trim();
        dish.DishCategoryId = vm.DishCategoryId;
    }

    private IActionResult Missing()
    {
        Response.StatusCode = 404;
        return View("NotFoundItem");
    }
}
