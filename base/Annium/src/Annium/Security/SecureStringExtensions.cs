using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Annium.Security;

/// <summary>
/// Provides extension methods for working with secure strings.
/// </summary>
public static class SecureStringExtensions
{
    /// <summary>
    /// Converts a secure string to a byte array using UTF-8 encoding.
    /// </summary>
    /// <param name="source">The secure string to convert.</param>
    /// <returns>A byte array containing the UTF-8 encoded characters from the secure string.</returns>
    public static byte[] AsBytes(this SecureString source)
    {
        lock (source)
        {
            var length = source.Length;
            var pointer = IntPtr.Zero;
            var symbols = new char[length];

            try
            {
                pointer = Marshal.SecureStringToBSTR(source);
                Marshal.Copy(pointer, symbols, 0, length);

                return Encoding.UTF8.GetBytes(symbols);
            }
            finally
            {
                if (pointer != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(pointer);
                }
            }
        }
    }
}
