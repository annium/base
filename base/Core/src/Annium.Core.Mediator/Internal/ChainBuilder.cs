using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Logging;
using Annium.Reflection;

namespace Annium.Core.Mediator.Internal;

internal class ChainBuilder : ILogSubject
{
    public ILogger Logger { get; }
    private readonly MediatorConfiguration _configuration;
    private readonly NextBuilder _nextBuilder;

    public ChainBuilder(
        IEnumerable<MediatorConfiguration> configurations,
        NextBuilder nextBuilder,
        ILogger logger
    )
    {
        _configuration = MediatorConfiguration.Merge(configurations.ToArray());
        _nextBuilder = nextBuilder;
        Logger = logger;
    }

    public IReadOnlyList<ChainElement> BuildExecutionChain(Type input, Type output)
    {
        var handlers = _configuration.Handlers.ToList();

        output = ResolveOutput(input, output);

        this.Trace<string, string, int>("Build execution chain for {input} -> {output} from {handlersCount} handler(s) available", input.FriendlyName(), output.FriendlyName(), handlers.Count);

        var chain = new List<ChainElement>();
        var isFinalized = false;

        while (true)
        {
            this.Trace<string, string>("Find chain element for {input} -> {output}", input.FriendlyName(), output.FriendlyName());

            Type? service = null;

            foreach (var handler in handlers.ToArray())
            {
                service = ResolveHandler(input, output, handler);

                if (service is null)
                {
                    this.Trace<string, string>("Handler {requestIn} -> {responseOut} didn't match", handler.RequestIn.FriendlyName(), handler.ResponseOut.FriendlyName());
                    continue;
                }

                this.Trace<string, string, string>("Handler {requestIn} -> {responseOut} resolved into {service}", handler.RequestIn.FriendlyName(), handler.ResponseOut.FriendlyName(), service.FriendlyName());

                handlers.Remove(handler);
                break;
            }

            if (service is null)
            {
                this.Trace<string, string>("No handler resolved for {input} -> {output}", input.FriendlyName(), output.FriendlyName());
                break;
            }

            this.Trace<string>("Add {service} to chain", service.FriendlyName());

            var serviceOutput = service.GetTargetImplementation(Constants.HandlerOutputType);
            // if final handler - break
            if (serviceOutput is null)
            {
                chain.Add(new ChainElement(service));
                isFinalized = true;
                this.Trace("Resolved handler is final");
                break;
            }

            var outputArgs = serviceOutput.GetGenericArguments();
            input = outputArgs[0];
            output = outputArgs[1];
            chain.Add(new ChainElement(service, _nextBuilder.BuildNext(input, output)));
        }

        TraceChain(chain);

        if (!isFinalized)
            throw new InvalidOperationException($"Can't resolve request handler by input {input.FriendlyName()} and output {output.FriendlyName()}");

        return chain;
    }

    private Type ResolveOutput(Type input, Type output)
    {
        var match = _configuration.Matches
            .SingleOrDefault(x => x.RequestedType == input && x.ExpectedType == output);

        return match?.ResolvedType ?? output;
    }

    private Type? ResolveHandler(Type input, Type output, Handler handler)
    {
        var requestIn = input.GetTargetImplementation(handler.RequestIn);
        var responseOut = output.GetTargetImplementation(handler.ResponseOut);

        if (requestIn is null || responseOut is null)
            return null;

        var handlerInput = Constants.HandlerInputType.MakeGenericType(requestIn, responseOut);
        var service = handler.Implementation.ResolveByImplementation(handlerInput);
        if (service is null)
        {
            this.Trace<string, string, string>("Can't resolve {handler} by input {requestIn} and output {responseOut}", handler.Implementation.FriendlyName(), requestIn.FriendlyName(), responseOut.FriendlyName());
            return null;
        }

        return service;
    }

    private void TraceChain(IReadOnlyCollection<ChainElement> chain)
    {
        this.Trace("Composed chain with {chainCount} handler(s):", chain.Count);
        foreach (var element in chain)
            this.Trace("- {handler}", element.Handler);
    }
}