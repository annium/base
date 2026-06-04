using System.Collections.Generic;
using System.ComponentModel;
using Annium.Core.Runtime.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Runtime.Tests.Types;

/// <summary>
/// Tests for guard/error paths exposed by <see cref="ITypeManager"/>.
/// Covers null-argument guards, whitespace/empty guards, and ambiguous-resolution
/// paths that surface as specific exception types.
/// </summary>
public class TypeManagerGuardTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeManagerGuardTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public TypeManagerGuardTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that passing a null key to <see cref="ITypeManager.ResolveByKey"/> throws
    /// <see cref="System.ArgumentNullException"/> before any hierarchy lookup is attempted.
    /// </summary>
    [Fact]
    public void ResolveByKey_NullKey_ThrowsArgumentNullException()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // assert
        Wrap.It(() => manager.ResolveByKey(null!, typeof(GuardBase))).Throws<System.ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that passing a null baseType to <see cref="ITypeManager.ResolveByKey"/> throws
    /// <see cref="System.ArgumentNullException"/> after the key null-check passes.
    /// </summary>
    [Fact]
    public void ResolveByKey_NullBaseType_ThrowsArgumentNullException()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // assert
        Wrap.It(() => manager.ResolveByKey("someKey", null!)).Throws<System.ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that passing a null signature to <see cref="ITypeManager.ResolveBySignature"/> throws
    /// <see cref="System.ArgumentNullException"/> before any resolution is attempted.
    /// </summary>
    [Fact]
    public void ResolveBySignature_NullSignature_ThrowsArgumentNullException()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // assert
        Wrap.It(() => manager.ResolveBySignature(null!, typeof(GuardBase))).Throws<System.ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that passing a null baseType to <see cref="ITypeManager.ResolveBySignature"/> throws
    /// <see cref="System.ArgumentNullException"/> after the signature null-check passes.
    /// </summary>
    [Fact]
    public void ResolveBySignature_NullBaseType_ThrowsArgumentNullException()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // assert
        Wrap.It(() => manager.ResolveBySignature(new[] { "prop" }, null!)).Throws<System.ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that <see cref="ITypeManager.ResolveBySignature"/> with <c>exact:true</c>
    /// throws <see cref="TypeResolutionException"/> when two descendants have the same
    /// match score for the supplied signature (ambiguous resolution).
    /// </summary>
    [Fact]
    public void ResolveBySignature_ExactTrueWithAmbiguousDescendants_ThrowsTypeResolutionException()
    {
        // arrange
        var manager = Get<ITypeManager>();
        // Both AmbiguousLeft and AmbiguousRight expose exactly the same property name ("SharedProp"),
        // so their signatures tie: score = 1*100 - 1 = 99 each.
        var signature = new[] { nameof(AmbiguousLeft.SharedProp) };

        // assert
        Wrap.It(() => manager.ResolveBySignature(signature, typeof(AmbiguousBase), exact: true))
            .Throws<TypeResolutionException>();
    }

    /// <summary>
    /// Verifies that <see cref="ITypeManager.GetTypeId"/> throws
    /// <see cref="InvalidEnumArgumentException"/> when given an empty string.
    /// </summary>
    [Fact]
    public void GetTypeId_EmptyString_ThrowsInvalidEnumArgumentException()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // assert
        Wrap.It(() => manager.GetTypeId(string.Empty)).Throws<InvalidEnumArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="ITypeManager.GetTypeId"/> throws
    /// <see cref="InvalidEnumArgumentException"/> when given a whitespace-only string.
    /// </summary>
    [Fact]
    public void GetTypeId_WhitespaceString_ThrowsInvalidEnumArgumentException()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // assert
        Wrap.It(() => manager.GetTypeId("   ")).Throws<InvalidEnumArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="ITypeManager.GetTypeId"/> returns a <see cref="TypeId"/> whose
    /// <see cref="TypeId.Type"/> property equals the type that was originally registered under
    /// that ID, confirming the round-trip through the type registry.
    /// </summary>
    [Fact]
    public void GetTypeId_ValidId_ReturnsTypeIdMatchingRegisteredType()
    {
        // arrange
        var manager = Get<ITypeManager>();
        var expectedType = typeof(int);
        var knownId = expectedType.GetTypeId();

        // act
        var result = manager.GetTypeId(knownId.Id);

        // assert
        result.IsNotNull();
        result.NotNull().Type.Is(expectedType);
    }

    /// <summary>
    /// Verifies that passing null to <see cref="ITypeManager.HasImplementations"/> throws
    /// <see cref="System.ArgumentNullException"/> before any hierarchy lookup is attempted.
    /// </summary>
    [Fact]
    public void HasImplementations_NullBaseType_ThrowsArgumentNullException()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // assert
        Wrap.It(() => manager.HasImplementations(null!)).Throws<System.ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that passing null to <see cref="ITypeManager.GetImplementations"/> throws
    /// <see cref="System.ArgumentNullException"/> before any hierarchy lookup is attempted.
    /// </summary>
    [Fact]
    public void GetImplementations_NullBaseType_ThrowsArgumentNullException()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // assert
        Wrap.It(() => manager.GetImplementations(null!)).Throws<System.ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that passing a null instance to <see cref="ITypeManager.Resolve"/> throws
    /// <see cref="System.ArgumentNullException"/> before any resolution is attempted.
    /// </summary>
    [Fact]
    public void Resolve_NullInstance_ThrowsArgumentNullException()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // assert
        Wrap.It(() => manager.Resolve(null!, typeof(GuardBase))).Throws<System.ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that passing a null baseType to <see cref="ITypeManager.Resolve"/> throws
    /// <see cref="System.ArgumentNullException"/> after the instance null-check passes.
    /// </summary>
    [Fact]
    public void Resolve_NullBaseType_ThrowsArgumentNullException()
    {
        // arrange
        var manager = Get<ITypeManager>();
        var instance = new GuardBase();

        // assert
        Wrap.It(() => manager.Resolve(instance, null!)).Throws<System.ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that <see cref="ITypeManager.GetResolutionKeyProperty"/> returns the
    /// <see cref="System.Reflection.PropertyInfo"/> for a property decorated with
    /// <see cref="ResolutionKeyAttribute"/>.
    /// </summary>
    [Fact]
    public void GetResolutionKeyProperty_TypeWithResolutionKeyAttribute_ReturnsProperty()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // act
        var property = manager.GetResolutionKeyProperty(typeof(KeyedBase));

        // assert
        property.IsNotNull();
        property.NotNull().Name.Is(nameof(KeyedBase.Kind));
    }

    /// <summary>
    /// Verifies that <see cref="ITypeManager.GetResolutionKeyProperty"/> returns null for a type
    /// that has no property decorated with <see cref="ResolutionKeyAttribute"/>.
    /// </summary>
    [Fact]
    public void GetResolutionKeyProperty_TypeWithoutAttribute_ReturnsNull()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // act
        var property = manager.GetResolutionKeyProperty(typeof(GuardBase));

        // assert
        property.IsNull();
    }

    /// <summary>
    /// Verifies that <see cref="ITypeManager.ResolveByKey"/> returns null — rather than
    /// throwing — when the base type has a <see cref="ResolutionKeyAttribute"/> property but
    /// none of its registered descendants carries a key value equal to the queried key.
    /// </summary>
    [Fact]
    public void ResolveByKey_KeyWithNoMatchingDescendant_ReturnsNull()
    {
        // arrange
        var manager = Get<ITypeManager>();
        // "unknown-key" is not assigned to any descendant of KeyedBase via [ResolutionKeyValue].
        var result = manager.ResolveByKey("unknown-key", typeof(KeyedBase));

        // assert — no match → null, not an exception
        result.IsNull();
    }
}

/// <summary>
/// Base class used exclusively in <see cref="TypeManagerGuardTests"/> guard-path tests.
/// Not used for key-based or ID-based resolution; it intentionally has no
/// <see cref="ResolutionKeyAttribute"/> so that <c>ResolveByKey</c> guard checks
/// (null key / null baseType) fire before any hierarchy lookup.
/// </summary>
file class GuardBase;

/// <summary>
/// Shared base for the ambiguous-signature pair used in
/// <see cref="TypeManagerGuardTests.ResolveBySignature_ExactTrueWithAmbiguousDescendants_ThrowsTypeResolutionException"/>.
/// </summary>
file class AmbiguousBase;

/// <summary>
/// First descendant of <see cref="AmbiguousBase"/>.
/// Exposes exactly one property whose lowercased name equals that of <see cref="AmbiguousRight.SharedProp"/>,
/// so both descendants produce the same match score when the query signature is <c>["SharedProp"]</c>.
/// </summary>
file class AmbiguousLeft : AmbiguousBase
{
    /// <summary>
    /// Shared property name — identical to <see cref="AmbiguousRight.SharedProp"/> so the two
    /// descendants have equal signatures and therefore tie during ambiguous-resolution checking.
    /// </summary>
    public int SharedProp { get; set; }
}

/// <summary>
/// Second descendant of <see cref="AmbiguousBase"/>.
/// Mirror of <see cref="AmbiguousLeft"/> — same single property, same match score, forcing a tie.
/// </summary>
file class AmbiguousRight : AmbiguousBase
{
    /// <summary>
    /// Shared property name — identical to <see cref="AmbiguousLeft.SharedProp"/> so the two
    /// descendants have equal signatures and therefore tie during ambiguous-resolution checking.
    /// </summary>
    public int SharedProp { get; set; }
}

/// <summary>
/// Base type with a <see cref="ResolutionKeyAttribute"/>-decorated property.
/// Used by <c>GetResolutionKeyProperty_*</c> and <c>ResolveByKey_KeyWithNoMatchingDescendant_ReturnsNull</c>
/// tests to exercise the key-property lookup path.
/// </summary>
file class KeyedBase
{
    /// <summary>
    /// The resolution-key property. Each concrete descendant supplies its own string value.
    /// </summary>
    [ResolutionKey]
    public string Kind { get; }

    protected KeyedBase(string kind)
    {
        Kind = kind;
    }
}

/// <summary>
/// Single concrete descendant of <see cref="KeyedBase"/> registered under the key "alpha".
/// Only one descendant is registered so there is never an ambiguous match; querying any other
/// key value against <see cref="KeyedBase"/> will return null.
/// </summary>
[ResolutionKeyValue("alpha")]
file class KeyedAlpha : KeyedBase
{
    public KeyedAlpha()
        : base("alpha") { }
}
