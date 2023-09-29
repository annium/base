using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Extensions.Validation;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers;

internal abstract class ValidationPipeHandlerBase<TRequest, TResponse> : ILogSubject
{
    public ILogger Logger { get; }
    private readonly IValidator<TRequest> _validator;

    protected ValidationPipeHandlerBase(
        IValidator<TRequest> validator,
        ILogger logger
    )
    {
        _validator = validator;
        Logger = logger;
    }

    public async Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken ct,
        Func<TRequest, CancellationToken, Task<TResponse>> next
    )
    {
        this.Trace("Validate {request}", typeof(TRequest));
        if (request is null)
        {
            this.Trace("Validation of {request} failed - request is null", typeof(TRequest));

            return GetResponse(Result.New().Error("Request is empty"));
        }

        var result = await _validator.ValidateAsync(request);
        if (result.HasErrors)
        {
            this.Trace("Validation of {request} failed", typeof(TRequest));

            return GetResponse(result);
        }

        return await next(request, ct);
    }

    protected abstract TResponse GetResponse(IResult validationResult);
}