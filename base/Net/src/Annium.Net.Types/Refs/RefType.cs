namespace Annium.Net.Types.Refs;

/// <summary>
/// Enumeration of all possible type reference categories in the type mapping system.
/// Used for runtime dispatch and resolution of type references.
/// </summary>
public enum RefType
{
    /// <summary>Primitive or built-in types like int, string, etc.</summary>
    BaseType,

    /// <summary>Nullable wrapper types like int?</summary>
    Nullable,

    /// <summary>Generic type parameters like T in List&lt;T&gt;</summary>
    GenericParameter,

    /// <summary>Enumeration types</summary>
    Enum,

    /// <summary>Array and collection types</summary>
    Array,

    /// <summary>Record types for immutable data structures</summary>
    Record,

    /// <summary>Struct and class types</summary>
    Struct,

    /// <summary>Interface types</summary>
    Interface,

    /// <summary>Promise or task-like asynchronous types</summary>
    Promise,
}
