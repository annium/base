using System;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Annium.Identity.Tokens.Jwt.Tests;

/// <summary>
/// Verifies that <see cref="ServiceContainerExtensions.AddJwtTokens{TContainer}"/> enforces
/// fail-fast guards for required configuration: <c>SigningKey</c>, <c>Algorithm</c>, and <c>Issuer</c>
/// must all be supplied or the method throws <see cref="InvalidOperationException"/> immediately.
/// </summary>
public class ServiceContainerExtensionsTests
{
    /// <summary>
    /// When <c>SigningKey</c> is left unset, <see cref="ServiceContainerExtensions.AddJwtTokens{TContainer}"/>
    /// must throw <see cref="InvalidOperationException"/> at registration time.
    /// </summary>
    [Fact]
    public void AddJwtTokens_SigningKeyNull_Throws()
    {
        var container = new ServiceContainer();

        Wrap.It(() =>
                container.AddJwtTokens(o =>
                {
                    o.Algorithm = SecurityAlgorithms.RsaSha256;
                    o.Issuer = "service";
                    // SigningKey intentionally left null
                })
            )
            .Throws<InvalidOperationException>();
    }

    /// <summary>
    /// When <c>Algorithm</c> is left unset (empty string), <see cref="ServiceContainerExtensions.AddJwtTokens{TContainer}"/>
    /// must throw <see cref="InvalidOperationException"/> at registration time.
    /// </summary>
    [Fact]
    public void AddJwtTokens_AlgorithmEmpty_Throws()
    {
        var container = new ServiceContainer();

        Wrap.It(() =>
                container.AddJwtTokens(o =>
                {
                    o.SigningKey = new SymmetricSecurityKey(new byte[32]);
                    o.Issuer = "service";
                    // Algorithm intentionally left as default empty string
                })
            )
            .Throws<InvalidOperationException>();
    }

    /// <summary>
    /// When <c>Issuer</c> is left unset (empty string), <see cref="ServiceContainerExtensions.AddJwtTokens{TContainer}"/>
    /// must throw <see cref="InvalidOperationException"/> at registration time.
    /// </summary>
    [Fact]
    public void AddJwtTokens_IssuerEmpty_Throws()
    {
        var container = new ServiceContainer();

        Wrap.It(() =>
                container.AddJwtTokens(o =>
                {
                    o.SigningKey = new SymmetricSecurityKey(new byte[32]);
                    o.Algorithm = SecurityAlgorithms.RsaSha256;
                    // Issuer intentionally left as default empty string
                })
            )
            .Throws<InvalidOperationException>();
    }
}
