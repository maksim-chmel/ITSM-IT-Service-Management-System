using ITSM.Models;
using ITSM.ViewModels.Manage;

namespace ITSM.Services.Qualification;

public interface IQualificationService
{
    Task<AssignCategoryToUserViewModel?> GetAssignCategoryViewModelAsync(string userId);
    Task<OperationResult> AssignCategoriesToUserAsync(AssignCategoryToUserViewModel model);
}