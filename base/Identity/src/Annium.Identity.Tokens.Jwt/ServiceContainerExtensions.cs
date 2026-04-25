using System;
using System.Security.Claims;
using Annium.Core.DependencyInjection;

namespace Annium.Identity.Tokens.Jwt;

/// <summary>
/// Extension methods for <see cref="IServiceContainer"/> to register JWT token reader/writer
/// services under the provider-neutral <see cref="ITokenReader{TClaims}"/> /
/// <see cref="ITokenWriter{TClaims}"/> abstractions.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers <see cref="ITokenReader{ClaimsPrincipal}"/> and
    /// <see cref="ITokenWriter{ClaimsPrincipal}"/> backed by <see cref="JwtReader"/> /
    /// <see cref="JwtWriter"/>, plus the singleton <see cref="JwtTokensOptions"/> they depend on.
    /// </summary>
    /// <typeparam name="TContainer">Service container type.</typeparam>
    /// <param name="container">The service container.</param>
    /// <param name="configure">Optional delegate to populate <see cref="JwtTokensOptions"/>.</param>
    /// <returns>The container for method chaining.</returns>
    public static TContainer AddJwtTokens<TContainer>(
        this TContainer container,
        Action<JwtTokensOptions>? configure = null
    )
        where TContainer : IServiceContainer
    {
        var options = new JwtTokensOptions();
        configure?.Invoke(options);

        container.Add(options).AsSelf().Singleton();
        container.Add<ITokenReader<ClaimsPrincipal>, JwtReader>().Singleton();
        container.Add<ITokenWriter<ClaimsPrincipal>, JwtWriter>().Singleton();

        return container;
    }
}
