using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Domain.Entities;
using TaskFlow.Web.Areas.Admin.ViewModels;

namespace TaskFlow.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly IUnitOfWork _uow;

    public CategoriesController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _uow.Categories.Query().OrderBy(c => c.Name).ToListAsync();
        return View(categories);
    }

    public IActionResult Create() => View(new AdminCategoryViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminCategoryViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        if (await _uow.Categories.AnyAsync(c => c.Name == vm.Name))
        {
            ModelState.AddModelError(string.Empty, "A category with this name already exists.");
            return View(vm);
        }

        await _uow.Categories.AddAsync(new Category
        {
            Name = vm.Name,
            Description = vm.Description,
            ColorHex = vm.ColorHex,
            IsActive = vm.IsActive
        });
        await _uow.SaveChangesAsync();

        TempData["StatusMessage"] = "Category created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _uow.Categories.GetByIdAsync(id);
        if (category == null) return NotFound();

        return View(new AdminCategoryViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ColorHex = category.ColorHex,
            IsActive = category.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminCategoryViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var category = await _uow.Categories.GetByIdAsync(vm.Id);
        if (category == null) return NotFound();

        category.Name = vm.Name;
        category.Description = vm.Description;
        category.ColorHex = vm.ColorHex;
        category.IsActive = vm.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        _uow.Categories.Update(category);
        await _uow.SaveChangesAsync();

        TempData["StatusMessage"] = "Category updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _uow.Categories.GetByIdAsync(id);
        if (category != null)
        {
            _uow.Categories.Remove(category);
            await _uow.SaveChangesAsync();
        }

        TempData["StatusMessage"] = "Category deleted.";
        return RedirectToAction(nameof(Index));
    }
}
