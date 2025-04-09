using ITSM.Enums;
using ITSM.Repositories.TicketCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.Admin))]
public class TicketCategoryController(ITicketCategoryRepository categoryRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> CategoryList()
    {
        var list = await categoryRepository.GetAllCategoriesToList();
        return View(list);
    }

    [HttpGet]
    public IActionResult CreateCategory()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(string name)
    {
        await categoryRepository.CreateCategory(name);
        TempData["SuccessMessage"] = "Категория успешно создана.";
        return RedirectToAction("CategoryList");
    }
    [HttpPost]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            await categoryRepository.DeleteCategory(id);
            TempData["SuccessMessage"] = "Категория успешно удалена.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction("CategoryList");
    }
    
    [HttpGet]
    public async Task<IActionResult> SubCategoryList(int categoryId)
    {
        var category = await categoryRepository.GetSubCategoryListAsync(categoryId);
        return View(category);
    }


    [HttpPost]
    public async Task<IActionResult> DeleteSubCategory(int subCategoryId)
    {
        await categoryRepository.DeleteSubCategoryAsync(subCategoryId);
        return RedirectToAction("CategoryList");
    }

  
    [HttpPost]
    public async Task<IActionResult> AddSubCategory(int categoryId, string name)
    {
        await categoryRepository.AddSubCategoryAsync(categoryId, name);
        return RedirectToAction(nameof(SubCategoryList), new { categoryId });
    }

}