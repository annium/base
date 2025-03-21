using System;
using System.IdentityModel.Tokens.Jwt;
using Annium.Data.Operations;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using OneOf;

namespace Annium.Identity.Tokens.Jwt;

public static class JwtReader
{
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

    private static IStatusResult<JwtReadStatus, OneOf<JwtSecurityToken, Exception>> Fail(
        JwtReadStatus status,
        string error
    )
    {
        return Result
            .Status<JwtReadStatus, OneOf<JwtSecurityToken, Exception>>(status, new InvalidOperationException(error))
            .Error(error);
    }

    private static IStatusResult<JwtReadStatus, OneOf<JwtSecurityToken, Exception>> Fail(
        JwtReadStatus status,
        Exception exception,
        string error
    )
    {
        return Result.Status<JwtReadStatus, OneOf<JwtSecurityToken, Exception>>(status, exception).Error(error);
    }
}
