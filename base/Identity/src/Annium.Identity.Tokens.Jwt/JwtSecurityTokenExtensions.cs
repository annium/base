using System.IdentityModel.Tokens.Jwt;

namespace Annium.Identity.Tokens.Jwt;

/// <summary>
/// Extension methods for JWT security token operations
/// </summary>
public static class JwtSecurityTokenExtensions
{
    /// <summary>
    /// Converts a JWT security token to its string representation
    /// </summary>
    /// <param name="token">The JWT security token</param>
    /// <returns>The token as a string</returns>
    public static string GetString(this JwtSecurityToken token)
    {
        var handler = new JwtSecurityTokenHandler();
        var raw = handler.WriteToken(token);

        return raw;
    }
}
