using ITSM.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = "Admin")]
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

}