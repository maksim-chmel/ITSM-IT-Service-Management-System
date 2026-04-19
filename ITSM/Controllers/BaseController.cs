using ITSM.Models;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

public class BaseController : Controller
{
    protected void SetNotification(OperationResult result)
    {
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;
    }

    protected void SetNotification(bool isSuccess, string successMessage, string errorMessage)
    {
        TempData[isSuccess ? "SuccessMessage" : "ErrorMessage"] = isSuccess ? successMessage : errorMessage;
    }

    protected void NotifySuccess(string message) => TempData["SuccessMessage"] = message;
    protected void NotifyError(string message) => TempData["ErrorMessage"] = message;
}