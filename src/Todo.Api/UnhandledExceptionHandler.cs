using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Todo.Api;

public sealed partial class UnhandledExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<UnhandledExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // O cliente desistiu. Não é falha, e já não há ninguém para receber a resposta.
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            return false;
        }

        // Em .NET 10 o middleware deixa de registar quando devolvemos true, por isso registamos aqui.
        // Preferia que fosse ele a fazê-lo, já dentro do contexto do pedido, em vez desta chamada à mão: é o que SuppressDiagnosticsCallback devolve.
        LogUnhandled(logger, httpContext.Request.Method, httpContext.Request.Path, exception);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError
            }
        });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception for {Method} {Path}")]
    private static partial void LogUnhandled(ILogger logger, string method, string path, Exception exception);
}
