using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Configuration.Tests.Lib;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Configuration.Abstractions.Tests;

/// <summary>
/// Edge-case tests for <c>ConfigurationProcessor</c> and the <c>KeyComparer</c> used by
/// <c>ConfigurationContainer</c>. These cover defensive throw paths that were previously
/// unverified by the suite.
/// </summary>
public class ProcessorEdgeCaseTests
{
    /// <summary>
    /// Creates a fresh <see cref="ServiceContainer"/> pre-configured with the standard
    /// test environment (runtime, logging, configuration abstractions).
    /// </summary>
    /// <returns>A new <see cref="ServiceContainer"/> ready for configuration registration.</returns>
    private static ServiceContainer CreateContainer() => TestContainerFactory.Create();

    /// <summary>
    /// When configuration data has a non-leaf descendant for a primitive-element list
    /// (<c>[array, 0, garbage]</c> instead of <c>[array, 0]</c>), the processor reaches
    /// <c>ProcessValue</c> with a path that has no exact entry and throws
    /// <see cref="ArgumentException"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Process_MissingRequiredLeafKey_ThrowsArgumentException()
    {
        var container = CreateContainer();
        var malformed = new Dictionary<string[], string>
        {
            // GetDescendants("array") returns "0"; pushed path becomes ["array", "0"]; processor
            // calls Process(typeof(int)) → ProcessValue → TryGetValue(["array","0"]) is false
            // because only the deeper key exists.
            [new[] { "array", "0", "garbage" }] = "5",
        };

        await container.AddConfigurationAsync<Config>(cfg => cfg.Add(malformed), TestContext.Current.CancellationToken);
        var sp = container.BuildServiceProvider();

        // Build<T> runs lazily inside the singleton factory; throw fires on first resolve.
        Wrap.It(() => sp.Resolve<Config>()).Throws<ArgumentException>();
    }

    /// <summary>
    /// When the target abstract type has no property marked with <c>[ResolutionKey]</c>, the
    /// processor cannot pick a concrete implementation and throws <see cref="ArgumentException"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Process_AbstractTypeWithoutResolutionKey_ThrowsArgumentException()
    {
        var container = CreateContainer();
        // No ResolutionKey-marked property on AbstractWithoutResolutionKey → processor cannot
        // resolve which concrete type to materialize.
        var data = new Dictionary<string[], string> { [new[] { "leaf", "value" }] = "1" };

        await container.AddConfigurationAsync<RootHoldingAbstract>(
            cfg => cfg.Add(data),
            TestContext.Current.CancellationToken
        );
        var sp = container.BuildServiceProvider();

        // Build<T> runs lazily inside the singleton factory; throw fires on first resolve.
        Wrap.It(() => sp.Resolve<RootHoldingAbstract>()).Throws<ArgumentException>();
    }

    /// <summary>
    /// Two keys that differ only in case fold to the same bucket — the later <c>Add</c> wins.
    /// </summary>
    [Fact]
    public void Add_KeysDifferingOnlyInCase_LastWins()
    {
        var container = ConfigurationFactory.CreateContainer();
        container.Add(new Dictionary<string[], string> { [new[] { "Plain" }] = "first" });
        container.Add(new Dictionary<string[], string> { [new[] { "plain" }] = "second" });

        var data = container.Get();

        data.Count.Is(1);
        data.At(new[] { "Plain" }).Is("second");
        data.At(new[] { "plain" }).Is("second");
    }

    /// <summary>
    /// The internal <c>KeyComparer</c> hashes case variants to the same bucket, which is the
    /// invariant that makes <see cref="Add_KeysDifferingOnlyInCase_LastWins"/> work. We
    /// indirectly verify this by exercising the bucket-collision behavior via the public API:
    /// two adds with different casing produce a single dictionary entry.
    /// </summary>
    [Fact]
    public void KeyComparer_GetHashCode_SameForCaseVariants()
    {
        var container = ConfigurationFactory.CreateContainer();
        container.Add(
            new Dictionary<string[], string>
            {
                [new[] { "section", "Plain" }] = "v1",
                [new[] { "section", "plain" }] = "v2",
            }
        );

        // Both entries fold to the same bucket; only one survives.
        container.Get().Count.Is(1);
    }

    /// <summary>
    /// Wrapper type holding an abstract member whose runtime type cannot be selected because
    /// <see cref="AbstractWithoutResolutionKey"/> exposes no <c>[ResolutionKey]</c> property.
    /// </summary>
    public sealed record RootHoldingAbstract
    {
        /// <summary>The abstract leaf the processor will try to resolve.</summary>
        public AbstractWithoutResolutionKey Leaf { get; set; } = new ConcreteOne();
    }

    /// <summary>
    /// Abstract record with no <c>[ResolutionKey]</c>-marked property — the processor will
    /// throw <see cref="ArgumentException"/> when asked to materialize this.
    /// </summary>
    public abstract record AbstractWithoutResolutionKey
    {
        /// <summary>A plain string value used by concrete implementations.</summary>
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>Concrete subtype of <see cref="AbstractWithoutResolutionKey"/>.</summary>
    public sealed record ConcreteOne : AbstractWithoutResolutionKey;

    /// <summary>
    /// Reading an object configuration where a <c>Nullable&lt;T&gt;</c> value-type property carries a
    /// non-null value exercises the <c>TypeVariant.Nullable</c> unwrap branch in
    /// <c>ObjectConfigurationProvider</c>. The flattened result must carry the underlying value
    /// as a string.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Read_ObjectWithNonNullNullableField_FlattensProperly()
    {
        var container = CreateContainer();
        var sentinel = new Config { Nullable = 99m };

        await container.AddConfigurationAsync<Config>(cfg => cfg.Add(sentinel), TestContext.Current.CancellationToken);

        var resolved = container.BuildServiceProvider().Resolve<Config>();
        resolved.Nullable.IsNotDefault();
        resolved.Nullable.Value.Is(99m);
    }

    /// <summary>
    /// The sync <c>AddConfiguration&lt;T&gt;(T instance)</c> overload registers the configuration
    /// AND its nested object properties as singletons resolvable from the built provider.
    /// </summary>
    [Fact]
    public void AddConfiguration_SyncOverload_RegistersConfigAndNestedSingleton()
    {
        var container = CreateContainer();
        var cfg = new Config
        {
            Plain = 5,
            Nested = new Val { Plain = 3 },
        };

        container.AddConfiguration(cfg);

        var sp = container.BuildServiceProvider();
        sp.Resolve<Config>().Plain.Is(5);
        sp.Resolve<Val>().Plain.Is(3);
    }

    /// <summary>
    /// When the resolution key field value does not map to any registered concrete type
    /// (<c>ResolveByKey</c> returns null), the processor throws <see cref="ArgumentException"/>.
    /// Exercised via a wrapper type because <c>AddConfigurationAsync&lt;T&gt;</c> requires
    /// <c>T : new()</c> and abstract types are barred at the entry point.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Process_AbstractTypeWithUnknownResolutionKeyValue_ThrowsArgumentException()
    {
        var container = CreateContainer();
        // RootHoldingSomeConfig.Inner is abstract SomeConfig with [ResolutionKey] string Type.
        // SomeConfig has concrete subtypes ConfigOne / ConfigTwo (via [ResolutionKeyValue]).
        // "UnknownVariant" matches neither → ResolveByKey returns null → ArgumentException.
        var data = new Dictionary<string[], string> { [new[] { "inner", "type" }] = "UnknownVariant" };

        await container.AddConfigurationAsync<RootHoldingSomeConfig>(
            cfg => cfg.Add(data),
            TestContext.Current.CancellationToken
        );
        var sp = container.BuildServiceProvider();

        Wrap.It(() => sp.Resolve<RootHoldingSomeConfig>()).Throws<ArgumentException>();
    }

    /// <summary>Wrapper type holding an abstract <see cref="SomeConfig"/> member.</summary>
    public sealed record RootHoldingSomeConfig
    {
        /// <summary>The abstract member; resolved by its [ResolutionKey] Type property.</summary>
        public SomeConfig Inner { get; set; } = new ConfigOne();
    }

    /// <summary>
    /// The sync <c>AddConfiguration&lt;T&gt;(T instance)</c> overload recurses through nested
    /// property types: a two-level-deep nested record is reachable from the built provider.
    /// </summary>
    [Fact]
    public void AddConfiguration_DeepNestedProperties_AllResolvable()
    {
        var container = CreateContainer();
        var root = new RootCfg { A = new Level1Cfg { B = new Level2Cfg { Value = 42 } } };

        container.AddConfiguration(root);

        var sp = container.BuildServiceProvider();
        sp.Resolve<RootCfg>().A.B.Value.Is(42);
        sp.Resolve<Level1Cfg>().B.Value.Is(42);
        sp.Resolve<Level2Cfg>().Value.Is(42);
    }

    /// <summary>Two-level config root.</summary>
    public sealed record RootCfg
    {
        /// <summary>Level 1 property.</summary>
        public Level1Cfg A { get; set; } = new();
    }

    /// <summary>Two-level config inner.</summary>
    public sealed record Level1Cfg
    {
        /// <summary>Level 2 property.</summary>
        public Level2Cfg B { get; set; } = new();
    }

    /// <summary>Two-level config leaf.</summary>
    public sealed record Level2Cfg
    {
        /// <summary>Leaf value.</summary>
        public int Value { get; set; }
    }

    /// <summary>
    /// When a dictionary property on a config object has an entry whose key is null,
    /// <c>ObjectConfigurationProvider.ProcessDictionary</c> throws
    /// <see cref="InvalidOperationException"/> with a message containing "key is null".
    /// Standard <c>Dictionary&lt;TKey,TValue&gt;</c> forbids null keys, so the test supplies a
    /// minimal custom <see cref="IReadOnlyDictionary{TKey,TValue}"/> whose enumerator yields
    /// a <see cref="KeyValuePair{TKey,TValue}"/> with a null Key — the only way to reach the guard.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ProcessDictionary_NullKey_ThrowsInvalidOperationException()
    {
        var container = ConfigurationFactory.CreateContainer();
        var instance = new CfgWithNullKeyDict { Data = new NullKeyDictionary<string>("value") };

        container.Add(instance);

        var ex = await Wrap.It(async () => await container.BuildAsync(TestContext.Current.CancellationToken))
            .ThrowsAsync<AggregateException>();
        ex.InnerExceptions.Has(1);
        var inner = ex.InnerExceptions[0].As<InvalidOperationException>();
        inner
            .Message.Contains("key is null")
            .IsTrue($"expected message containing 'key is null'; got: {inner.Message}");
    }

    /// <summary>Config type whose <see cref="Data"/> property is an <see cref="IReadOnlyDictionary{TKey,TValue}"/>
    /// — the provider detects it via the interface check and routes to <c>ProcessDictionary</c>.</summary>
    public sealed class CfgWithNullKeyDict
    {
        /// <summary>Dictionary property that will be handed to ProcessDictionary.</summary>
        public IReadOnlyDictionary<string?, string> Data { get; set; } = new NullKeyDictionary<string>(string.Empty);
    }

    /// <summary>
    /// Minimal <see cref="IReadOnlyDictionary{TKey,TValue}"/> implementation whose only enumerator
    /// entry has a null key. Used solely to exercise the null-key guard in
    /// <c>ObjectConfigurationProvider.ProcessDictionary</c>.
    /// </summary>
    private sealed class NullKeyDictionary<TValue> : IReadOnlyDictionary<string?, TValue>
    {
        /// <summary>The single value returned for every lookup and the sole element in the enumerator.</summary>
        private readonly TValue _value;

        public NullKeyDictionary(TValue value) => _value = value;

        /// <summary>Always 1 — this dictionary contains exactly one entry with a null key.</summary>
        public int Count => 1;

        /// <summary>Returns a sequence containing the single key: <c>null</c>.</summary>
        public IEnumerable<string?> Keys => new string?[] { null };

        /// <summary>Returns a sequence containing the single stored value.</summary>
        public IEnumerable<TValue> Values => new[] { _value };

        /// <summary>Returns the stored value regardless of <paramref name="key"/>.</summary>
        public TValue this[string? key] => _value;

        /// <summary>
        /// Returns <see langword="true"/> only when <paramref name="key"/> is <see langword="null"/>,
        /// which is the only key this dictionary contains.
        /// </summary>
        /// <param name="key">The key to test.</param>
        /// <returns><see langword="true"/> if <paramref name="key"/> is <see langword="null"/>; otherwise <see langword="false"/>.</returns>
        public bool ContainsKey(string? key) => key is null;

        /// <summary>
        /// Attempts to retrieve the stored value. Always outputs <see cref="_value"/> and
        /// returns <see langword="true"/> only when <paramref name="key"/> is <see langword="null"/>.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="value">Set to the stored value on every call.</param>
        /// <returns><see langword="true"/> if <paramref name="key"/> is <see langword="null"/>; otherwise <see langword="false"/>.</returns>
        public bool TryGetValue(string? key, out TValue value)
        {
            value = _value;
            return key is null;
        }

        /// <summary>
        /// Returns an enumerator over the single entry <c>(null, value)</c>.
        /// </summary>
        /// <returns>An enumerator yielding the single null-keyed pair.</returns>
        public IEnumerator<KeyValuePair<string?, TValue>> GetEnumerator()
        {
            yield return new KeyValuePair<string?, TValue>(null, _value);
        }

        /// <summary>
        /// Returns a non-generic enumerator over the single entry <c>(null, value)</c>.
        /// </summary>
        /// <returns>A non-generic enumerator delegating to <see cref="GetEnumerator"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Self-referential config type — <c>AddConfiguration</c>'s recursive
    /// <c>Register</c> must use a visited-set to avoid <see cref="System.StackOverflowException"/>.
    /// Removing the visited guard at <c>ServiceContainerExtensions.Register</c> would crash here.
    /// </summary>
    [Fact]
    public void AddConfiguration_SelfReferentialType_NoStackOverflow()
    {
        var container = CreateContainer();
        var instance = new SelfRefCfg { Value = 1 };

        container.AddConfiguration(instance);

        var sp = container.BuildServiceProvider();
        sp.Resolve<SelfRefCfg>().Value.Is(1);
    }

    /// <summary>
    /// Config type with a property of its own type — the visited-set guard prevents
    /// infinite recursion when <c>Register</c> walks nested properties.
    /// </summary>
    public sealed class SelfRefCfg
    {
        /// <summary>Leaf value.</summary>
        public int Value { get; set; }

        /// <summary>Self-reference: same type as the enclosing class.</summary>
        public SelfRefCfg? Self { get; set; }
    }

    /// <summary>
    /// Round-trip a list with 12 elements through the object provider + processor pipeline.
    /// Without the numeric sort fix in <c>ProcessList</c>, index "10" would land before "2"
    /// (lexicographic) and the reconstructed list would have wrong ordering.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Process_ListWith12Elements_OrdersNumerically()
    {
        var container = CreateContainer();
        var expected = new[] { 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200, 210 };
        var instance = new BigListCfg { Items = [.. expected] };

        await container.AddConfigurationAsync<BigListCfg>(
            cfg => cfg.Add(instance),
            TestContext.Current.CancellationToken
        );
        var sp = container.BuildServiceProvider();

        var resolved = sp.Resolve<BigListCfg>();
        resolved.Items.Has(12);
        for (var i = 0; i < expected.Length; i++)
            resolved.Items[i].Is(expected[i]);
    }

    /// <summary>Config holding a 12+-element list of integers for numeric-index ordering verification.</summary>
    public sealed record BigListCfg
    {
        /// <summary>The list under test.</summary>
        public List<int> Items { get; set; } = new();
    }

    /// <summary>
    /// A null entry in an enumerable source value is silently skipped by
    /// <c>ObjectConfigurationProvider.ProcessEnumerable</c>. Index is only incremented
    /// for non-null items, so the resulting flat key set compacts nulls out (no gap
    /// indices in the reconstructed list).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Process_ListWithNullEntry_CompactsOutNullEntries()
    {
        var container = CreateContainer();
        var instance = new NullableListCfg { Items = ["a", null, "c"] };

        await container.AddConfigurationAsync<NullableListCfg>(
            cfg => cfg.Add(instance),
            TestContext.Current.CancellationToken
        );
        var sp = container.BuildServiceProvider();

        var resolved = sp.Resolve<NullableListCfg>();
        // Null at source-index 1 is skipped before the index counter increments → the
        // reconstructed list has 2 elements, "a" and "c", with no null in the middle.
        resolved.Items.Has(2);
        resolved.Items[0].Is("a");
        resolved.Items[1].Is("c");
    }

    /// <summary>Config holding a list with nullable elements for enumerable-null-skip verification.</summary>
    public sealed record NullableListCfg
    {
        /// <summary>The list under test (may include nulls).</summary>
        public List<string?> Items { get; set; } = new();
    }

    /// <summary>
    /// A non-integer key at a list path is malformed source data. <c>ProcessList</c>'s
    /// <c>int.TryParse</c> guard throws <see cref="InvalidOperationException"/> with the
    /// offending key and path — not a bare <c>FormatException</c>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Process_ListWithNonIntegerIndex_ThrowsInvalidOperationException()
    {
        var container = CreateContainer();
        // Raw flattened data with a non-integer segment under the "items" list path.
        var data = new Dictionary<string[], string> { [new[] { "items", "notAnIndex" }] = "5" };

        await container.AddConfigurationAsync<BigListCfg>(cfg => cfg.Add(data), TestContext.Current.CancellationToken);
        var sp = container.BuildServiceProvider();

        var ex = Wrap.It(() => sp.Resolve<BigListCfg>()).Throws<InvalidOperationException>();
        ex.Message.Contains("notAnIndex").IsTrue($"expected message naming the bad index; got: {ex.Message}");
    }

    /// <summary>
    /// Properties absent from the configuration retain their constructor defaults — the
    /// <c>if (KeyExists())</c> guard in <c>ProcessObject</c> skips them rather than forcing
    /// <c>Process</c> (which would throw on the missing leaf). Supplying only <c>Plain</c>
    /// leaves <c>Nested</c> / <c>List</c> / <c>Array</c> at their defaults.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Process_PartialConfig_AbsentPropertiesRetainDefaults()
    {
        var container = CreateContainer();
        var data = new Dictionary<string[], string> { [new[] { "plain" }] = "5" };

        await container.AddConfigurationAsync<Config>(cfg => cfg.Add(data), TestContext.Current.CancellationToken);
        var sp = container.BuildServiceProvider();

        var resolved = sp.Resolve<Config>();
        resolved.Plain.Is(5);
        // Absent keys keep constructor defaults instead of throwing on missing leaves.
        resolved.Nested.Plain.Is(0);
        resolved.List.IsEmpty();
        resolved.Array.IsEmpty();
    }
}
