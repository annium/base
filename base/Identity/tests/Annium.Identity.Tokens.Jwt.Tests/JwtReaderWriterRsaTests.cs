using System.IO;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Annium.Identity.Tokens.Jwt.Tests;

/// <summary>
/// Tests for JWT reader and writer functionality using RSA cryptographic algorithm.
/// Validates JWT token creation, signing, and verification with RSA keys.
/// </summary>
public class JwtReaderWriterRsaTests : JwtReaderWriterTestsBase
{
    /// <summary>
    /// Tests JWT token creation and reading with RSA cryptographic algorithm.
    /// Verifies that tokens signed with RSA private key can be validated with corresponding public key.
    /// </summary>
    [Fact]
    public void Works()
    {
        var privateKey = RSA.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "rsa_private.pem"))).GetKey();
        var publicKey = RSA.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "rsa_public.pem"))).GetKey();

        Works_Base(privateKey, publicKey, SecurityAlgorithms.RsaSha256);
    }
}
