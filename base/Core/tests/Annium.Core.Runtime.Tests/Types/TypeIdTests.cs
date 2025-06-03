using System;
using System.Collections.Generic;
using Annium.Core.Runtime.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Runtime.Tests.Types;

/// <summary>
/// Tests for the TypeId functionality, which provides unique identification for types in the system.
/// </summary>
/// <remarks>
/// These tests verify that TypeId correctly handles different types of .NET types:
/// - Plain types (non-generic)
/// - Constructed generic types (with specific type parameters)
/// - Open generic types (without type parameters)
/// </remarks>
public class TypeIdTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the TypeIdTests class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public TypeIdTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that TypeId works correctly with plain (non-generic) types.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - TypeId is correctly generated for a plain type
    /// - The ID contains the type's namespace and name
    /// - The ID is consistent across multiple calls
    /// - The ID can be parsed back to the original type
    /// </remarks>
    [Fact]
    public void PlainId_Works()
    {
        Assert(typeof(int));
    }

    /// <summary>
    /// Tests that TypeId works correctly with constructed generic types.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - TypeId is correctly generated for a complex generic type
    /// - The ID contains all necessary type information
    /// - The ID is consistent across multiple calls
    /// - The ID can be parsed back to the original type with all generic parameters
    /// </remarks>
    [Fact]
    public void ConstructedGenericId_Works()
    {
        Assert(typeof(Dictionary<string, List<int>>));
    }

    /// <summary>
    /// Tests that TypeId works correctly with open generic types.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - TypeId is correctly generated for an open generic type
    /// - The ID contains the generic type definition
    /// - The ID is consistent across multiple calls
    /// - The ID can be parsed back to the original generic type definition
    /// </remarks>
    [Fact]
    public void OpenGenericId_Works()
    {
        Assert(typeof(Dictionary<,>));
    }

    /// <summary>
    /// Helper method to verify TypeId functionality for a given type.
    /// </summary>
    /// <param name="type">The type to test TypeId functionality for.</param>
    /// <remarks>
    /// Performs the following verifications:
    /// - The generated ID contains the type's namespace and name
    /// - The ID is consistent (same ID is generated for the same type)
    /// - The ID can be parsed back to the original type
    /// - The parsed type matches the original type
    /// </remarks>
    private void Assert(Type type)
    {
        // act
        var id = type.GetTypeId();

        // assert
        id.Id.Contains(type.Namespace!).IsTrue();
        id.Id.Contains(type.Name).IsTrue();
        (id == type.GetTypeId()).IsTrue();
        var tm = Get<ITypeManager>();
        var parsed = TypeId.TryParse(id.Id, tm);
        (parsed == id).IsTrue();
        parsed!.Type.Is(type);
    }
}
