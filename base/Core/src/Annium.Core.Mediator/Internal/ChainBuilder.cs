using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Logging;
using Annium.Reflection;

namespace Annium.Core.Mediator.Internal;

/// <summary>
/// Builds execution chains for mediator request handling
/// </summary>
internal class ChainBuilder : ILogSubject
{
    /// <summary>
    /// Logger for tracing chain building operations
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Merged mediator configuration containing handlers and matches
    /// </summary>
    private readonly MediatorConfiguration _configuration;

    /// <summary>
    /// Builder for creating next delegate functions in the execution chain
    /// </summary>
    private readonly NextBuilder _nextBuilder;

    /// <summary>
    /// Initializes a new instance of the ChainBuilder class
    /// </summary>
    /// <param name="configurations">Collection of mediator configurations to merge</param>
    /// <param name="nextBuilder">Builder for creating next delegate functions</param>
    /// <param name="logger">Logger for tracing operations</param>
    public ChainBuilder(IEnumerable<MediatorConfiguration> configurations, NextBuilder nextBuilder, ILogger logger)
    {
        _configuration = MediatorConfiguration.Merge(configurations.ToArray());
        _nextBuilder = nextBuilder;
        Logger = logger;
    }

    /// <summary>
    /// Builds an execution chain for processing a request from input type to output type
    /// </summary>
    /// <param name="input">Input type of the request</param>
    /// <param name="output">Expected output type of the response</param>
    /// <returns>Ordered list of chain elements representing the execution path</returns>
    public IReadOnlyList<ChainElement> BuildExecutionChain(Type input, Type output)
    {
        var handlers = _configuration.Handlers.ToList();

        output = ResolveOutput(input, output);

        this.Trace<string, string, int>(
            "Build execution chain for {input} -> {output} from {handlersCount} handler(s) available",
            input.FriendlyName(),
            output.FriendlyName(),
            handlers.Count
        );

        var chain = new List<ChainElement>();
        var isFinalized = false;

        while (true)
        {
            this.Trace<string, string>(
                "Find chain element for {input} -> {output}",
                input.FriendlyName(),
                output.FriendlyName()
            );

            Type? service = null;

            foreach (var handler in handlers.ToArray())
            {
                service = ResolveHandler(input, output, handler);

                if (service is null)
                {
                    this.Trace<string, string>(
                        "Handler {requestIn} -> {responseOut} didn't match",
                        handler.RequestIn.FriendlyName(),
                        handler.ResponseOut.FriendlyName()
                    );
                    continue;
                }

                this.Trace<string, string, string>(
                    "Handler {requestIn} -> {responseOut} resolved into {service}",
                    handler.RequestIn.FriendlyName(),
                    handler.ResponseOut.FriendlyName(),
                    service.FriendlyName()
                );

                handlers.Remove(handler);
                break;
            }

            if (service is null)
            {
                this.Trace<string, string>(
                    "No handler resolved for {input} -> {output}",
                    input.FriendlyName(),
                    output.FriendlyName()
                );
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
            throw new InvalidOperationException(
                $"Can't resolve request handler by input {input.FriendlyName()} and output {output.FriendlyName()}"
            );

        return chain;
    }

    /// <summary>
    /// Resolves the actual output type based on configured type matches
    /// </summary>
    /// <param name="input">Input type to match against</param>
    /// <param name="output">Expected output type</param>
    /// <returns>Resolved output type, or original output type if no match found</returns>
    private Type ResolveOutput(Type input, Type output)
    {
        var match = _configuration.Matches.SingleOrDefault(x => x.RequestedType == input && x.ExpectedType == output);

        return match?.ResolvedType ?? output;
    }

    /// <summary>
    /// Attempts to resolve a specific handler for the given input and output types
    /// </summary>
    /// <param name="input">Input type to match</param>
    /// <param name="output">Output type to match</param>
    /// <param name="handler">Handler definition to resolve against</param>
    /// <returns>Resolved handler service type, or null if no match</returns>
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
            this.Trace<string, string, string>(
                "Can't resolve {handler} by input {requestIn} and output {responseOut}",
                handler.Implementation.FriendlyName(),
                requestIn.FriendlyName(),
                responseOut.FriendlyName()
            );
            return null;
        }

        return service;
    }

    /// <summary>
    /// Logs trace information about the composed execution chain
    /// </summary>
    /// <param name="chain">Chain elements to trace</param>
    private void TraceChain(IReadOnlyCollection<ChainElement> chain)
    {
        this.Trace("Composed chain with {chainCount} handler(s):", chain.Count);
        foreach (var element in chain)
            this.Trace("- {handler}", element.Handler);
    }
}
