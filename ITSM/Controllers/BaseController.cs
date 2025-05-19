using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

public class BaseController : Controller
{
   
    protected void SetTempDataMessage(bool isSuccess, string successMessage, string errorMessage)
    {
        TempData[isSuccess ? "SuccessMessage" : "ErrorMessage"] = isSuccess ? successMessage : errorMessage;
    }
}