using ITSM.Enums;
using ITSM.Services.TicketCategory;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.Admin))]
public class TicketCategoryController(ITicketCategoryService categoryService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> CategoryList()
    {
        var list = await categoryService.GetAllCategoriesToList();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            NotifyError("Category name is required.");
            return RedirectToAction("CategoryList");
        }

        var result = await categoryService.CreateCategory(name);
        SetNotification(result);
        return RedirectToAction("CategoryList");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await categoryService.SoftDeleteCategory(id);
        SetNotification(result);
        return RedirectToAction("CategoryList");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSubCategory(int subCategoryId)
    {
        var result = await categoryService.SoftDeleteSubCategoryAsync(subCategoryId);
        SetNotification(result);
        return RedirectToAction("CategoryList");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSubCategory(SubCategoryCreateViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            NotifyError("Please correct the errors in the form.");
            return RedirectToAction("CategoryList");
        }

        var result = await categoryService.AddSubCategoryAsync(viewModel);
        SetNotification(result);

        return RedirectToAction("CategoryList");
    }

}