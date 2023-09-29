using System;
using System.Globalization;
using NodaTime;

namespace Annium.Core.Mapper.Internal.Profiles;

internal class DefaultProfile : Profile
{
    public DefaultProfile()
    {
        RegisterString();
        RegisterByte();
        RegisterSbyte();
        RegisterChar();
        RegisterDecimal();
        RegisterDouble();
        RegisterFloat();
        RegisterInt();
        RegisterUint();
        RegisterLong();
        RegisterUlong();
        RegisterShort();
        RegisterUshort();
        RegisterNodaTime();
    }

    private void RegisterString()
    {
        // from
        Map<string, bool>(x => bool.Parse(x));
        Map<string, int>(x => int.Parse(x));
        Map<string, uint>(x => uint.Parse(x));
        Map<string, long>(x => long.Parse(x));
        Map<string, ulong>(x => ulong.Parse(x));
        Map<string, float>(x => float.Parse(x));
        Map<string, double>(x => double.Parse(x));
        Map<string, decimal>(x => decimal.Parse(x));
        // generic, commonly used types
        Map<string, Guid>(x => Guid.Parse(x));
        Map<string, Uri>(x => new Uri(x));
        // date/time built-in types
        Map<string, DateTime>(x => DateTime.Parse(x));
        Map<string, DateTimeOffset>(x => DateTimeOffset.Parse(x));
        Map<string, DateOnly>(x => DateOnly.Parse(x));
        Map<string, TimeSpan>(x => TimeSpan.Parse(x));
        Map<string, TimeOnly>(x => TimeOnly.Parse(x));

        // to
        Map<bool, string>(x => x.ToString());
        Map<int, string>(x => x.ToString());
        Map<uint, string>(x => x.ToString());
        Map<long, string>(x => x.ToString());
        Map<ulong, string>(x => x.ToString());
        Map<float, string>(x => x.ToString(CultureInfo.CurrentUICulture));
        Map<double, string>(x => x.ToString(CultureInfo.CurrentUICulture));
        Map<decimal, string>(x => x.ToString(CultureInfo.CurrentUICulture));
        // generic, commonly used types
        Map<Guid, string>(x => x.ToString());
        Map<Uri, string>(x => x.ToString());
        // date/time built-in types
        Map<DateTime, string>(x => x.ToString(CultureInfo.CurrentUICulture));
        Map<DateTimeOffset, string>(x => x.ToString());
        Map<DateOnly, string>(x => x.ToString());
        Map<TimeSpan, string>(x => x.ToString());
        Map<TimeOnly, string>(x => x.ToString());
    }

    private void RegisterByte()
    {
        //from
        Map<byte, sbyte>(x => (sbyte)x);
        Map<byte, char>(x => (char)x);
        Map<byte, decimal>(x => x);
        Map<byte, double>(x => x);
        Map<byte, float>(x => x);
        Map<byte, int>(x => x);
        Map<byte, uint>(x => x);
        Map<byte, long>(x => x);
        Map<byte, ulong>(x => x);
        Map<byte, short>(x => x);
        Map<byte, ushort>(x => x);

        //to
        Map<sbyte, byte>(x => (byte)x);
        Map<char, byte>(x => (byte)x);
        Map<decimal, byte>(x => (byte)x);
        Map<double, byte>(x => (byte)x);
        Map<float, byte>(x => (byte)x);
        Map<int, byte>(x => (byte)x);
        Map<uint, byte>(x => (byte)x);
        Map<long, byte>(x => (byte)x);
        Map<ulong, byte>(x => (byte)x);
        Map<short, byte>(x => (byte)x);
        Map<ushort, byte>(x => (byte)x);
    }

    private void RegisterSbyte()
    {
        //from
        Map<sbyte, byte>(x => (byte)x);
        Map<sbyte, char>(x => (char)x);
        Map<sbyte, decimal>(x => x);
        Map<sbyte, double>(x => x);
        Map<sbyte, float>(x => x);
        Map<sbyte, int>(x => x);
        Map<sbyte, uint>(x => (uint)x);
        Map<sbyte, long>(x => x);
        Map<sbyte, ulong>(x => (ulong)x);
        Map<sbyte, short>(x => x);
        Map<sbyte, ushort>(x => (ushort)x);

        //to
        Map<byte, sbyte>(x => (sbyte)x);
        Map<char, sbyte>(x => (sbyte)x);
        Map<decimal, sbyte>(x => (sbyte)x);
        Map<double, sbyte>(x => (sbyte)x);
        Map<float, sbyte>(x => (sbyte)x);
        Map<int, sbyte>(x => (sbyte)x);
        Map<uint, sbyte>(x => (sbyte)x);
        Map<long, sbyte>(x => (sbyte)x);
        Map<ulong, sbyte>(x => (sbyte)x);
        Map<short, sbyte>(x => (sbyte)x);
        Map<ushort, sbyte>(x => (sbyte)x);
    }

    private void RegisterChar()
    {
        //from
        Map<char, byte>(x => (byte)x);
        Map<char, sbyte>(x => (sbyte)x);
        Map<char, decimal>(x => x);
        Map<char, double>(x => x);
        Map<char, float>(x => x);
        Map<char, int>(x => x);
        Map<char, uint>(x => x);
        Map<char, long>(x => x);
        Map<char, ulong>(x => x);
        Map<char, short>(x => (short)x);
        Map<char, ushort>(x => x);

        //to
        Map<byte, char>(x => (char)x);
        Map<sbyte, char>(x => (char)x);
        Map<decimal, char>(x => (char)x);
        Map<double, char>(x => (char)x);
        Map<float, char>(x => (char)x);
        Map<int, char>(x => (char)x);
        Map<uint, char>(x => (char)x);
        Map<long, char>(x => (char)x);
        Map<ulong, char>(x => (char)x);
        Map<short, char>(x => (char)x);
        Map<ushort, char>(x => (char)x);
    }

    private void RegisterDecimal()
    {
        //from
        Map<decimal, byte>(x => (byte)x);
        Map<decimal, sbyte>(x => (sbyte)x);
        Map<decimal, char>(x => (char)x);
        Map<decimal, double>(x => (double)x);
        Map<decimal, float>(x => (float)x);
        Map<decimal, int>(x => (int)x);
        Map<decimal, uint>(x => (uint)x);
        Map<decimal, long>(x => (long)x);
        Map<decimal, ulong>(x => (ulong)x);
        Map<decimal, short>(x => (short)x);
        Map<decimal, ushort>(x => (ushort)x);

        //to
        Map<byte, decimal>(x => x);
        Map<sbyte, decimal>(x => x);
        Map<char, decimal>(x => x);
        Map<double, decimal>(x => (decimal)x);
        Map<float, decimal>(x => (decimal)x);
        Map<int, decimal>(x => x);
        Map<uint, decimal>(x => x);
        Map<long, decimal>(x => x);
        Map<ulong, decimal>(x => x);
        Map<short, decimal>(x => x);
        Map<ushort, decimal>(x => x);
    }

    private void RegisterDouble()
    {
        //from
        Map<double, byte>(x => (byte)x);
        Map<double, sbyte>(x => (sbyte)x);
        Map<double, char>(x => (char)x);
        Map<double, decimal>(x => (decimal)x);
        Map<double, float>(x => (float)x);
        Map<double, int>(x => (int)x);
        Map<double, uint>(x => (uint)x);
        Map<double, long>(x => (long)x);
        Map<double, ulong>(x => (ulong)x);
        Map<double, short>(x => (short)x);
        Map<double, ushort>(x => (ushort)x);

        //to
        Map<byte, double>(x => x);
        Map<sbyte, double>(x => x);
        Map<char, double>(x => x);
        Map<decimal, double>(x => (double)x);
        Map<float, double>(x => x);
        Map<int, double>(x => x);
        Map<uint, double>(x => x);
        Map<long, double>(x => x);
        Map<ulong, double>(x => x);
        Map<short, double>(x => x);
        Map<ushort, double>(x => x);
    }

    private void RegisterFloat()
    {
        //from
        Map<float, byte>(x => (byte)x);
        Map<float, sbyte>(x => (sbyte)x);
        Map<float, char>(x => (char)x);
        Map<float, decimal>(x => (decimal)x);
        Map<float, double>(x => x);
        Map<float, int>(x => (int)x);
        Map<float, uint>(x => (uint)x);
        Map<float, long>(x => (long)x);
        Map<float, ulong>(x => (ulong)x);
        Map<float, short>(x => (short)x);
        Map<float, ushort>(x => (ushort)x);

        //to
        Map<byte, float>(x => x);
        Map<sbyte, float>(x => x);
        Map<char, float>(x => x);
        Map<decimal, float>(x => (float)x);
        Map<double, float>(x => (float)x);
        Map<int, float>(x => x);
        Map<uint, float>(x => x);
        Map<long, float>(x => x);
        Map<ulong, float>(x => x);
        Map<short, float>(x => x);
        Map<ushort, float>(x => x);
    }

    private void RegisterInt()
    {
        //from
        Map<int, byte>(x => (byte)x);
        Map<int, sbyte>(x => (sbyte)x);
        Map<int, char>(x => (char)x);
        Map<int, decimal>(x => x);
        Map<int, double>(x => x);
        Map<int, float>(x => x);
        Map<int, uint>(x => (uint)x);
        Map<int, long>(x => x);
        Map<int, ulong>(x => (ulong)x);
        Map<int, short>(x => (short)x);
        Map<int, ushort>(x => (ushort)x);

        //to
        Map<byte, int>(x => x);
        Map<sbyte, int>(x => x);
        Map<char, int>(x => x);
        Map<decimal, int>(x => (int)x);
        Map<double, int>(x => (int)x);
        Map<float, int>(x => (int)x);
        Map<uint, int>(x => (int)x);
        Map<long, int>(x => (int)x);
        Map<ulong, int>(x => (int)x);
        Map<short, int>(x => x);
        Map<ushort, int>(x => x);
    }

    private void RegisterUint()
    {
        //from
        Map<uint, byte>(x => (byte)x);
        Map<uint, sbyte>(x => (sbyte)x);
        Map<uint, char>(x => (char)x);
        Map<uint, decimal>(x => x);
        Map<uint, double>(x => x);
        Map<uint, float>(x => x);
        Map<uint, int>(x => (int)x);
        Map<uint, long>(x => x);
        Map<uint, ulong>(x => x);
        Map<uint, short>(x => (short)x);
        Map<uint, ushort>(x => (ushort)x);

        //to
        Map<byte, uint>(x => x);
        Map<sbyte, uint>(x => (uint)x);
        Map<char, uint>(x => x);
        Map<decimal, uint>(x => (uint)x);
        Map<double, uint>(x => (uint)x);
        Map<float, uint>(x => (uint)x);
        Map<int, uint>(x => (uint)x);
        Map<long, uint>(x => (uint)x);
        Map<ulong, uint>(x => (uint)x);
        Map<short, uint>(x => (uint)x);
        Map<ushort, uint>(x => x);
    }

    private void RegisterLong()
    {
        //from
        Map<long, byte>(x => (byte)x);
        Map<long, sbyte>(x => (sbyte)x);
        Map<long, char>(x => (char)x);
        Map<long, decimal>(x => x);
        Map<long, double>(x => x);
        Map<long, float>(x => x);
        Map<long, int>(x => (int)x);
        Map<long, uint>(x => (uint)x);
        Map<long, ulong>(x => (ulong)x);
        Map<long, short>(x => (short)x);
        Map<long, ushort>(x => (ushort)x);

        //to
        Map<byte, long>(x => x);
        Map<sbyte, long>(x => x);
        Map<char, long>(x => x);
        Map<decimal, long>(x => (long)x);
        Map<double, long>(x => (long)x);
        Map<float, long>(x => (long)x);
        Map<int, long>(x => x);
        Map<uint, long>(x => x);
        Map<ulong, long>(x => (long)x);
        Map<short, long>(x => x);
        Map<ushort, long>(x => x);
    }

    private void RegisterUlong()
    {
        //from
        Map<ulong, byte>(x => (byte)x);
        Map<ulong, sbyte>(x => (sbyte)x);
        Map<ulong, char>(x => (char)x);
        Map<ulong, decimal>(x => x);
        Map<ulong, double>(x => x);
        Map<ulong, float>(x => x);
        Map<ulong, int>(x => (int)x);
        Map<ulong, uint>(x => (uint)x);
        Map<ulong, long>(x => (long)x);
        Map<ulong, short>(x => (short)x);
        Map<ulong, ushort>(x => (ushort)x);

        //to
        Map<byte, ulong>(x => x);
        Map<sbyte, ulong>(x => (ulong)x);
        Map<char, ulong>(x => x);
        Map<decimal, ulong>(x => (ulong)x);
        Map<double, ulong>(x => (ulong)x);
        Map<float, ulong>(x => (ulong)x);
        Map<int, ulong>(x => (ulong)x);
        Map<uint, ulong>(x => x);
        Map<long, ulong>(x => (ulong)x);
        Map<short, ulong>(x => (ulong)x);
        Map<ushort, ulong>(x => x);
    }

    private void RegisterShort()
    {
        //from
        Map<short, byte>(x => (byte)x);
        Map<short, sbyte>(x => (sbyte)x);
        Map<short, char>(x => (char)x);
        Map<short, decimal>(x => x);
        Map<short, double>(x => x);
        Map<short, float>(x => x);
        Map<short, int>(x => x);
        Map<short, uint>(x => (uint)x);
        Map<short, long>(x => x);
        Map<short, ulong>(x => (ulong)x);
        Map<short, ushort>(x => (ushort)x);

        //to
        Map<byte, short>(x => x);
        Map<sbyte, short>(x => x);
        Map<char, short>(x => (short)x);
        Map<decimal, short>(x => (short)x);
        Map<double, short>(x => (short)x);
        Map<float, short>(x => (short)x);
        Map<int, short>(x => (short)x);
        Map<uint, short>(x => (short)x);
        Map<long, short>(x => (short)x);
        Map<ulong, short>(x => (short)x);
        Map<ushort, short>(x => (short)x);
    }

    private void RegisterUshort()
    {
        //from
        Map<ushort, byte>(x => (byte)x);
        Map<ushort, sbyte>(x => (sbyte)x);
        Map<ushort, char>(x => (char)x);
        Map<ushort, decimal>(x => x);
        Map<ushort, double>(x => x);
        Map<ushort, float>(x => x);
        Map<ushort, int>(x => x);
        Map<ushort, uint>(x => x);
        Map<ushort, long>(x => x);
        Map<ushort, ulong>(x => x);
        Map<ushort, short>(x => (short)x);

        //to
        Map<byte, ushort>(x => x);
        Map<sbyte, ushort>(x => (ushort)x);
        Map<char, ushort>(x => x);
        Map<decimal, ushort>(x => (ushort)x);
        Map<double, ushort>(x => (ushort)x);
        Map<float, ushort>(x => (ushort)x);
        Map<int, ushort>(x => (ushort)x);
        Map<uint, ushort>(x => (ushort)x);
        Map<long, ushort>(x => (ushort)x);
        Map<ulong, ushort>(x => (ushort)x);
        Map<short, ushort>(x => (ushort)x);
    }

    private void RegisterNodaTime()
    {
        // to noda time

        // from string
        Map<string, Instant>(x => Instant.FromUnixTimeMilliseconds(long.Parse(x)));
        Map<string, Duration>(x => Duration.FromTimeSpan(TimeSpan.Parse(x)));
        Map<string, IsoDayOfWeek>(x => x.ParseEnum<IsoDayOfWeek>());
        Map<string, LocalDate>(ctx => x => ctx.Map<LocalDate>(DateOnly.Parse(x)));
        Map<string, LocalTime>(ctx => x => ctx.Map<LocalTime>(TimeOnly.Parse(x)));
        // from built-in date/time types
        Map<DateTime, Instant>(d => Instant.FromDateTimeUtc(d.ToUniversalTime()));
        Map<DateTimeOffset, Instant>(d => Instant.FromDateTimeOffset(d));
        Map<DateOnly, LocalDate>(x => new LocalDate(x.Year, x.Month, x.Day));
        Map<TimeOnly, LocalTime>(x => new LocalTime(x.Hour, x.Minute, x.Second, x.Millisecond));

        // from noda time

        // to string
        Map<Instant, string>(x => x.ToString());
        Map<Duration, string>(x => x.ToString());
        Map<IsoDayOfWeek, string>(x => x.ToString());
        Map<LocalDate, string>(x => x.ToString());
        Map<LocalTime, string>(x => x.ToString());
        // to built-in date/time types
        Map<Instant, DateTime>(i => i.ToDateTimeUtc());
        Map<Instant, DateTimeOffset>(i => i.ToDateTimeOffset());
        Map<LocalDate, DateOnly>(x => new DateOnly(x.Year, x.Month, x.Day));
        Map<LocalTime, TimeOnly>(x => new TimeOnly(x.Hour, x.Minute, x.Second, x.Millisecond));
    }
}