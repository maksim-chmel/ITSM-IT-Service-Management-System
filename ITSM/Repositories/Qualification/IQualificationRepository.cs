using ITSM.ViewModels.Manage;

namespace ITSM.Repositories.Qualification;

public interface IQualificationRepository
{
    Task<AssignCategoryToUserViewModel> GetAssignCategoryViewModelAsync(string userId);
    Task<bool> AssignCategoriesToUserAsync(AssignCategoryToUserViewModel model);
}