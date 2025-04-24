using ITSM.DB;
using ITSM.Models;
using ITSM.Repositories.UserManagment;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Repositories.Qualification;

public class QualificationRepository(DBaseContext dBaseContext, IUserManagementRepository userManagement)
    : IQualificationRepository
{
    
    public async Task<AssignCategoryToUserViewModel> GetAssignCategoryViewModelAsync(string userId)
    {
        var user = await userManagement.GetUserById(userId);
        if (user == null)
            return null;

        var assignedCategoryIds = await GetAssignedCategoryIdsAsync(userId);
        var categories = await GetAllTicketCategoriesAsync();

        return new AssignCategoryToUserViewModel
        {
            UserId = user.Id,
            UserName = user.UserName,
            Categories = categories,
            SelectedCategoryIds = assignedCategoryIds,
            SkillLevel = user.SkillLevel 
        };
    }

   
    public async Task<bool> AssignCategoriesToUserAsync(AssignCategoryToUserViewModel model)
    {
        var user = await userManagement.GetUserById(model.UserId);
        if (user == null)
            return false;

      
        user.SkillLevel = model.SkillLevel;

       
        model.SelectedCategoryIds ??= new List<string>();

      
        await RemoveExistingAssignmentsAsync(model.UserId);

       
        await AddNewAssignmentsAsync(model.UserId, model.SelectedCategoryIds);

       
        await dBaseContext.SaveChangesAsync();
        return true;
    }

    private async Task<List<string>> GetAssignedCategoryIdsAsync(string userId)
    {
        return await dBaseContext.UserCategoryAssignments
            .Where(uca => uca.UserId == userId)
            .Select(uca => uca.CategoryId.ToString())
            .ToListAsync();
    }

    private async Task<List<SelectListItem>> GetAllTicketCategoriesAsync()
    {
        return await dBaseContext.TicketCategories
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToListAsync();
    }

    private async Task RemoveExistingAssignmentsAsync(string userId)
    {
        var existingAssignments = await dBaseContext.UserCategoryAssignments
            .Where(uca => uca.UserId == userId)
            .ToListAsync();


        dBaseContext.UserCategoryAssignments.RemoveRange(existingAssignments);


        await dBaseContext.SaveChangesAsync();
    }

    private async Task AddNewAssignmentsAsync(string userId, List<string> categoryIds)
    {
        var assignments = categoryIds.Select(catId => new UserCategoryAssignment
        {
            UserId = userId,
            CategoryId = int.Parse(catId)
        }).ToList();


        await dBaseContext.UserCategoryAssignments.AddRangeAsync(assignments);


        await dBaseContext.SaveChangesAsync();
    }
}