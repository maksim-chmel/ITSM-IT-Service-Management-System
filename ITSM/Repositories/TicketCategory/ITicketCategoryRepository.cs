using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.Repositories.TicketCategory;

public interface ITicketCategoryRepository
{
    Task CreateCategory(string name);
    Task<List<Models.TicketCategory>> GetAllCategoriesToList();
    Task<List<SelectListItem>> GetCategorySelectList();
    Task<bool> DeleteCategory(int id);
    Task<Models.TicketCategory> GetSubCategoryListAsync(int categoryId);
    Task DeleteSubCategoryAsync(int subCategoryId);
    Task AddSubCategoryAsync(int categoryId, string name);
}