using System;

namespace Annium.Net.Http;

public interface IHttpRequestFactory
{
    IHttpRequest New();
    IHttpRequest New(string baseUri);
    IHttpRequest New(Uri baseUri);
}