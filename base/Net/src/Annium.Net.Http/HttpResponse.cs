using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Annium.Net.Http;

public class HttpResponse<T> : HttpResponse, IHttpResponse<T>
{
    public T Data { get; }

    public HttpResponse(
        IHttpResponse message,
        T data
    ) : base(message)
    {
        Data = data;
    }
}

public class HttpResponse : IHttpResponse
{
    private static readonly HttpResponseHeaders DefaultHeaders;

    static HttpResponse()
    {
        using var message = new HttpResponseMessage();
        DefaultHeaders = message.Headers;
    }

    public bool IsAbort { get; }
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public HttpStatusCode StatusCode { get; }
    public string StatusText { get; }
    public Uri Uri { get; }
    public HttpResponseHeaders Headers { get; }
    public HttpContent Content { get; }

    public HttpResponse(Uri uri, HttpResponseMessage message)
    {
        IsAbort = false;
        IsSuccess = message.IsSuccessStatusCode;
        IsFailure = !message.IsSuccessStatusCode;
        StatusCode = message.StatusCode;
        StatusText = message.ReasonPhrase ?? string.Empty;
        Uri = uri;
        Headers = message.Headers;
        Content = message.Content;
    }

    internal HttpResponse(
        Uri uri,
        HttpStatusCode statusCode,
        string statusText,
        string message
    )
    {
        IsAbort = true;
        IsSuccess = false;
        IsFailure = false;
        StatusCode = statusCode;
        StatusText = statusText;
        Uri = uri;
        Headers = DefaultHeaders;
        Content = new StringContent(message);
    }

    protected HttpResponse(IHttpResponse response)
    {
        IsAbort = response.IsAbort;
        IsSuccess = response.IsSuccess;
        IsFailure = response.IsFailure;
        StatusCode = response.StatusCode;
        StatusText = response.StatusText;
        Uri = response.Uri;
        Headers = response.Headers;
        Content = response.Content;
    }
}