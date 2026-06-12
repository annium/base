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
    /// <remarks>
    /// Requires an <c>ITimeProvider</c> to be registered (e.g. via <c>AddTime()...SetDefault()</c> from
    /// Annium.Core.Runtime) before the provider is built — <see cref="JwtReader"/> and <see cref="JwtWriter"/>
    /// inject it as the token clock. Registration order is irrelevant; the dependency is resolved at build time.
    /// </remarks>
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

        if (options.SigningKey is null)
            throw new InvalidOperationException(
                "JwtTokensOptions.SigningKey must be configured: pass a configure delegate to AddJwtTokens that sets SigningKey."
            );

        if (string.IsNullOrWhiteSpace(options.Algorithm))
            throw new InvalidOperationException(
                "JwtTokensOptions.Algorithm must be configured: pass a configure delegate to AddJwtTokens that sets Algorithm."
            );

        if (string.IsNullOrWhiteSpace(options.Issuer))
            throw new InvalidOperationException(
                "JwtTokensOptions.Issuer must be configured: pass a configure delegate to AddJwtTokens that sets Issuer."
            );

        container.Add(options).AsSelf().Singleton();
        container.Add<ITokenReader<ClaimsPrincipal>, JwtReader>().Singleton();
        container.Add<ITokenWriter<ClaimsPrincipal>, JwtWriter>().Singleton();

        return container;
    }
}
