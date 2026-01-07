using FluentValidation;
using System.Net;
using System.Text.Json;

namespace HRManager.WebAPI.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Tenta executar o pedido normalmente
            }
            catch (Exception ex)
            {
                // Se der erro, captura e trata aqui
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            var errorResponse = new ErrorDetails
            {
                Success = false,
                Message = exception.Message
            };

            switch (exception)
            {
                case ValidationException validationEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = "Erro de validação.";
                    errorResponse.Errors = validationEx.Errors.Select(e => e.ErrorMessage).ToList();
                    break;

                case KeyNotFoundException:
                    // Ex: Colaborador não encontrado
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case UnauthorizedAccessException:
                    // Ex: Tentar editar avaliação de outro
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    errorResponse.Message = "Acesso negado. Não tem permissão para esta ação.";
                    break;

                case ArgumentException:
                    // Ex: Dados inválidos simples
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                default:
                    // Erro inesperado (Bug ou BD em baixo)
                    _logger.LogError(exception, "Erro não tratado ocorrido.");
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Message = "Ocorreu um erro interno no servidor. Contacte o suporte.";
                    break;
            }

            var result = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(result);
        }
    }

    // Classe auxiliar para o formato do JSON de erro
    public class ErrorDetails
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string>? Errors { get; set; } // Lista de erros detalhados (opcional)
    }
}