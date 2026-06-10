namespace Annium.Net.Http.Internal;

/// <summary>
/// Internal extension of <see cref="IHttpRequest"/> exposing implementation-only members
/// kept off the public <see cref="IHttpRequest"/> contract.
/// </summary>
internal interface IHttpRequestInternal : IHttpRequest
{
    /// <summary>
    /// Gets the serializer for content processing.
    /// </summary>
    Serializer Serializer { get; }
}

/// <summary>
/// Internal helpers for accessing <see cref="IHttpRequestInternal"/> members from an <see cref="IHttpRequest"/>.
/// </summary>
internal static class HttpRequestInternalExtensions
{
    /// <summary>
    /// Gets the content serializer associated with the request.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <returns>The serializer for content processing.</returns>
    internal static Serializer GetSerializer(this IHttpRequest request) => ((IHttpRequestInternal)request).Serializer;
}
