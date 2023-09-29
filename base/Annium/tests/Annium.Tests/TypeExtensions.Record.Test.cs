using System;
using System.Collections;
using System.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

public class TypeRecordExtensionsTest
{
    [Fact]
    public void TryGetRecordElementTypes_Ok()
    {
        typeof(string).TryGetRecordElementTypes(out _, out _).IsFalse();
        typeof(Array).TryGetRecordElementTypes(out _, out _).IsFalse();
        typeof(string[]).TryGetRecordElementTypes(out _, out _).IsFalse();
        typeof(IEnumerable).TryGetRecordElementTypes(out _, out _).IsFalse();
        typeof(IEnumerable<int>).TryGetRecordElementTypes(out _, out _).IsFalse();
        typeof(IReadOnlyDictionary<int, string>).TryGetRecordElementTypes(out var keyType, out var valueType).IsTrue();
        keyType.Is(typeof(int));
        valueType.Is(typeof(string));
    }
}