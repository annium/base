using System.Collections.Generic;
using Annium.Net.Types.Internal.Helpers;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Internals.Helpers;

public class ArrayHelperTest
{
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