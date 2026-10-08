using Microsoft.AspNetCore.Diagnostics; 
using Microsoft.EntityFrameworkCore; 
using Naideth.Traslados.Dominio.Kernel.Interfaces;
using System.Diagnostics;

namespace Naideth.Traslados.Api.Kernel.GlobalExceptions
{
    internal sealed class GlobalExceptionHandler : IExceptionHandler
        {
            private readonly ILogger<GlobalExceptionHandler> _logger;

            public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
            {
                _logger = logger;
            }

            public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
            {
                if (httpContext is null)
                {
                    _logger.LogError(exception, "httpContext null");
                    throw new ArgumentNullException(nameof(httpContext));
                }

                var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
                var (statusCode, title, mensage) = ExceptionMapper(exception, traceId ?? "no traceId");
                

                await Results.Problem(
                    title: title,
                    statusCode: statusCode,
                    extensions: new Dictionary<string, object?>
                    {
                    { "traceId", traceId },
                    { "errors", new[] {
                        new ErrorDTO(statusCode.ToString() ,mensage),
                    }}
                    }
                ).ExecuteAsync(httpContext).ConfigureAwait(false);

                return true;
            }
        private (int statusCode, string title, string message) ExceptionMapper(Exception exception, string traceId)
        {
            
            //if (exception is IExcepcion customException)
            //{
            //    _logger.LogWarning(exception, "TraceId: {TraceId}, Message {Message}", traceId, exception.Message);
            //    return (customException.Codigo, customException.Titulo, exception.Message);
            //}
          
            //    _logger.LogError(exception, "TraceId: {TraceId}, Message {Message}", traceId, exception.Message);
            //    return (StatusCodes.Status500InternalServerError, "Error Interno del Servidor", exception.Message);


            switch (exception)
            {
                case IExcepcion customException:
                    _logger.LogWarning(exception, "TraceId: {TraceId}, Message {Mensaje}", traceId, exception.Message);
                    return (customException.Codigo, customException.Titulo, customException.Mensaje);

                case TaskCanceledException taskCanceledException:
                   // _logger.LogWarning(taskCanceledException, "TraceId: {TraceId}, Task was canceled: {Message}", traceId, taskCanceledException.Message);
                    return (StatusCodes.Status408RequestTimeout,"Request Timeout","La solicitud fue cancelada o excedió el tiempo de espera.");

                case DbUpdateException dbUpdateException:
                    //_logger.LogError(dbUpdateException, "TraceId: {TraceId}, Database update error: {Message}", traceId, dbUpdateException.Message);
                    return (StatusCodes.Status400BadRequest, "Error Valor de Datos", "Error Valor de Datos");

                case OperationCanceledException operationCanceledException:
                    //_logger.LogWarning(operationCanceledException, "TraceId: {TraceId}, Operation was canceled: {Message}", traceId, operationCanceledException.Message);
                    return (StatusCodes.Status408RequestTimeout,"Request Timeout","La operación fue cancelada.");

                default:
                    _logger.LogError(exception, "TraceId: {TraceId}, Message {Message}", traceId, exception.Message);
                    return (StatusCodes.Status500InternalServerError, "Internal Server Error", exception.Message);
            }


        } 


    }
    
}
