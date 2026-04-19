using ITSM.Models;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.Services.TicketCategory;

public interface ITicketCategoryService
{
    Task<OperationResult> CreateCategory(string name);
    Task<List<Models.TicketCategory>> GetAllCategoriesToList();
    Task<List<SelectListItem>> GetCategorySelectListAsync(List<Models.TicketCategory>? categories = null);
    Task<OperationResult> DeleteCategory(int id);
    Task<OperationResult> SoftDeleteCategory(int id);
    Task<Models.TicketCategory> GetSubCategoryListAsync(int categoryId);
    Task<OperationResult> DeleteSubCategoryAsync(int subCategoryId);
    Task<OperationResult> SoftDeleteSubCategoryAsync(int subCategoryId);
    Task<OperationResult> AddSubCategoryAsync(SubCategoryCreateViewModel viewModel);
    Task<List<TicketSubCategory>> GetSubCategoriesForCategory(int? selectedCategoryId);
    List<SelectListItem> MapSubCategoriesToSelectList(List<TicketSubCategory> subCategories);
  
}