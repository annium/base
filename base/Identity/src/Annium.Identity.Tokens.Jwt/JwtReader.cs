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
        RsaSecurityKey securityKey,
        string raw,
        string issuer,
        string? audience,
        Instant now,
        Duration? expirationWindow
    )
    {
        var handler = new JwtSecurityTokenHandler();

        // ensure token is readable
        if (!handler.CanReadToken(raw))
            return Fail(JwtReadStatus.BadSource, "Token is not valid JWT");

        // define validation parameters
        var validationParameters = GetTokenValidationParameters(
            securityKey,
            issuer,
            audience,
            expirationWindow
        );

        try
        {
            // execute validation
            handler.ValidateToken(raw, validationParameters, out var securityToken);
            var token = (JwtSecurityToken)securityToken;

            // if expiration window has value - expiration is already validated,
            if (expirationWindow.HasValue)
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

    private static TokenValidationParameters GetTokenValidationParameters(
        RsaSecurityKey securityKey,
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
            SecurityTokenDecompressionFailedException _   => (JwtReadStatus.Failed, "Token decompression failed"),
            SecurityTokenEncryptionKeyNotFoundException _ => (JwtReadStatus.Failed, "Token decryption failed"),
            SecurityTokenDecryptionFailedException _      => (JwtReadStatus.Failed, "Token decryption failed"),
            SecurityTokenNoExpirationException _          => (JwtReadStatus.Failed, "Token has no expiration claim"),
            SecurityTokenExpiredException _               => (JwtReadStatus.Failed, "Token is expired"),
            SecurityTokenNotYetValidException _           => (JwtReadStatus.Failed, "Token is not yet valid"),
            SecurityTokenInvalidLifetimeException _       => (JwtReadStatus.Failed, "Token has invalid lifetime"),
            SecurityTokenInvalidAudienceException _       => (JwtReadStatus.Failed, "Token has invalid audience"),
            SecurityTokenInvalidIssuerException _         => (JwtReadStatus.Failed, "Token has invalid issuer"),
            SecurityTokenSignatureKeyNotFoundException _  => (JwtReadStatus.Failed, "Token has invalid signature"),
            SecurityTokenInvalidSignatureException _      => (JwtReadStatus.Failed, "Token has invalid signature"),
            _                                             => (JwtReadStatus.BadSource, "Token is invalid")
        };
    }

    private static IStatusResult<JwtReadStatus, OneOf<JwtSecurityToken, Exception>> Fail(JwtReadStatus status, string error)
    {
        return Result.Status<JwtReadStatus, OneOf<JwtSecurityToken, Exception>>(status, new InvalidOperationException(error)).Error(error);
    }

    private static IStatusResult<JwtReadStatus, OneOf<JwtSecurityToken, Exception>> Fail(JwtReadStatus status, Exception exception, string error)
    {
        return Result.Status<JwtReadStatus, OneOf<JwtSecurityToken, Exception>>(status, exception).Error(error);
    }
}