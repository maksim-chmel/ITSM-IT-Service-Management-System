using Microsoft.AspNetCore.Mvc.ViewFeatures;
namespace ITSM.Middleware
{
    public class ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        ITempDataDictionaryFactory tempDataFactory)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                // Log the full exception for developers
                logger.LogError(ex, "Unhandled exception occurred.");

                // Provide a safe, generic message to the user
                var tempData = tempDataFactory.GetTempData(context);
                tempData["ErrorMessage"] = "An unexpected error occurred. Please contact support if the problem persists.";

                // Redirect to a safe error page
                context.Response.Redirect("/Home/Error");
            }
        }
    }
}

