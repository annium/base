using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Xunit;

namespace Annium.Net.Servers.Sockets.Tests;

/// <summary>
/// Base class for Servers.Sockets tests, providing a helper to start a server
/// with a caller-supplied handler delegate.
/// </summary>
public abstract class TestBase : Testing.TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestBase"/> class.
    /// </summary>
    /// <param name="outputHelper">The xunit output helper.</param>
    protected TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Starts a socket server on a dynamically assigned loopback port using the supplied handler.
    /// </summary>
    /// <param name="handle">Handler invoked for each accepted connection.</param>
    /// <returns>A started <see cref="IServer"/> instance.</returns>
    protected IServer StartServer(Func<IServiceProvider, Socket, CancellationToken, Task> handle)
    {
        this.Trace("start");

        var sp = Get<IServiceProvider>();
        var handler = new DelegateHandler(sp, handle);

        return ServerBuilder.New(sp).WithHandler(handler).Start().NotNull();
    }

    /// <summary>
    /// Starts a socket server on the specified port using the supplied handler.
    /// </summary>
    /// <param name="port">Port to bind to.</param>
    /// <param name="handle">Handler invoked for each accepted connection.</param>
    /// <returns>A started <see cref="IServer"/> instance, or null if the port could not be bound.</returns>
    protected IServer? StartServerOnPort(ushort port, Func<IServiceProvider, Socket, CancellationToken, Task> handle)
    {
        this.Trace("start on port {port}", port);

        var sp = Get<IServiceProvider>();
        var handler = new DelegateHandler(sp, handle);

        return ServerBuilder.New(sp, port).WithHandler(handler).Start();
    }
}

/// <summary>
/// IHandler implementation that delegates to a caller-supplied function.
/// </summary>
file class DelegateHandler : IHandler
{
    /// <summary>The service provider passed to the handler callback.</summary>
    private readonly IServiceProvider _sp;

    /// <summary>The delegate that handles each accepted socket connection.</summary>
    private readonly Func<IServiceProvider, Socket, CancellationToken, Task> _handle;

    public DelegateHandler(IServiceProvider sp, Func<IServiceProvider, Socket, CancellationToken, Task> handle)
    {
        _sp = sp;
        _handle = handle;
    }

    /// <summary>Handles a socket connection by delegating to the configured callback.</summary>
    /// <param name="socket">The accepted socket connection.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the handling operation.</returns>
    public Task HandleAsync(Socket socket, CancellationToken ct) => _handle(_sp, socket, ct);
}
