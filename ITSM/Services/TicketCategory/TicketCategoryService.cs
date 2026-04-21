using ITSM.Data;
using ITSM.Models;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Services.TicketCategory;

public class TicketCategoryService(DBaseContext context) : ITicketCategoryService
{
    public async Task<OperationResult> CreateCategory(string name)
    {
        var categoryExists = await context.TicketCategories
            .AnyAsync(c => c.Name.ToLower() == name.ToLower());

        if (categoryExists) return OperationResult.Failure("Category already exists.");

        var category = new Models.TicketCategory
        {
            Name = name
        };
        context.TicketCategories.Add(category);
        await context.SaveChangesAsync();
        return OperationResult.Success("Category created successfully.");
    }


    public async Task<List<Models.TicketCategory>> GetAllCategoriesToList()
    {
        return await context.TicketCategories
            .Include(c => c.SubCategories)
            .ToListAsync();
    }


    public async Task<OperationResult> DeleteCategory(int id)
    {
        // Portfolio/demo rule: we do not permanently delete business entities.
        // Keep the method for backwards compatibility, but perform an archive instead.
        return await SoftDeleteCategory(id);
    }

    public async Task<OperationResult> SoftDeleteCategory(int id)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        var category = await context.TicketCategories
            .Include(c => c.Tickets)
            .Include(c => c.SubCategories)
            .Include(c => c.UserCategoryAssignments) // Eagerly load assignments
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return OperationResult.Failure("Category not found.");


        category.IsDeleted = true;
        context.TicketCategories.Update(category);


        foreach (var subCategory in category.SubCategories)
        {
            subCategory.IsDeleted = true;
            context.TicketSubCategories.Update(subCategory);
        }

        foreach (var assignment in category.UserCategoryAssignments)
        {
            assignment.IsDeleted = true;
            context.UserCategoryAssignments.Update(assignment);
        }

        try
        {
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return OperationResult.Success("Category, its subcategories, and related user assignments have been archived.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return OperationResult.Failure($"Error archiving category: {ex.Message}");
        }
    }


    public async Task<List<SelectListItem>> GetCategorySelectListAsync(List<Models.TicketCategory>? categories = null)
    {
        if (categories == null)
        {
            categories = await context.TicketCategories.ToListAsync();
        }

        return categories
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
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == categoryId);
    }


    public async Task<OperationResult> DeleteSubCategoryAsync(int subCategoryId)
    {
        // Portfolio/demo rule: do not permanently delete.
        return await SoftDeleteSubCategoryAsync(subCategoryId);
    }

    public async Task<OperationResult> SoftDeleteSubCategoryAsync(int subCategoryId)
    {
        var subCategory = await context.TicketSubCategories.FindAsync(subCategoryId);
        if (subCategory == null) return OperationResult.Failure("Subcategory not found.");

        subCategory.IsDeleted = true;
        context.TicketSubCategories.Update(subCategory);

        await context.SaveChangesAsync();
        return OperationResult.Success("Subcategory archived.");
    }


    public async Task<OperationResult> AddSubCategoryAsync(SubCategoryCreateViewModel viewModel)
    {
        var categoryExists = await context.TicketCategories
            .AnyAsync(c => c.Id == viewModel.CategoryId);

        if (!categoryExists)
        {
            return OperationResult.Failure("Parent category not found.");
        }

        var subCategory = new TicketSubCategory()
        {
            Name = viewModel.Name,
            CategoryId = viewModel.CategoryId
        };

        context.TicketSubCategories.Add(subCategory);
        await context.SaveChangesAsync();
        return OperationResult.Success("Subcategory added successfully.");
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
