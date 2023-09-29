using System.Collections;
using System.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

public class TypeNameExtensionsTest
{
    [Fact]
    public void PureName_BaseType_Ok()
    {
        typeof(int).PureName().Is("int");
    }

    [Fact]
    public void FriendlyName_BaseType_Ok()
    {
        typeof(int).FriendlyName().Is("int");
    }

    [Fact]
    public void PureName_SimpleType_Ok()
    {
        typeof(IEnumerable).PureName().Is("IEnumerable");
    }

    [Fact]
    public void FriendlyName_SimpleType_Ok()
    {
        typeof(IEnumerable).FriendlyName().Is("IEnumerable");
    }

    [Fact]
    public void PureName_GenericTypeDefinition_Ok()
    {
        typeof(IReadOnlyDictionary<,>).PureName().Is("IReadOnlyDictionary");
    }

    [Fact]
    public void FriendlyName_GenericTypeDefinition_Ok()
    {
        typeof(IReadOnlyDictionary<,>).FriendlyName().Is("IReadOnlyDictionary<TKey, TValue>");
    }

    [Fact]
    public void PureName_GenericType_Ok()
    {
        typeof(IReadOnlyDictionary<string, IList<int?>>).PureName().Is("IReadOnlyDictionary");
    }

    [Fact]
    public void FriendlyName_GenericType_Ok()
    {
        typeof(IReadOnlyDictionary<string, IList<int?>>).FriendlyName().Is("IReadOnlyDictionary<string, IList<int?>>");
    }

    [Fact]
    public void PureName_FileLocalType_Ok()
    {
        typeof(FileLocal<string, IList<int?>>).PureName().Is("FileLocal");
    }

    [Fact]
    public void FriendlyName_FileLocalType_Ok()
    {
        typeof(FileLocal<string, IList<int?>>).FriendlyName().Is("FileLocal<string, IList<int?>>");
    }
}

file record struct FileLocal<TA, TB>;