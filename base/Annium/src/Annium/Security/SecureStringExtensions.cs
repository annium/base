using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Annium.Security;

public static class SecureStringExtensions
{
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
