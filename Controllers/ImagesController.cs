using BIT240149_FoodManagement.Data;
using BIT240149_FoodManagement.Models;
using BIT240149_FoodManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BIT240149_FoodManagement.Controllers;

public class ImagesController : Controller
{
    private readonly AppDbContext _db;
    private readonly UiText _t;

    public ImagesController(AppDbContext db, UiText t)
    {
        _db = db;
        _t = t;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int dishId, string? imageUrl, bool makeThumbnail)
    {
        if (!await _db.Dishes.AnyAsync(d => d.Id == dishId)) return Missing();
        if (!ValidUrl(imageUrl))
        {
            TempData["Error"] = _t["InvalidImageUrl"];
            return RedirectToAction("Details", "Dishes", new { id = dishId });
        }

        await using var tx = await _db.Database.BeginTransactionAsync();
        var noImagesYet = !await _db.DishImages.AnyAsync(i => i.DishId == dishId);
        if (makeThumbnail || noImagesYet)
        {
            // Reset old thumbnail in SQL before inserting a new one.
            await _db.DishImages.Where(i => i.DishId == dishId && i.IsThumbnail)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.IsThumbnail, false));
        }

        _db.DishImages.Add(new DishImage_BIT240149
        {
            DishId = dishId,
            ImageUrl = imageUrl!.Trim(),
            IsThumbnail = makeThumbnail || noImagesYet
        });
        try
        {
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            TempData["Success"] = _t["ImageSaved"];
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            TempData["Error"] = _t["SaveFailed"];
        }
        return RedirectToAction("Details", "Dishes", new { id = dishId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetThumbnail(int id)
    {
        var image = await _db.DishImages.FindAsync(id);
        if (image == null) return Missing();

        await using var tx = await _db.Database.BeginTransactionAsync();
        await _db.DishImages.Where(i => i.DishId == image.DishId && i.IsThumbnail)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.IsThumbnail, false));
        try
        {
            // ExecuteUpdate bypasses change tracking; update the target explicitly too.
            await _db.DishImages.Where(i => i.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.IsThumbnail, true));
            await tx.CommitAsync();
            TempData["Success"] = _t["ThumbnailChanged"];
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            TempData["Error"] = _t["SaveFailed"];
        }
        return RedirectToAction("Details", "Dishes", new { id = image.DishId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var image = await _db.DishImages.FindAsync(id);
        if (image == null) return Missing();
        var dishId = image.DishId;
        var wasThumbnail = image.IsThumbnail;

        await using var tx = await _db.Database.BeginTransactionAsync();
        _db.DishImages.Remove(image);
        try
        {
            await _db.SaveChangesAsync();
            if (wasThumbnail)
            {
                var replacement = await _db.DishImages.Where(i => i.DishId == dishId)
                    .OrderBy(i => i.Id).FirstOrDefaultAsync();
                if (replacement != null)
                {
                    replacement.IsThumbnail = true;
                    await _db.SaveChangesAsync();
                }
            }
            await tx.CommitAsync();
            TempData["Success"] = _t["ImageDeleted"];
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            TempData["Error"] = _t["SaveFailed"];
        }
        return RedirectToAction("Details", "Dishes", new { id = dishId });
    }

    private static bool ValidUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 2000) return false;
        var url = value.Trim();
        if (url.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
            return !url.Contains("\\") && !url.Contains("..") && !url.Contains('?') && !url.Contains('#');
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }

    private IActionResult Missing()
    {
        Response.StatusCode = 404;
        return View("NotFoundItem");
    }
}
