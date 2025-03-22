using ITSM.Models;

namespace ITSM.Repositories;

public interface ITicketCategoryRepository
{
    Task CreateCategory(string name);
    Task<List<TicketCategory>> GetAllCategoriesToList();
    Task<bool> DeleteCategory(int id);
}