using System;
using System.Net;

namespace Annium.Net.Servers.Web.Internal;

/// <summary>
/// Resolves <see cref="HttpListener" /> instances based on requested host and port.
/// </summary>
internal class HttpListenerResolver
{
    /// <summary>
    /// Maximum attempts to find a free port when resolving dynamically.
    /// </summary>
    private const int MaxDynamicAttempts = 100;

    /// <summary>
    /// Minimum port number considered for dynamic resolution.
    /// </summary>
    private const ushort MinPortNumber = 1000;

    /// <summary>
    /// Maximum port number considered for dynamic resolution.
    /// </summary>
    private const ushort MaxPortNumber = 65535;

    /// <summary>
    /// Singleton instance of the resolver.
    /// </summary>
    public static readonly HttpListenerResolver Instance = new();

    private HttpListenerResolver() { }

    /// <summary>
    /// Resolves a listener for the provided host and port, choosing dynamic or static mode as needed.
    /// </summary>
    /// <param name="isSecure">Whether to use HTTPS for the listener prefix.</param>
    /// <param name="host">Host name or address to bind.</param>
    /// <param name="port">Port to bind, or 0 to allocate dynamically.</param>
    /// <returns>A configured listener if binding succeeds; otherwise null.</returns>
    public HttpListener? Resolve(bool isSecure, string host, ushort port)
    {
        return port == 0 ? ResolveDynamic(isSecure, host) : ResolveStatic(isSecure, host, port);
    }

    /// <summary>
    /// Attempts to create a listener by selecting a random free port within the allowed range.
    /// </summary>
    /// <param name="isSecure">Whether to use HTTPS for the listener prefix.</param>
    /// <param name="host">Host name or address to bind.</param>
    /// <returns>A configured listener if binding succeeds; otherwise null.</returns>
    private HttpListener? ResolveDynamic(bool isSecure, string host)
    {
        var rnd = new Random();
        for (var i = 0; i < MaxDynamicAttempts; i++)
        {
            // get random port in range from 1000 (below are usually considered as system ports)
            var port = (ushort)rnd.Next(MinPortNumber, MaxPortNumber);
            var listener = ResolveStatic(isSecure, host, port);
            if (listener is null)
                continue;

            return listener;
        }

        return null;
    }

    /// <summary>
    /// Attempts to create and start a listener on a specific port.
    /// </summary>
    /// <param name="isSecure">Whether to use HTTPS for the listener prefix.</param>
    /// <param name="host">Host name or address to bind.</param>
    /// <param name="port">Port number to bind.</param>
    /// <returns>A started listener if binding succeeds; otherwise null.</returns>
    private HttpListener? ResolveStatic(bool isSecure, string host, ushort port)
    {
        try
        {
            var listener = new HttpListener();
            listener.Prefixes.Add($"{(isSecure ? "https" : "http")}://{host}:{port}/");

            if (listener.IsListening)
                return null;

            listener.Start();

            return listener;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
