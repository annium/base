using System.IO;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Annium.Identity.Tokens.Jwt.Tests;

public class JwtReaderWriterEcdsaTests : JwtReaderWriterTestsBase
{
    [Fact]
    public void Works()
    {
        var privateKey = ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_private.pem"))).GetKey();
        var publicKey = ECDsa.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "ecdsa_public.pem"))).GetKey();

        Works_Base(privateKey, publicKey, SecurityAlgorithms.EcdsaSha512);
    }
}
