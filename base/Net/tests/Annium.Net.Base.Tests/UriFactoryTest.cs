using System;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Base.Tests;

public class UriFactoryTest
{
    [Fact]
    public void Create_Uri_Works()
    {
        // act
        var uri = UriFactory.Base(new Uri("https://example.com")).Build();

        // assert
        uri.ToString().Is("https://example.com/");
    }

    [Fact]
    public void Create_Uri_Relative_Throws()
    {
        // assert
        Wrap.It(() => UriFactory.Base(new Uri("example.com/path")).Build()).Throws<UriFormatException>();
    }

    [Fact]
    public void Create_string_Works()
    {
        // act
        var uri = UriFactory.Base("https://example.com").Build();

        // assert
        uri.ToString().Is("https://example.com/");
    }

    [Fact]
    public void Create_string_Relative_Throws()
    {
        // assert
        Wrap.It(() => UriFactory.Base("example.com/path").Build()).Throws<UriFormatException>();
    }

    [Fact]
    public void Create_empty_Works()
    {
        // act
        var uri = UriFactory.Base().Path("https://example.com").Build();

        // assert
        uri.ToString().Is("https://example.com/");
    }

    [Fact]
    public void Create_empty_Unfilled_Throws()
    {
        // assert
        Wrap.It(() => UriFactory.Base().Build()).Throws<UriFormatException>();
    }

    [Fact]
    public void Create_empty_Relative_Throws()
    {
        // assert
        Wrap.It(() => UriFactory.Base("example.com/path").Build()).Throws<UriFormatException>();
    }

    [Fact]
    public void Path_Full_Throws()
    {
        // assert
        Wrap.It(() => UriFactory.Base("https://example.com").Path("https://localhost:9000/path").Build()).Throws<UriFormatException>();
    }

    [Fact]
    public void Path_Relative_Works()
    {
        UriFactory.Base("https://example.com/some").Path("/path/on/server").Build().ToString()
            .Is("https://example.com/path/on/server");
        UriFactory.Base("https://example.com/some").Path("path/on/server").Build().ToString()
            .Is("https://example.com/some/path/on/server");
        UriFactory.Base("https://example.com").Path("/path/on/server").Build().ToString()
            .Is("https://example.com/path/on/server");
        UriFactory.Base("https://example.com").Path("path/on/server").Build().ToString()
            .Is("https://example.com/path/on/server");
    }

    [Fact]
    public void Path_Root_Works()
    {
        // act
        var uri = UriFactory.Base("https://example.com").Path("/").Build().ToString();

        // assert
        uri.Is("https://example.com/");
    }

    [Fact]
    public void Path_Ports_Work()
    {
        // assert
        UriFactory.Base("https://example.com:443/").Path("/path/on/server").Build().ToString()
            .Is("https://example.com/path/on/server");
        UriFactory.Base("https://example.com:8443/").Path("/path/on/server").Build().ToString()
            .Is("https://example.com:8443/path/on/server");
        UriFactory.Base("https://example.com:443/").Path("path/on/server").Build().ToString()
            .Is("https://example.com/path/on/server");
        UriFactory.Base("https://example.com:8443/").Path("path/on/server").Build().ToString()
            .Is("https://example.com:8443/path/on/server");
    }

    [Fact]
    public void Path_Query_Works()
    {
        // act
        var uri = UriFactory.Base("https://example.com").Path("path/on/server?with=query").Build().ToString();

        // assert
        uri.Is("https://example.com/path/on/server?with=query");
    }

    [Fact]
    public void Path_QueryParams_Works()
    {
        // act
        var uri = UriFactory.Base("https://example.com")
            .Path("path/on/server?with=query")
            .Param("with", "param")
            .Param("int", 20)
            .Param("null", null as object)
            .Param("x", new[] { 1, 2 })
            .Build()
            .ToString();

        // assert
        uri.Is("https://example.com/path/on/server?with=query&with=param&int=20&null=&x=1&x=2");
    }

    [Fact]
    public void Clone_Works()
    {
        // act
        var uri = UriFactory.Base("https://example.com").Path("path/on/server?with=query")
            .Param("with", "param").Param("int", 20).Clone().Build().ToString();

        // assert
        uri.Is("https://example.com/path/on/server?with=query&with=param&int=20");
    }
}