using MediatR;
using Microsoft.Extensions.Logging;

namespace Ordering.Application.Behaviours;

public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TRequest> where TRequest : IRequest<TRequest>
{
    private readonly ILogger<TRequest> _logger;

    // ReSharper disable once ContextualLoggerProblem
    public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TRequest> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TRequest> next)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogError(ex, "Application Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);
            throw;
        }
    }
}