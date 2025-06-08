using System.Collections;
using System.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for type name extension methods.
/// </summary>
public class TypeNameExtensionsTest
{
    /// <summary>
    /// Verifies that PureName returns the correct name for base types.
    /// </summary>
    [Fact]
    public void PureName_BaseType_Ok()
    {
        typeof(int).PureName().Is("int");
    }

    /// <summary>
    /// Verifies that FriendlyName returns the correct name for base types.
    /// </summary>
    [Fact]
    public void FriendlyName_BaseType_Ok()
    {
        typeof(int).FriendlyName().Is("int");
    }

    /// <summary>
    /// Verifies that PureName returns the correct name for simple types.
    /// </summary>
    [Fact]
    public void PureName_SimpleType_Ok()
    {
        typeof(IEnumerable).PureName().Is("IEnumerable");
    }

    /// <summary>
    /// Verifies that FriendlyName returns the correct name for simple types.
    /// </summary>
    [Fact]
    public void FriendlyName_SimpleType_Ok()
    {
        typeof(IEnumerable).FriendlyName().Is("IEnumerable");
    }

    /// <summary>
    /// Verifies that PureName returns the correct name for generic type definitions.
    /// </summary>
    [Fact]
    public void PureName_GenericTypeDefinition_Ok()
    {
        typeof(IReadOnlyDictionary<,>).PureName().Is("IReadOnlyDictionary");
    }

    /// <summary>
    /// Verifies that FriendlyName returns the correct name for generic type definitions.
    /// </summary>
    [Fact]
    public void FriendlyName_GenericTypeDefinition_Ok()
    {
        typeof(IReadOnlyDictionary<,>).FriendlyName().Is("IReadOnlyDictionary<TKey, TValue>");
    }

    /// <summary>
    /// Verifies that PureName returns the correct name for generic types.
    /// </summary>
    [Fact]
    public void PureName_GenericType_Ok()
    {
        typeof(IReadOnlyDictionary<string, IList<int?>>).PureName().Is("IReadOnlyDictionary");
    }

    /// <summary>
    /// Verifies that FriendlyName returns the correct name for generic types.
    /// </summary>
    [Fact]
    public void FriendlyName_GenericType_Ok()
    {
        typeof(IReadOnlyDictionary<string, IList<int?>>).FriendlyName().Is("IReadOnlyDictionary<string, IList<int?>>");
    }

    /// <summary>
    /// Verifies that PureName returns the correct name for file-local types.
    /// </summary>
    [Fact]
    public void PureName_FileLocalType_Ok()
    {
        typeof(FileLocal<string, IList<int?>>).PureName().Is("FileLocal");
    }

    /// <summary>
    /// Verifies that FriendlyName returns the correct name for file-local types.
    /// </summary>
    [Fact]
    public void FriendlyName_FileLocalType_Ok()
    {
        typeof(FileLocal<string, IList<int?>>).FriendlyName().Is("FileLocal<string, IList<int?>>");
    }
}

/// <summary>
/// A file-local record struct used for testing type name extensions.
/// </summary>
/// <typeparam name="TA">The first type parameter.</typeparam>
/// <typeparam name="TB">The second type parameter.</typeparam>
file record struct FileLocal<TA, TB>;
