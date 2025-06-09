using System.Net;

namespace Annium.Net.Http.Tests;

/// <summary>
/// Extension methods for HttpListenerResponse.
/// </summary>
internal static class HttpListenerResponseExtensions
{
    /// <summary>
    /// Sets the response status code to OK (200).
    /// </summary>
    /// <param name="response">The HTTP listener response.</param>
    public static void Ok(this HttpListenerResponse response)
    {
        response.StatusCode = (int)HttpStatusCode.OK;
    }
}
