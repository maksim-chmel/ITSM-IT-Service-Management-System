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

    [HttpGet]
    public IActionResult CreateCategory()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateCategory(string name)
    {
        if (!ModelState.IsValid)
        {
            SetTempDataMessage(false, "", "Please correct the errors in the form.");
            return RedirectToAction("CategoryList");
        }

        var result = await categoryService.CreateCategory(name);
        SetTempDataMessage(result, "Category successfully added.", "Error adding category.");
        return RedirectToAction("CategoryList");
    }



    [HttpPost]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await categoryService.SoftDeleteCategory(id);

        SetTempDataMessage(result, "Category successfully deleted.",
            "Error deleting category. It might be used in tickets.");
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
        SetTempDataMessage(result, "Subcategory successfully deleted.",
            "Error deleting subcategory. It might be used in tickets.");

        return RedirectToAction("CategoryList");
    }


    [HttpPost]
    public async Task<IActionResult> AddSubCategory(SubCategoryCreateViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            SetTempDataMessage(false, "", "Please correct the errors in the form.");

            return RedirectToAction(nameof(SubCategoryList), new { categoryId = viewModel.CategoryId });
        }

        var result = await categoryService.AddSubCategoryAsync(viewModel);
        SetTempDataMessage(result, "Subcategory added successfully.", "Error adding the subcategory.");


        return RedirectToAction(nameof(SubCategoryList), new { categoryId = viewModel.CategoryId });
    }

}