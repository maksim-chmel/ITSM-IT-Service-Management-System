using Microsoft.AspNetCore.Mvc.ViewFeatures;
namespace ITSM.Middleware
{
    public class ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        ITempDataDictionaryFactory tempDataFactory)
    {
        // Конструктор

        // Метод для обработки исключений
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Переход к следующему middleware
                await next(context);
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                logger.LogError(ex, "Unhandled exception occurred.");

                // Получаем TempData для передачи сообщения об ошибке
                var tempData = tempDataFactory.GetTempData(context);
                tempData["ErrorMessage"] = ex.Message; // Передаем сообщение об ошибке

                // Перенаправляем пользователя на страницу ошибки
                context.Response.Redirect("/Home/Error");
            }
        }
    }
}

