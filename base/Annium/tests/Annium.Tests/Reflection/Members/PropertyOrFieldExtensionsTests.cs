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
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    [Fact]
    public void GetPropertyOrFieldType_Property_ReturnsPropertyType()
    {
        var member = typeof(Sample).GetProperty(nameof(Sample.Prop), InstanceFlags)!;
        member.GetPropertyOrFieldType().Is(typeof(int));
    }

    [Fact]
    public void GetPropertyOrFieldType_Field_ReturnsFieldType()
    {
        var member = typeof(Sample).GetField(nameof(Sample.Field), InstanceFlags)!;
        member.GetPropertyOrFieldType().Is(typeof(string));
    }

    [Fact]
    public void GetPropertyOrFieldType_Method_Throws()
    {
        var member = typeof(Sample).GetMethod(nameof(Sample.Method), InstanceFlags)!;
        Wrap.It(() => member.GetPropertyOrFieldType()).Throws<InvalidOperationException>();
    }

    [Fact]
    public void GetPropertyOrFieldValue_Property_ReturnsCurrentValue()
    {
        var sample = new Sample { Prop = 42 };
        var member = typeof(Sample).GetProperty(nameof(Sample.Prop), InstanceFlags)!;
        member.GetPropertyOrFieldValue(sample).Is(42);
    }

    [Fact]
    public void GetPropertyOrFieldValue_Field_ReturnsCurrentValue()
    {
        var sample = new Sample { Field = "abc" };
        var member = typeof(Sample).GetField(nameof(Sample.Field), InstanceFlags)!;
        member.GetPropertyOrFieldValue(sample).Is("abc");
    }

    [Fact]
    public void GetPropertyOrFieldValue_TypedOverload_CastsOrReturnsDefault()
    {
        var sample = new Sample { Prop = 7 };
        var member = typeof(Sample).GetProperty(nameof(Sample.Prop), InstanceFlags)!;
        member.GetPropertyOrFieldValue<int>(sample).Is(7);
        member.GetPropertyOrFieldValue<string>(sample).IsDefault();
    }

    [Fact]
    public void SetPropertyOrFieldValue_Property_SetsValue()
    {
        var sample = new Sample();
        var member = typeof(Sample).GetProperty(nameof(Sample.Prop), InstanceFlags)!;
        member.SetPropertyOrFieldValue(sample, 99);
        sample.Prop.Is(99);
    }

    [Fact]
    public void SetPropertyOrFieldValue_Field_SetsValue()
    {
        var sample = new Sample();
        var member = typeof(Sample).GetField(nameof(Sample.Field), InstanceFlags)!;
        member.SetPropertyOrFieldValue(sample, "set");
        sample.Field.Is("set");
    }

    [Fact]
    public void SetPropertyOrFieldValue_ReadOnlyProperty_Throws()
    {
        var sample = new Sample();
        var member = typeof(Sample).GetProperty(nameof(Sample.ReadOnlyProp), InstanceFlags)!;
        Wrap.It(() => member.SetPropertyOrFieldValue(sample, 1)).Throws<InvalidOperationException>();
    }

    [Fact]
    public void GetDefaultConstructor_NoDefaultCtor_Throws()
    {
        Wrap.It(() => typeof(NoDefault).GetDefaultConstructor()).Throws<ArgumentException>();
    }

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

    private sealed class Sample
    {
        public int Prop { get; set; }
        public string Field = string.Empty;
        public int ReadOnlyProp => 1;

        public void Method() { }
    }

    private sealed class NoDefault
    {
        public NoDefault(int x)
        {
            _ = x;
        }
    }

    private sealed class InternalCtorOnly
    {
        internal InternalCtorOnly() { }
    }
}
