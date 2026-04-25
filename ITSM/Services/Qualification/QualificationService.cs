using ITSM.Data;
using ITSM.Models;
using ITSM.Services.TicketCategory;
using ITSM.Services.UserManagement;
using ITSM.ViewModels.Manage;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Services.Qualification;

public class QualificationService(DBaseContext dBaseContext, IUserManagementService userManagement,ITicketCategoryService categoryService)
    : IQualificationService
{
    public async Task<AssignCategoryToUserViewModel?> GetAssignCategoryViewModelAsync(string userId)
    {
        var user = await userManagement.GetUserById(userId);
        if (user == null)
            return null;

        var assignedCategoryIds = await GetAssignedCategoryIdsAsync(userId);
        var categories = await categoryService.GetCategorySelectListAsync();

        return new AssignCategoryToUserViewModel
        {
            UserId = user.Id,
            UserName = user.UserName,
            Categories = categories,
            SelectedCategoryIds = assignedCategoryIds,
            SkillLevel = user.SkillLevel
        };
    }


    public async Task<OperationResult> AssignCategoriesToUserAsync(AssignCategoryToUserViewModel model)
    {
        var user = await userManagement.GetUserById(model.UserId);
        if (user == null)
            return OperationResult.Failure("User not found.");
            
        user.SkillLevel = model.SkillLevel;
        model.SelectedCategoryIds ??= new List<string>();

        // Portfolio/demo rule: do not physically delete assignments.
        // We keep a stable (UserId, CategoryId) row and toggle IsDeleted.
        var selectedCategoryIds = model.SelectedCategoryIds
            .Select(int.Parse)
            .ToHashSet();

        var existing = await dBaseContext.UserCategoryAssignments
            .IgnoreQueryFilters()
            .Where(uca => uca.UserId == model.UserId)
            .ToListAsync();

        foreach (var assignment in existing)
        {
            assignment.IsDeleted = !selectedCategoryIds.Contains(assignment.CategoryId);
        }

        var existingCategoryIds = existing.Select(x => x.CategoryId).ToHashSet();
        var missing = selectedCategoryIds.Except(existingCategoryIds);
        foreach (var catId in missing)
        {
            dBaseContext.UserCategoryAssignments.Add(new UserCategoryAssignment
            {
                UserId = model.UserId,
                CategoryId = catId,
                IsDeleted = false
            });
        }

        await dBaseContext.SaveChangesAsync();
        return OperationResult.Success("User qualifications and categories updated.");
    }

    private async Task<List<string>> GetAssignedCategoryIdsAsync(string userId)
    {
        return await dBaseContext.UserCategoryAssignments
            .Where(uca => uca.UserId == userId)
            .Select(uca => uca.CategoryId.ToString())
            .ToListAsync();
    }

    // Legacy helpers removed: assignments are now archived/restored via IsDeleted toggling.
}
