using System.Collections.Generic;
using Annium.Net.Types.Internal.Helpers;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Internals.Helpers;

/// <summary>
/// Tests for array helper functionality
/// </summary>
public class ArrayHelperTest
{
    /// <summary>
    /// Tests resolving element type from array types
    /// </summary>
    [Fact]
    public void Array()
    {
        // arrange
        var target = typeof(int[]).ToContextualType();

        // act
        var elementType = ArrayHelper.ResolveElementType(target);

        // assert
        elementType.Is(typeof(int).ToContextualType());
    }

    /// <summary>
    /// Tests resolving element type from generic enumerable types
    /// </summary>
    [Fact]
    public void Enumerable()
    {
        // arrange
        var target = typeof(IEnumerable<>).ToContextualType();

        // act
        var elementType = ArrayHelper.ResolveElementType(target);

        // assert
        elementType.Type.IsGenericTypeParameter.IsTrue();
        elementType.Type.Name.Is(typeof(IEnumerable<>).GetGenericArguments()[0].Name);
    }

    /// <summary>
    /// Tests resolving element type from array-like collection types
    /// </summary>
    [Fact]
    public void ArrayLike()
    {
        // arrange
        var target = typeof(HashSet<string[]>).ToContextualType();

        // act
        var elementType = ArrayHelper.ResolveElementType(target);

        // assert
        elementType.Type.Is(typeof(string[]));
    }
}
