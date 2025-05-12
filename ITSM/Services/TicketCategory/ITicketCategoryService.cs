using ITSM.Models;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.Repositories.TicketCategory;

public interface ITicketCategoryService
{
    Task<bool> CreateCategory(string name);
    Task<List<Models.TicketCategory>> GetAllCategoriesToList();
    Task<List<SelectListItem>> GetCategorySelectListAsync(List<Models.TicketCategory>? categories = null);
    Task<bool> DeleteCategory(int id);
    Task<bool> SoftDeleteCategory(int id);
    Task<Models.TicketCategory> GetSubCategoryListAsync(int categoryId);
    Task<bool> DeleteSubCategoryAsync(int subCategoryId);
    Task<bool> SoftDeleteSubCategoryAsync(int subCategoryId);
    Task<bool> AddSubCategoryAsync(SubCategoryCreateViewModel viewModel);
    Task<List<TicketSubCategory>> GetSubCategoriesForCategory(int? selectedCategoryId);
    List<SelectListItem> MapSubCategoriesToSelectList(List<TicketSubCategory> subCategories);
  
}