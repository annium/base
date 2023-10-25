using System.IO;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Annium.Identity.Tokens.Jwt.Tests;

public class JwtReaderWriterRsaTests : JwtReaderWriterTestsBase
{
    [Fact]
    public void Works()
    {
        var privateKey = RSA.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "rsa_private.pem"))).GetKey();
        var publicKey = RSA.Create().ImportPem(File.ReadAllText(Path.Combine("keys", "rsa_public.pem"))).GetKey();

        Works_Base(privateKey, publicKey, SecurityAlgorithms.RsaSha256);
    }
}