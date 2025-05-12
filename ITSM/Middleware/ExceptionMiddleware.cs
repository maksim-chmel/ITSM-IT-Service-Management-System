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
               
                logger.LogError(ex, "Unhandled exception occurred.");

            
                var tempData = tempDataFactory.GetTempData(context);
                tempData["ErrorMessage"] = ex.Message;

               
                context.Response.Redirect("/Home/Error");
            }
        }
    }
}

