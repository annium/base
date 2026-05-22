using System;
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
    private static ServiceContainer CreateContainer() => TestContainerFactory.Create();

    /// <summary>
    /// When configuration data has a non-leaf descendant for a primitive-element list
    /// (<c>[array, 0, garbage]</c> instead of <c>[array, 0]</c>), the processor reaches
    /// <c>ProcessValue</c> with a path that has no exact entry and throws
    /// <see cref="ArgumentException"/>.
    /// </summary>
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
}
