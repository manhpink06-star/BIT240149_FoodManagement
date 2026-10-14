using BIT240149_FoodManagement.Data;
using BIT240149_FoodManagement.Models;
using BIT240149_FoodManagement.Services;
using BIT240149_FoodManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BIT240149_FoodManagement.Controllers;

public class CategoriesController : Controller
{
    private readonly AppDbContext _db;
    private readonly UiText _t;

    public CategoriesController(AppDbContext db, UiText t)
    {
        _db = db;
        _t = t;
    }

    public async Task<IActionResult> Index()
    {
        var data = await _db.DishCategories.AsNoTracking()
            .Include(c => c.Dishes)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(data);
    }

    public IActionResult Create() => View("Form", new CategoryFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel vm)
    {
        await ValidateAsync(vm);
        if (!ModelState.IsValid) return View("Form", vm);
        _db.DishCategories.Add(new DishCategory_BIT240149
        {
            Name = vm.Name!.Trim(), Description = vm.Description?.Trim()
        });
        try
        {
            await _db.SaveChangesAsync();
            TempData["Success"] = _t["CategorySaved"];
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, _t["SaveFailed"]);
            return View("Form", vm);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _db.DishCategories.FindAsync(id);
        if (entity == null) return Missing();
        return View("Form", new CategoryFormViewModel
        {
            Id = entity.Id, Name = entity.Name, Description = entity.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel vm)
    {
        if (id != vm.Id) return Missing();
        var entity = await _db.DishCategories.FindAsync(id);
        if (entity == null) return Missing();
        await ValidateAsync(vm);
        if (!ModelState.IsValid) return View("Form", vm);
        entity.Name = vm.Name!.Trim();
        entity.Description = vm.Description?.Trim();
        try
        {
            await _db.SaveChangesAsync();
            TempData["Success"] = _t["CategorySaved"];
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, _t["SaveFailed"]);
            return View("Form", vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.DishCategories.FindAsync(id);
        if (category == null) return Missing();

        if (await _db.Dishes.AnyAsync(d => d.DishCategoryId == id))
        {
            TempData["Error"] = _t["CategoryInUse"];
            return RedirectToAction(nameof(Index));
        }

        _db.DishCategories.Remove(category);
        try
        {
            await _db.SaveChangesAsync();
            TempData["Success"] = _t["CategoryDeleted"];
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = _t["CategoryInUse"];
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateAsync(CategoryFormViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.Name))
            ModelState.AddModelError(nameof(vm.Name), _t["RequiredName"]);
        else
        {
            var trimmed = vm.Name.Trim();
            if (trimmed.Length > 120)
                ModelState.AddModelError(nameof(vm.Name), _t["SaveFailed"]);
            if (await _db.DishCategories.AnyAsync(c => c.Name == trimmed && c.Id != vm.Id))
                ModelState.AddModelError(nameof(vm.Name), _t["DuplicateCategory"]);
        }
    }

    private IActionResult Missing()
    {
        Response.StatusCode = 404;
        return View("NotFoundItem");
    }
}
