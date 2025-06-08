using System;
using System.IdentityModel.Tokens.Jwt;
using Annium.Data.Operations;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using OneOf;

namespace Annium.Identity.Tokens.Jwt;

/// <summary>
/// Provides functionality for reading and validating JWT tokens
/// </summary>
public static class JwtReader
{
    /// <summary>
    /// Reads and validates a JWT token
    /// </summary>
    /// <param name="raw">The raw JWT token string</param>
    /// <param name="opts">The token validation parameters</param>
    /// <param name="now">The current time for validation</param>
    /// <returns>The result containing either the token or an exception</returns>
    public static IStatusResult<JwtReadStatus, OneOf<JwtSecurityToken, Exception>> Read(
        string raw,
        TokenValidationParameters opts,
        Instant now
    )
    {
        var handler = new JwtSecurityTokenHandler();

        // ensure token is readable
        if (!handler.CanReadToken(raw))
            return Fail(JwtReadStatus.BadSource, "Token is not valid JWT");

        try
        {
            // execute validation
            handler.ValidateToken(raw, opts, out var securityToken);
            var token = (JwtSecurityToken)securityToken;

            // if expiration window has value - expiration is already validated,
            if (opts.ValidateLifetime)
                return Result.Status<JwtReadStatus, OneOf<JwtSecurityToken, Exception>>(JwtReadStatus.Ok, token);

            var nowUtc = now.ToDateTimeUtc();

            // ensure token is already valid
            if (token.ValidFrom > nowUtc)
                return Fail(JwtReadStatus.Failed, "Token is not yet valid");

            return Result.Status<JwtReadStatus, OneOf<JwtSecurityToken, Exception>>(JwtReadStatus.Ok, token);
        }
        catch (Exception exception)
        {
            var (status, error) = HandleValidationFailure(exception);

            return Fail(status, exception, error);
        }
    }

    /// <summary>
    /// Creates token validation parameters for JWT validation
    /// </summary>
    /// <param name="securityKey">The security key for validation</param>
    /// <param name="issuer">The expected issuer</param>
    /// <param name="audience">The expected audience</param>
    /// <param name="expirationWindow">The clock skew tolerance</param>
    /// <returns>The configured validation parameters</returns>
    public static TokenValidationParameters GetValidationParameters(
        SecurityKey securityKey,
        string issuer,
        string? audience,
        Duration? expirationWindow
    )
    {
        var validationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = securityKey,
            RequireSignedTokens = true,
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateIssuerSigningKey = true,
        };

        if (!string.IsNullOrWhiteSpace(audience))
        {
            validationParameters.ValidateAudience = true;
            validationParameters.ValidAudience = audience;
        }
        else
        {
            validationParameters.ValidateAudience = false;
        }

        if (expirationWindow.HasValue)
        {
            validationParameters.ClockSkew = expirationWindow.Value.ToTimeSpan();
            validationParameters.RequireExpirationTime = true;
            validationParameters.ValidateLifetime = true;
        }
        else
        {
            validationParameters.RequireExpirationTime = false;
            validationParameters.ValidateLifetime = false;
        }

        return validationParameters;
    }

    /// <summary>
    /// Handles JWT validation failures by mapping exceptions to status codes and error messages
    /// </summary>
    /// <param name="exception">The exception that occurred during validation</param>
    /// <returns>A tuple containing the status code and error message</returns>
    private static ValueTuple<JwtReadStatus, string> HandleValidationFailure(Exception exception)
    {
        return exception switch
        {
            SecurityTokenDecompressionFailedException => (JwtReadStatus.Failed, "Token decompression failed"),
            SecurityTokenEncryptionKeyNotFoundException => (JwtReadStatus.Failed, "Token decryption failed"),
            SecurityTokenDecryptionFailedException => (JwtReadStatus.Failed, "Token decryption failed"),
            SecurityTokenNoExpirationException => (JwtReadStatus.Failed, "Token has no expiration claim"),
            SecurityTokenExpiredException => (JwtReadStatus.Failed, "Token is expired"),
            SecurityTokenNotYetValidException => (JwtReadStatus.Failed, "Token is not yet valid"),
            SecurityTokenInvalidLifetimeException => (JwtReadStatus.Failed, "Token has invalid lifetime"),
            SecurityTokenInvalidAudienceException => (JwtReadStatus.Failed, "Token has invalid audience"),
            SecurityTokenInvalidIssuerException => (JwtReadStatus.Failed, "Token has invalid issuer"),
            SecurityTokenSignatureKeyNotFoundException => (JwtReadStatus.Failed, "Token has invalid signature"),
            SecurityTokenInvalidSignatureException => (JwtReadStatus.Failed, "Token has invalid signature"),
            _ => (JwtReadStatus.BadSource, "Token is invalid"),
        };
    }

    /// <summary>
    /// Creates a failure result with the specified status and error message
    /// </summary>
    /// <param name="status">The JWT read status indicating the type of failure</param>
    /// <param name="error">The error message describing the failure</param>
    /// <returns>A status result indicating failure with an exception</returns>
    private static IStatusResult<JwtReadStatus, OneOf<JwtSecurityToken, Exception>> Fail(
        JwtReadStatus status,
        string error
    )
    {
        return Result
            .Status<JwtReadStatus, OneOf<JwtSecurityToken, Exception>>(status, new InvalidOperationException(error))
            .Error(error);
    }

    /// <summary>
    /// Creates a failure result with the specified status, exception, and error message
    /// </summary>
    /// <param name="status">The JWT read status indicating the type of failure</param>
    /// <param name="exception">The exception that caused the failure</param>
    /// <param name="error">The error message describing the failure</param>
    /// <returns>A status result indicating failure with the provided exception</returns>
    private static IStatusResult<JwtReadStatus, OneOf<JwtSecurityToken, Exception>> Fail(
        JwtReadStatus status,
        Exception exception,
        string error
    )
    {
        return Result.Status<JwtReadStatus, OneOf<JwtSecurityToken, Exception>>(status, exception).Error(error);
    }
}
