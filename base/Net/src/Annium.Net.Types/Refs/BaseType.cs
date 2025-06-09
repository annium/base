namespace Annium.Net.Types.Refs;

/// <summary>
/// Constants for base type names used in type mapping.
/// These represent primitive and common .NET types in their mapped form.
/// </summary>
public static class BaseType
{
    /// <summary>The base object type</summary>
    public const string Object = "object";

    /// <summary>Boolean type</summary>
    public const string Bool = "bool";

    /// <summary>String type</summary>
    public const string String = "string";

    /// <summary>Unsigned 8-bit integer</summary>
    public const string Byte = "byte";

    /// <summary>Signed 8-bit integer</summary>
    public const string SByte = "sbyte";

    /// <summary>32-bit signed integer</summary>
    public const string Int = "int";

    /// <summary>32-bit unsigned integer</summary>
    public const string UInt = "uint";

    /// <summary>64-bit signed integer</summary>
    public const string Long = "long";

    /// <summary>64-bit unsigned integer</summary>
    public const string ULong = "ulong";

    /// <summary>Single precision floating point</summary>
    public const string Float = "float";

    /// <summary>Double precision floating point</summary>
    public const string Double = "double";

    /// <summary>High precision decimal</summary>
    public const string Decimal = "decimal";

    /// <summary>Globally unique identifier</summary>
    public const string Guid = "guid";

    /// <summary>Date and time</summary>
    public const string DateTime = "dateTime";

    /// <summary>Date and time with timezone offset</summary>
    public const string DateTimeOffset = "dateTimeOffset";

    /// <summary>Date only</summary>
    public const string Date = "date";

    /// <summary>Time only</summary>
    public const string Time = "time";

    /// <summary>Time span or duration</summary>
    public const string TimeSpan = "timeSpan";

    /// <summary>Year and month combination</summary>
    public const string YearMonth = "yearMonth";

    /// <summary>Void return type</summary>
    public const string Void = "void";
}
