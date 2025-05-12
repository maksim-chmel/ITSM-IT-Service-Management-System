using ITSM.Enums;
using ITSM.Repositories.TicketCategory;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.Admin))]
public class TicketCategoryController(ITicketCategoryService categoryService) : Controller
{
    private void SetTempDataMessage(bool isSuccess, string successMessage, string errorMessage)
    {
        if (isSuccess)
        {
            TempData["SuccessMessage"] = successMessage;
        }
        else
        {
            TempData["ErrorMessage"] = errorMessage;
        }
    }

    [HttpGet]
    public async Task<IActionResult> CategoryList()
    {
        var list = await categoryService.GetAllCategoriesToList();
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
       
          var result =  await categoryService.CreateCategory(name);
          SetTempDataMessage(result, "категория успешно добавлена.",
              "Ошибка при добавлении категории.");
         
        return RedirectToAction("CategoryList");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await categoryService.SoftDeleteCategory(id);

        SetTempDataMessage(result, "Категория успешно удалена.",
            "Ошибка при удалении категории. Возможно, она используется в тикетах.");

        return RedirectToAction("CategoryList");
    }

    [HttpGet]
    public async Task<IActionResult> SubCategoryList(int categoryId)
    {
        var category = await categoryService.GetSubCategoryListAsync(categoryId);


        ViewData["Category"] = category;


        var viewModel = new SubCategoryCreateViewModel
        {
            CategoryId = category.Id,
            Name = string.Empty
        };

        return View(viewModel);
    }


    [HttpPost]
    public async Task<IActionResult> DeleteSubCategory(int subCategoryId)
    {
        var result = await categoryService.SoftDeleteSubCategoryAsync(subCategoryId);
        SetTempDataMessage(result, "Подкатегория успешно удалена.",
            "Ошибка при удалении подкатегории. Возможно, она используется в тикетах.");
        return RedirectToAction("CategoryList");
    }


    [HttpPost]
    public async Task<IActionResult> AddSubCategory(SubCategoryCreateViewModel viewModel)
    {
       
          var  result =await categoryService.AddSubCategoryAsync(viewModel);
          
          SetTempDataMessage(result, "Подкатегория успешно добавлена.",
              "Ошибка при добавлении подкатегории.");
        
       

        return RedirectToAction(nameof(SubCategoryList), new { viewModel.CategoryId });
    }
}