using System.IdentityModel.Tokens.Jwt;

namespace Annium.Identity.Tokens.Jwt;

public static class JwtSecurityTokenExtensions
{
    public static string GetString(this JwtSecurityToken token)
    {
        var handler = new JwtSecurityTokenHandler();
        var raw = handler.WriteToken(token);

        return raw;
    }
}