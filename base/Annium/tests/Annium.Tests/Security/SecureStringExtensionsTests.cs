using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Annium.Security;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Security;

/// <summary>
/// Contains unit tests for the SecureStringExtensions class.
/// </summary>
public class SecureStringExtensionsTests
{
    /// <summary>
    /// Verifies that encoding a string to SecureString and decoding it back works correctly.
    /// </summary>
    [Fact]
    public void Encode_Decode_Works()
    {
        // arrange
        var source = "sample*$&тест123";

        // encode
        var encoded = source.AsSecureString();

        // decode
        var decoded = Encoding.UTF8.GetString(encoded.AsBytes());

        decoded.Is(source);
    }

    /// <summary>
    /// Verifies that AsSecureString on a non-empty char sequence produces a SecureString whose
    /// plain-text content matches the original.
    /// </summary>
    [Fact]
    public void AsSecureString_CharSequence_ProducesMatchingSecureString()
    {
        // arrange
        const string original = "hello";

        // act
        var secure = ((IEnumerable<char>)original).AsSecureString();

        // assert — unmarshal back to plain string for equality check
        var ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.SecureStringToBSTR(secure);
            var result = Marshal.PtrToStringBSTR(ptr);
            result.Is(original);
        }
        finally
        {
            if (ptr != IntPtr.Zero)
                Marshal.ZeroFreeBSTR(ptr);
        }
    }

    /// <summary>
    /// Verifies that AsSecureString on an empty char sequence produces a SecureString with length zero.
    /// </summary>
    [Fact]
    public void AsSecureString_EmptySequence_ProducesEmptySecureString()
    {
        // arrange
        var empty = Array.Empty<char>();

        // act
        var secure = ((IEnumerable<char>)empty).AsSecureString();

        // assert
        secure.Length.Is(0);
    }
}
