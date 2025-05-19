using ITSM.ViewModels.Manage;

namespace ITSM.Services.Qualification;

public interface IQualificationService
{
    Task<AssignCategoryToUserViewModel> GetAssignCategoryViewModelAsync(string userId);
    Task<bool> AssignCategoriesToUserAsync(AssignCategoryToUserViewModel model);
}