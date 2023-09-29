using System;
using System.IO;
using System.Linq;
using Annium.NodaTime.Extensions;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Identity.Tokens.Jwt.Tests;

public class JwtReaderWriterTests
{
    [Fact]
    public void Works()
    {
        // arrange
        var privateKey = KeyReader.ReadRsaKey(File.ReadAllText(Path.Combine("keys", "private.key")));
        var publicKey = KeyReader.ReadRsaKey(File.ReadAllText(Path.Combine("keys", "public.key")));
        var tokenId = Guid.NewGuid().ToString();
        var issuer = "service";
        var audience = "audience";
        var now = SystemClock.Instance.GetCurrentInstant().FloorToSecond();
        var nowUtc = now.ToDateTimeUtc();
        var lifetime = Duration.FromSeconds(45);
        var expiresUtc = (now + lifetime).ToDateTimeUtc();
        var key = "sample";
        var data = "g87asgdf";

        // act - write
        var token = JwtWriter.Create(
            privateKey,
            tokenId,
            issuer,
            audience,
            now,
            lifetime,
            (key, data)
        );
        var encoded = token.GetString();

        // assert - write
        token.IsNotDefault();
        token.Id.Is(tokenId);
        token.Issuer.Is(issuer);
        token.Audiences.Has(1);
        token.Audiences.At(0).Is(audience);
        token.IssuedAt.Is(nowUtc);
        token.ValidFrom.Is(nowUtc);
        token.ValidTo.Is(expiresUtc);
        token.Claims
            .FirstOrDefault(x => x.Type == key)
            .IsNotDefault()
            .Value.Is(data);

        // act - read
        var readResult = JwtReader.Read(
            publicKey,
            encoded,
            issuer,
            audience,
            now,
            Duration.FromSeconds(10)
        );

        // assert - read
        readResult.HasErrors.IsFalse();
        var (status, restored) = readResult;
        status.Is(JwtReadStatus.Ok);
        restored.IsT0.IsTrue();
        restored.AsT0.IsNotDefault();
        restored.AsT0.Id.Is(tokenId);
        restored.AsT0.Issuer.Is(issuer);
        restored.AsT0.Audiences.Has(1);
        restored.AsT0.Audiences.At(0).Is(audience);
        restored.AsT0.IssuedAt.Is(nowUtc);
        restored.AsT0.ValidFrom.Is(nowUtc);
        restored.AsT0.ValidTo.Is(expiresUtc);
        restored.AsT0.Claims
            .FirstOrDefault(x => x.Type == key)
            .IsNotDefault()
            .Value.Is(data);
    }
}