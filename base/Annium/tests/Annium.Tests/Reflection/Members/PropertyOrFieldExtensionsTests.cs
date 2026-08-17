using System;
using System.Reflection;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Members;

/// <summary>
/// Tests for the <c>Reflection/Members/*</c> family — <see cref="GetPropertyOrFieldTypeExtension"/>,
/// <see cref="GetPropertyOrFieldValueExtension"/>, <see cref="SetPropertyOrFieldValueExtension"/>,
/// <see cref="GetDefaultConstructorExtension"/>. Closes the TG7 zero-coverage gap. The
/// <c>GetDefaultConstructor_WithBindingFlags_HonorsFlags</c> test would also have caught the B1 bug
/// (silent <c>bindingFlags</c> drop).
/// </summary>
public class PropertyOrFieldExtensionsTests
{
    /// <summary>
    /// Binding flags used to locate instance members regardless of access modifier.
    /// </summary>
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>
    /// Verifies that <c>GetPropertyOrFieldType</c> returns the declared type of a property member.
    /// </summary>
    [Fact]
    public void GetPropertyOrFieldType_Property_ReturnsPropertyType()
    {
        var member = typeof(Sample).GetProperty(nameof(Sample.Prop), InstanceFlags)!;
        member.GetPropertyOrFieldType().Is(typeof(int));
    }

    /// <summary>
    /// Verifies that <c>GetPropertyOrFieldType</c> returns the declared type of a field member.
    /// </summary>
    [Fact]
    public void GetPropertyOrFieldType_Field_ReturnsFieldType()
    {
        var member = typeof(Sample).GetField(nameof(Sample.Field), InstanceFlags)!;
        member.GetPropertyOrFieldType().Is(typeof(string));
    }

    /// <summary>
    /// Verifies that <c>GetPropertyOrFieldType</c> throws <see cref="InvalidOperationException"/>
    /// when the member is a method rather than a property or field.
    /// </summary>
    [Fact]
    public void GetPropertyOrFieldType_Method_Throws()
    {
        var member = typeof(Sample).GetMethod(nameof(Sample.Method), InstanceFlags)!;
        Wrap.It(() => member.GetPropertyOrFieldType()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that <c>GetPropertyOrFieldValue</c> returns the current runtime value of a property.
    /// </summary>
    [Fact]
    public void GetPropertyOrFieldValue_Property_ReturnsCurrentValue()
    {
        var sample = new Sample { Prop = 42 };
        var member = typeof(Sample).GetProperty(nameof(Sample.Prop), InstanceFlags)!;
        member.GetPropertyOrFieldValue(sample).Is(42);
    }

    /// <summary>
    /// Verifies that <c>GetPropertyOrFieldValue</c> returns the current runtime value of a field.
    /// </summary>
    [Fact]
    public void GetPropertyOrFieldValue_Field_ReturnsCurrentValue()
    {
        var sample = new Sample { Field = "abc" };
        var member = typeof(Sample).GetField(nameof(Sample.Field), InstanceFlags)!;
        member.GetPropertyOrFieldValue(sample).Is("abc");
    }

    /// <summary>
    /// Verifies that the typed overload of <c>GetPropertyOrFieldValue</c> returns the cast value when
    /// the type matches and returns the default when the requested type is incompatible.
    /// </summary>
    [Fact]
    public void GetPropertyOrFieldValue_TypedOverload_CastsOrReturnsDefault()
    {
        var sample = new Sample { Prop = 7 };
        var member = typeof(Sample).GetProperty(nameof(Sample.Prop), InstanceFlags)!;
        member.GetPropertyOrFieldValue<int>(sample).Is(7);
        member.GetPropertyOrFieldValue<string>(sample).IsDefault();
    }

    /// <summary>
    /// Verifies that <c>SetPropertyOrFieldValue</c> writes a new value through a settable property.
    /// </summary>
    [Fact]
    public void SetPropertyOrFieldValue_Property_SetsValue()
    {
        var sample = new Sample();
        var member = typeof(Sample).GetProperty(nameof(Sample.Prop), InstanceFlags)!;
        member.SetPropertyOrFieldValue(sample, 99);
        sample.Prop.Is(99);
    }

    /// <summary>
    /// Verifies that <c>SetPropertyOrFieldValue</c> writes a new value into a public field.
    /// </summary>
    [Fact]
    public void SetPropertyOrFieldValue_Field_SetsValue()
    {
        var sample = new Sample();
        var member = typeof(Sample).GetField(nameof(Sample.Field), InstanceFlags)!;
        member.SetPropertyOrFieldValue(sample, "set");
        sample.Field.Is("set");
    }

    /// <summary>
    /// Verifies that <c>SetPropertyOrFieldValue</c> throws <see cref="InvalidOperationException"/>
    /// when the target property has no setter.
    /// </summary>
    [Fact]
    public void SetPropertyOrFieldValue_ReadOnlyProperty_Throws()
    {
        var sample = new Sample();
        var member = typeof(Sample).GetProperty(nameof(Sample.ReadOnlyProp), InstanceFlags)!;
        Wrap.It(() => member.SetPropertyOrFieldValue(sample, 1)).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that <c>GetDefaultConstructor</c> throws <see cref="ArgumentException"/>
    /// when the type has no accessible default constructor.
    /// </summary>
    [Fact]
    public void GetDefaultConstructor_NoDefaultCtor_Throws()
    {
        Wrap.It(() => typeof(NoDefault).GetDefaultConstructor()).Throws<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <c>TryGetDefaultConstructor</c> returns <see langword="null"/> for an interface
    /// type, which cannot have constructors.
    /// </summary>
    [Fact]
    public void TryGetDefaultConstructor_Interface_ReturnsNull()
    {
        typeof(IDisposable).TryGetDefaultConstructor().IsDefault();
    }

    /// <summary>
    /// Verifies that the <c>(Type, BindingFlags)</c> overload of <c>GetDefaultConstructor</c> actually
    /// honors the binding flags it was given. Catches the B1 bug from review-2026.05.15: the throwing
    /// overload was calling the parameterless <c>TryGetDefaultConstructor()</c> and silently dropping
    /// its <c>bindingFlags</c> argument.
    /// </summary>
    [Fact]
    public void GetDefaultConstructor_WithBindingFlags_HonorsFlags()
    {
        // Sample has only a non-public default ctor — passing Public-only flags must NOT find one.
        Wrap.It(() => typeof(InternalCtorOnly).GetDefaultConstructor(BindingFlags.Public | BindingFlags.Instance))
            .Throws<ArgumentException>();
        // Passing NonPublic flags MUST find it.
        var ctor = typeof(InternalCtorOnly).GetDefaultConstructor(BindingFlags.NonPublic | BindingFlags.Instance);
        ctor.IsNotDefault();
    }

    /// <summary>
    /// Simple fixture type used as the reflection target in property/field/method tests.
    /// </summary>
    private sealed class Sample
    {
        /// <summary>Gets or sets the integer property under test.</summary>
        public int Prop { get; set; }

        /// <summary>A public string field used as a reflection target in field tests.</summary>
        public string Field = string.Empty;

        /// <summary>A read-only property that always returns 1; used to verify set-prevention behaviour.</summary>
        public int ReadOnlyProp => 1;

        /// <summary>A no-op method; used to verify that <c>GetPropertyOrFieldType</c> rejects method members.</summary>
        public void Method() { }
    }

    /// <summary>
    /// Fixture type that has no default (parameterless) constructor; used to verify
    /// that <c>GetDefaultConstructor</c> throws when no default constructor exists.
    /// </summary>
    private sealed class NoDefault
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoDefault"/> class.
        /// </summary>
        /// <param name="x">Unused argument — the type exists only to lack a default constructor.</param>
        public NoDefault(int x)
        {
            _ = x;
        }
    }

    /// <summary>
    /// Fixture type whose only constructor is <see langword="internal"/>; used to verify that
    /// <c>GetDefaultConstructor</c> respects <see cref="BindingFlags"/> when locating it.
    /// </summary>
    private sealed class InternalCtorOnly
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InternalCtorOnly"/> class.
        /// </summary>
        internal InternalCtorOnly() { }
    }
}
