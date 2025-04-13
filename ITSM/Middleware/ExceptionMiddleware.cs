using Microsoft.AspNetCore.Mvc.ViewFeatures;
namespace ITSM.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly ITempDataDictionaryFactory _tempDataFactory;

        // Конструктор
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, ITempDataDictionaryFactory tempDataFactory)
        {
            _next = next;
            _logger = logger;
            _tempDataFactory = tempDataFactory;
        }

        // Метод для обработки исключений
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Переход к следующему middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                _logger.LogError(ex, "Unhandled exception occurred.");

                // Получаем TempData для передачи сообщения об ошибке
                var tempData = _tempDataFactory.GetTempData(context);
                tempData["ErrorMessage"] = ex.Message; // Передаем сообщение об ошибке

                // Перенаправляем пользователя на страницу ошибки
                context.Response.Redirect("/Home/Error");
            }
        }
    }
}

