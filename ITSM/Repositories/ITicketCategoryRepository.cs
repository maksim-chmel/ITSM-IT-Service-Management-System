using ITSM.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.Repositories;

public interface ITicketCategoryRepository
{
    Task CreateCategory(string name);
    Task<List<TicketCategory>> GetAllCategoriesToList();
    Task<List<SelectListItem>> GetCategorySelectList();
    Task<bool> DeleteCategory(int id);
    Task<TicketCategory> GetSubCategoryListAsync(int categoryId);
    Task DeleteSubCategoryAsync(int subCategoryId);
    Task AddSubCategoryAsync(int categoryId, string name);
}