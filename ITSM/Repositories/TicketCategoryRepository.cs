using ITSM.DB;
using ITSM.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Repositories;

public class TicketCategoryRepository(DBaseContext context) : ITicketCategoryRepository
{
    public async Task CreateCategory(string name)
    {
        var categoryExists = await context.TicketCategories
            .AnyAsync(c => c.Name.ToLower() == name.ToLower());

        if (categoryExists) throw new Exception("Category exist");
        var category = new TicketCategory
        {
            Name = name
        };
        context.TicketCategories.Add(category);
        await context.SaveChangesAsync();
    }

    public async Task<List<TicketCategory>> GetAllCategoriesToList()
    {
        return await context.TicketCategories.ToListAsync();
    }


    public async Task<bool> DeleteCategory(int id)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        var category = await context.TicketCategories
            .Include(c => c.Tickets)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return false;


        foreach (var ticket in category.Tickets)
        {
            ticket.CategoryId = null;
        }

        context.TicketCategories.Remove(category);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }
    public async Task<List<SelectListItem>> GetCategorySelectList()
    {
        return await context.TicketCategories
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToListAsync();
    }
    
   
    public async Task<TicketCategory> GetSubCategoryListAsync(int categoryId)
    {
        return await context.TicketCategories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == categoryId);
    }



    
    public async Task DeleteSubCategoryAsync(int subCategoryId)
    {
        var subCategory = await context.TicketSubCategories.FindAsync(subCategoryId);
        if (subCategory != null)
        {
            context.TicketSubCategories.Remove(subCategory);
            await context.SaveChangesAsync();
        }
    }

  
   
    public async Task AddSubCategoryAsync(int categoryId, string name)
    {
        var subCategory = new TicketSubCategory
        {
            Name = name,
            CategoryId = categoryId
        };

        context.TicketSubCategories.Add(subCategory);
        await context.SaveChangesAsync();
    }

}