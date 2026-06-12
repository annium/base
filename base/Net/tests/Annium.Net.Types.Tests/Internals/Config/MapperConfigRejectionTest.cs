using System;
using System.Collections.Generic;
using Annium.Net.Types.Internal.Config;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Types.Tests.Internals.Config;

/// <summary>
/// Tests for the rejection guards in MapperConfig.SetRecord and MapperConfig.SetBaseType
/// </summary>
public class MapperConfigRejectionTest
{
    // ── SetRecord ─────────────────────────────────────────────────────────────

    /// <summary>
    /// SetRecord with a closed generic type (e.g. List&lt;string&gt;) throws because the pure form differs
    /// </summary>
    [Fact]
    public void SetRecord_ClosedGenericType_Throws()
    {
        var config = new MapperConfig();

        // List<string> is a closed generic — TryGetPure returns List<>, so type != type.TryGetPure()
        Wrap.It(() => config.SetRecord(typeof(List<string>)))
            .Throws<ArgumentException>()
            .Reports("Can't register type");
    }

    /// <summary>
    /// SetRecord with the open generic IList&lt;&gt; (IEnumerable&lt;T&gt;, not IEnumerable&lt;KeyValuePair&lt;,&gt;&gt;) throws
    /// because it does not implement IEnumerable&lt;KeyValuePair&lt;,&gt;&gt;
    /// </summary>
    [Fact]
    public void SetRecord_OpenGenericIList_Throws()
    {
        var config = new MapperConfig();

        // IList<> is pure (IsInterface, TryGetPure returns itself); implements IEnumerable<T> not IEnumerable<KeyValuePair<,>>
        // The error message uses the private _baseRecordType which is IEnumerable<KeyValuePair<,>>;
        // match on the stable substring that covers both the type name and the message intent
        Wrap.It(() => config.SetRecord(typeof(IList<>)))
            .Throws<ArgumentException>()
            .Reports($"doesn't implement IEnumerable<{MapperConfig.BaseRecordValueType.FriendlyName()}>");
    }

    /// <summary>
    /// SetRecord with the open generic HashSet&lt;&gt; (IEnumerable&lt;T&gt;, not IEnumerable&lt;KeyValuePair&lt;,&gt;&gt;) throws
    /// </summary>
    [Fact]
    public void SetRecord_OpenGenericHashSet_Throws()
    {
        var config = new MapperConfig();

        // HashSet<> is pure (IsClass, TryGetPure returns itself); implements IEnumerable<T> not IEnumerable<KeyValuePair<,>>
        Wrap.It(() => config.SetRecord(typeof(HashSet<>)))
            .Throws<ArgumentException>()
            .Reports($"doesn't implement IEnumerable<{MapperConfig.BaseRecordValueType.FriendlyName()}>");
    }

    /// <summary>
    /// SetRecord with the open generic Dictionary&lt;,&gt; (implements IEnumerable&lt;KeyValuePair&lt;,&gt;&gt;) succeeds and IsRecord returns true
    /// </summary>
    [Fact]
    public void SetRecord_OpenGenericDictionary_SucceedsAndIsRecordIsTrue()
    {
        var config = new MapperConfig();

        // Dictionary<,> is the open generic type — it is its own "pure" form
        config.SetRecord(typeof(Dictionary<,>));

        // IsRecord checks GetPure() internally, so both open and closed forms match
        config.IsRecord(typeof(Dictionary<,>)).IsTrue();
        config.IsRecord(typeof(Dictionary<string, int>)).IsTrue();
    }

    /// <summary>
    /// SetRecord called twice for the same open generic Dictionary type throws on the second call
    /// </summary>
    [Fact]
    public void SetRecord_DuplicateOpenGenericType_Throws()
    {
        var config = new MapperConfig();
        config.SetRecord(typeof(Dictionary<,>));

        Wrap.It(() => config.SetRecord(typeof(Dictionary<,>)))
            .Throws<ArgumentException>()
            .Reports("already registered as Record type");
    }

    // ── SetBaseType ───────────────────────────────────────────────────────────

    /// <summary>
    /// SetBaseType with an interface type throws (interfaces are neither class nor struct)
    /// </summary>
    [Fact]
    public void SetBaseType_InterfaceType_Throws()
    {
        var config = new MapperConfig();

        Wrap.It(() => config.SetBaseType(typeof(IComparable), "IComparable"))
            .Throws<ArgumentException>()
            .Reports("is neither class nor struct");
    }

    /// <summary>
    /// SetBaseType with an open generic type (e.g. List&lt;&gt;) throws
    /// </summary>
    [Fact]
    public void SetBaseType_OpenGenericType_Throws()
    {
        var config = new MapperConfig();

        Wrap.It(() => config.SetBaseType(typeof(List<>), "List"))
            .Throws<ArgumentException>()
            .Reports("is generic type");
    }

    /// <summary>
    /// SetBaseType with a closed generic type (e.g. List&lt;int&gt;) also throws (IsGenericType is true for closed generics)
    /// </summary>
    [Fact]
    public void SetBaseType_ClosedGenericType_Throws()
    {
        var config = new MapperConfig();

        Wrap.It(() => config.SetBaseType(typeof(List<int>), "ListInt"))
            .Throws<ArgumentException>()
            .Reports("is generic type");
    }

    /// <summary>
    /// SetBaseType with a generic type parameter (e.g. the T in List&lt;T&gt;) throws
    /// </summary>
    [Fact]
    public void SetBaseType_GenericTypeParameter_Throws()
    {
        var config = new MapperConfig();
        var typeParam = typeof(List<>).GetGenericArguments()[0]; // T — IsGenericTypeParameter == true

        Wrap.It(() => config.SetBaseType(typeParam, "T"))
            .Throws<ArgumentException>()
            .Reports("is generic type parameter");
    }

    /// <summary>
    /// SetBaseType called twice for the same concrete type throws on the second call
    /// </summary>
    [Fact]
    public void SetBaseType_DuplicateType_Throws()
    {
        var config = new MapperConfig();
        config.SetBaseType(typeof(string), "string");

        Wrap.It(() => config.SetBaseType(typeof(string), "string2"))
            .Throws<ArgumentException>()
            .Reports("is already registered");
    }

    /// <summary>
    /// SetBaseType with a concrete non-generic class succeeds and IsBaseType returns true afterward
    /// </summary>
    [Fact]
    public void SetBaseType_ConcreteClass_SucceedsAndIsBaseTypeIsTrue()
    {
        var config = new MapperConfig();

        config.SetBaseType(typeof(string), "string");

        config.IsBaseType(typeof(string)).IsTrue();
    }
}
