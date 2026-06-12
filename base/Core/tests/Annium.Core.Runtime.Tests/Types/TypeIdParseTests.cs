using System;
using System.Collections.Generic;
using Annium.Core.Runtime.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Runtime.Tests.Types;

/// <summary>
/// Tests for TypeId.TryParse and TypeId.Create covering round-trips, malformed input, unknown types,
/// and invalid type arguments. These serve as a regression guard for the depth-aware generic-arg
/// parsing fix in SplitGenericArgs.
/// </summary>
public class TypeIdParseTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the TypeIdParseTests class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public TypeIdParseTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that a doubly-nested generic type Dictionary&lt;Dictionary&lt;int,int&gt;,string&gt;
    /// round-trips correctly through TypeId serialisation and TryParse.
    /// This is the primary regression guard for the depth-aware comma-splitting fix.
    /// </summary>
    [Fact]
    public void TryParse_NestedGenericDictionaryOfDictionaryIntIntAndString_RoundTrips()
    {
        // arrange
        var type = typeof(Dictionary<Dictionary<int, int>, string>);
        var tm = Get<ITypeManager>();
        var id = type.GetTypeId();

        // act
        var parsed = TypeId.TryParse(id.Id, tm);

        // assert
        (parsed == id).IsTrue();
        parsed.NotNull().Type.Is(type);
    }

    /// <summary>
    /// Tests that List&lt;Dictionary&lt;int,string&gt;&gt; round-trips correctly through
    /// TypeId serialisation and TryParse.
    /// </summary>
    [Fact]
    public void TryParse_NestedGenericListOfDictionaryIntString_RoundTrips()
    {
        // arrange
        var type = typeof(List<Dictionary<int, string>>);
        var tm = Get<ITypeManager>();
        var id = type.GetTypeId();

        // act
        var parsed = TypeId.TryParse(id.Id, tm);

        // assert
        (parsed == id).IsTrue();
        parsed.NotNull().Type.Is(type);
    }

    /// <summary>
    /// Tests that a malformed id containing '&lt;' but no '&gt;' causes TryParse to throw
    /// ArgumentException with an informative message about invalid format.
    /// </summary>
    [Fact]
    public void TryParse_MalformedIdWithOpenAngleBracketOnly_ThrowsArgumentException()
    {
        // arrange
        var tm = Get<ITypeManager>();
        const string malformedId = "System.Collections.Generic:Dictionary<int,int";

        // assert
        Wrap.It(() => TypeId.TryParse(malformedId, tm)).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that a non-existent type name (no '&lt;' or '&gt;') causes TryParse to return null
    /// rather than throwing.
    /// </summary>
    [Fact]
    public void TryParse_UnknownBaseName_ReturnsNull()
    {
        // arrange
        var tm = Get<ITypeManager>();
        const string unknownId = "No.Such:Type";

        // act
        var result = TypeId.TryParse(unknownId, tm);

        // assert
        (result is null).IsTrue();
    }

    /// <summary>
    /// Tests that creating a TypeId for a generic type parameter throws InvalidOperationException.
    /// Generic parameters (e.g. T in List&lt;T&gt;) do not represent concrete types and cannot
    /// be assigned a stable identity.
    /// </summary>
    [Fact]
    public void Create_GenericParameterType_ThrowsInvalidOperationException()
    {
        // arrange
        var genericParam = typeof(List<>).GetGenericArguments()[0];

        // assert
        Wrap.It(() => TypeId.Create(genericParam)).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Tests that a malformed id where '&gt;' appears before '&lt;' (raIndex &lt; laIndex) causes
    /// TryParse to throw ArgumentException about invalid format.
    /// </summary>
    [Fact]
    public void TryParse_MalformedIdWithCloseBracketBeforeOpen_ThrowsArgumentException()
    {
        // arrange
        var tm = Get<ITypeManager>();
        const string malformedId = "System.Collections.Generic:Dictionary>int,int<";

        // assert
        Wrap.It(() => TypeId.TryParse(malformedId, tm)).Throws<ArgumentException>();
    }
}
