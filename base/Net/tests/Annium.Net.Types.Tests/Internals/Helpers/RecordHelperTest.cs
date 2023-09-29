using System;
using System.Collections;
using System.Collections.Generic;
using Annium.Net.Types.Internal.Config;
using Annium.Net.Types.Internal.Helpers;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Internals.Helpers;

public class RecordHelperTest
{
    [Theory]
    [InlineData(typeof(IEnumerable))]
    public void NotArrayLike(Type type)
    {
        // arrange
        var target = type.ToContextualType();

        // act
        Wrap.It(() => RecordHelper.ResolveElementType(target))
            .Throws<InvalidOperationException>()
            .Reports($"doesn't implement {MapperConfig.BaseArrayType.FriendlyName()}");
    }

    [Theory]
    [InlineData(typeof(IEnumerable<>))]
    [InlineData(typeof(int[]))]
    [InlineData(typeof(HashSet<string[]>))]
    public void NotRecordLike(Type type)
    {
        // arrange
        var target = type.ToContextualType();

        // act
        Wrap.It(() => RecordHelper.ResolveElementType(target))
            .Throws<InvalidOperationException>()
            .Reports($"doesn't implement {MapperConfig.BaseRecordValueType.FriendlyName()}");
    }

    [Theory]
    [InlineData(typeof(IEnumerable<KeyValuePair<string, int>>))]
    [InlineData(typeof(IDictionary<string, int>))]
    [InlineData(typeof(Dictionary<string, int>))]
    public void Resolved(Type type)
    {
        // arrange
        var target = type.ToContextualType();

        // act
        var (keyType, valueType) = RecordHelper.ResolveElementType(target);

        // assert
        keyType.Type.Is(typeof(string));
        valueType.Type.Is(typeof(int));
    }
}