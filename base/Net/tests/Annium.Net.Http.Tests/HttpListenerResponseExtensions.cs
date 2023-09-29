using System.Net;

namespace Annium.Net.Http.Tests;

internal static class HttpListenerResponseExtensions
{
    public static void Ok(this HttpListenerResponse response)
    {
        response.StatusCode = (int)HttpStatusCode.OK;
    }
}