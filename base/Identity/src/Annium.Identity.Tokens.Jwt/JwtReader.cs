using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Annium.Identity.Tokens.Jwt;

/// <summary>
/// JWT-backed implementation of <see cref="ITokenReader{ClaimsPrincipal}"/>. Configured via
/// <see cref="JwtTokensOptions"/>; current time supplied by <see cref="ITimeProvider"/> for
/// the manual ValidFrom/ValidTo check that runs when <see cref="JwtTokensOptions.ExpirationWindow"/>
/// is null.
/// </summary>
public sealed class JwtReader : ITokenReader<ClaimsPrincipal>
{
    /// <summary>Reader configuration — signing key, issuer, audience, expiration window.</summary>
    private readonly JwtTokensOptions _options;

    /// <summary>Time provider for the manual ValidFrom/ValidTo check.</summary>
    private readonly ITimeProvider _time;

    /// <summary>Per-instance handler — <see cref="JwtSecurityTokenHandler"/> read paths are thread-safe.</summary>
    private readonly JwtSecurityTokenHandler _handler = new();

    public JwtReader(JwtTokensOptions options, ITimeProvider time)
    {
        _options = options;
        _time = time;
    }

    /// <summary>
    /// Reads and validates the token, mapping any failure to a <see cref="TokenReadStatus"/>.
    /// Builds a <see cref="ClaimsPrincipal"/> from the parsed token's claims on success.
    /// </summary>
    /// <param name="token">String-encoded JWT to read.</param>
    /// <returns>The read result.</returns>
    public TokenReadResult<ClaimsPrincipal> Read(string token)
    {
        if (!_handler.CanReadToken(token))
            return new TokenReadResult<ClaimsPrincipal>(TokenReadStatus.Malformed, null, "Token is not valid JWT");

        var validationParameters = BuildValidationParameters();

        try
        {
            _handler.ValidateToken(token, validationParameters, out var securityToken);
            var jwt = (JwtSecurityToken)securityToken;

            // When ValidateLifetime is false (ExpirationWindow == null), MS skips the lifetime
            // check entirely; this code enforces ValidFrom/ValidTo with the configured time.
            if (!validationParameters.ValidateLifetime)
            {
                var nowUtc = _time.Now.ToDateTimeUtc();
                if (jwt.ValidFrom > nowUtc)
                    return new TokenReadResult<ClaimsPrincipal>(
                        TokenReadStatus.NotYetValid,
                        null,
                        "Token is not yet valid"
                    );

                if (jwt.ValidTo <= nowUtc)
                    return new TokenReadResult<ClaimsPrincipal>(TokenReadStatus.Expired, null, "Token is expired");
            }

            var identity = new ClaimsIdentity(jwt.Claims, "JWT");
            var principal = new ClaimsPrincipal(identity);
            return new TokenReadResult<ClaimsPrincipal>(TokenReadStatus.Ok, principal, null);
        }
        catch (Exception ex)
        {
            var (status, message) = MapValidationFailure(ex);
            return new TokenReadResult<ClaimsPrincipal>(status, null, message);
        }
    }

    /// <summary>
    /// Builds <see cref="TokenValidationParameters"/> from the configured options.
    /// </summary>
    /// <returns>The validation parameters.</returns>
    private TokenValidationParameters BuildValidationParameters()
    {
        var parameters = new TokenValidationParameters
        {
            IssuerSigningKey = _options.SigningKey,
            RequireSignedTokens = true,
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateIssuerSigningKey = true,
        };

        if (!string.IsNullOrWhiteSpace(_options.Audience))
        {
            parameters.ValidateAudience = true;
            parameters.ValidAudience = _options.Audience;
        }
        else
        {
            parameters.ValidateAudience = false;
        }

        if (_options.ExpirationWindow.HasValue)
        {
            parameters.ClockSkew = _options.ExpirationWindow.Value.ToTimeSpan();
            parameters.RequireExpirationTime = true;
            parameters.ValidateLifetime = true;
        }
        else
        {
            parameters.RequireExpirationTime = false;
            parameters.ValidateLifetime = false;
        }

        return parameters;
    }

    /// <summary>
    /// Maps the platform-specific exception thrown by <c>JwtSecurityTokenHandler.ValidateToken</c>
    /// to the provider-neutral <see cref="TokenReadStatus"/>.
    /// </summary>
    /// <param name="ex">The exception thrown during validation.</param>
    /// <returns>Tuple of mapped status and human-readable message.</returns>
    private static (TokenReadStatus status, string message) MapValidationFailure(Exception ex) =>
        ex switch
        {
            SecurityTokenExpiredException => (TokenReadStatus.Expired, "Token is expired"),
            SecurityTokenNotYetValidException => (TokenReadStatus.NotYetValid, "Token is not yet valid"),
            // SignatureKeyNotFound derives from InvalidSignature, so the more-specific arm comes first.
            SecurityTokenSignatureKeyNotFoundException => (
                TokenReadStatus.InvalidSignature,
                "Token has invalid signature"
            ),
            SecurityTokenInvalidSignatureException => (TokenReadStatus.InvalidSignature, "Token has invalid signature"),
            SecurityTokenInvalidAudienceException => (TokenReadStatus.InvalidClaims, "Token has invalid audience"),
            SecurityTokenInvalidIssuerException => (TokenReadStatus.InvalidClaims, "Token has invalid issuer"),
            SecurityTokenInvalidLifetimeException => (TokenReadStatus.InvalidClaims, "Token has invalid lifetime"),
            SecurityTokenNoExpirationException => (TokenReadStatus.InvalidClaims, "Token has no expiration claim"),
            SecurityTokenDecompressionFailedException => (TokenReadStatus.Malformed, "Token decompression failed"),
            SecurityTokenEncryptionKeyNotFoundException => (TokenReadStatus.Malformed, "Token decryption failed"),
            SecurityTokenDecryptionFailedException => (TokenReadStatus.Malformed, "Token decryption failed"),
            _ => (TokenReadStatus.Unknown, ex.Message),
        };
}
