using ITSM.DB;
using ITSM.Models;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Services.TicketCategory;

public class TicketCategoryService(DBaseContext context) : ITicketCategoryService
{
    public async Task<bool> CreateCategory(string name)
    {
        var categoryExists = await context.TicketCategories
            .AnyAsync(c => c.Name.ToLower() == name.ToLower());

        if (categoryExists) return false;

        var category = new Models.TicketCategory
        {
            Name = name
        };
        context.TicketCategories.Add(category);
        await context.SaveChangesAsync();
        return true;
    }


    public async Task<List<Models.TicketCategory>> GetAllCategoriesToList()
    {
        return await context.TicketCategories
            .Where(c => !c.IsDeleted)
            .ToListAsync();
    }


    public async Task<bool> DeleteCategory(int id)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        var category = await context.TicketCategories
            .Include(c => c.Tickets)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return false;


        foreach (var subCategory in category.SubCategories)
        {
            context.TicketSubCategories.Remove(subCategory);
        }


        context.TicketCategories.Remove(category);

        try
        {
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> SoftDeleteCategory(int id)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        var category = await context.TicketCategories
            .Include(c => c.Tickets)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return false;


        category.IsDeleted = true;
        context.TicketCategories.Update(category);


        foreach (var subCategory in category.SubCategories)
        {
            subCategory.IsDeleted = true;
            context.TicketSubCategories.Update(subCategory);
        }

        try
        {
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }


    public async Task<List<SelectListItem>> GetCategorySelectListAsync(List<Models.TicketCategory>? categories = null)
    {
        if (categories == null)
        {
            categories = await context.TicketCategories
                .Where(c => !c.IsDeleted)
                .ToListAsync();
        }

        return categories
            .Where(c => !c.IsDeleted)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToList();
    }


    public async Task<Models.TicketCategory> GetSubCategoryListAsync(int categoryId)
    {
        return await context.TicketCategories
            .Where(c => !c.IsDeleted)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == categoryId);
    }


    public async Task<bool> DeleteSubCategoryAsync(int subCategoryId)
    {
        var subCategory = await context.TicketSubCategories.FindAsync(subCategoryId);
        if (subCategory == null) return false;
        context.TicketSubCategories.Remove(subCategory);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SoftDeleteSubCategoryAsync(int subCategoryId)
    {
        var subCategory = await context.TicketSubCategories.FindAsync(subCategoryId);
        if (subCategory == null) return false;

        subCategory.IsDeleted = true;
        context.TicketSubCategories.Update(subCategory);

        await context.SaveChangesAsync();
        return true;
    }


    public async Task<bool> AddSubCategoryAsync(SubCategoryCreateViewModel viewModel)
    {
        var categoryExists = await context.TicketCategories
            .AnyAsync(c => c.Id == viewModel.CategoryId);

        if (!categoryExists)
        {
            return false;
        }

        var subCategory = new TicketSubCategory()
        {
            Name = viewModel.Name,
            CategoryId = viewModel.CategoryId
        };

        context.TicketSubCategories.Add(subCategory);
        await context.SaveChangesAsync();
        return true;
    }

    private async Task<List<TicketSubCategory>> GetSubCategories(int categoryId)
    {
        return await context.TicketSubCategories
            .Where(sc => sc.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<List<TicketSubCategory>> GetSubCategoriesForCategory(int? selectedCategoryId)
    {
        if (selectedCategoryId.HasValue)
        {
            return await GetSubCategories(selectedCategoryId.Value);
        }

        return new List<TicketSubCategory>();
    }

    public List<SelectListItem> MapSubCategoriesToSelectList(List<TicketSubCategory> subCategories)
    {
        return subCategories.Select(sc => new SelectListItem
        {
            Value = sc.Id.ToString(),
            Text = sc.Name
        }).ToList();
    }
}