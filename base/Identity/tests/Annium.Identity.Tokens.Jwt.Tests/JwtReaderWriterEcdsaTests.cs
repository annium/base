using System.IO;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Annium.Identity.Tokens.Jwt.Tests;

/// <summary>
/// Tests for JWT reader and writer functionality using ECDSA cryptographic algorithm.
/// Validates JWT token creation, signing, and verification with ECDSA keys.
/// </summary>
public class JwtReaderWriterEcdsaTests : JwtReaderWriterTestsBase
{
    /// <summary>
    /// Tests JWT token creation and reading with ECDSA cryptographic algorithm.
    /// Verifies that tokens signed with ECDSA private key can be validated with corresponding public key.
    /// </summary>
    [Fact]
    public void Works()
    {
        var privateKey = ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_private.pem"))).GetKey();
        var publicKey = ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_public.pem"))).GetKey();

        Works_Base(privateKey, publicKey, SecurityAlgorithms.EcdsaSha512);
    }
}
