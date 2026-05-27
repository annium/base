using System;
using System.Collections.Generic;
using Annium.Configuration.Tests.Lib;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Configuration.Abstractions.Tests;

/// <summary>
/// Tests for <c>ServiceContainerExtensions.GetRegisteredProperties</c> exclusion predicates.
/// Verifies that only reference-type non-enum non-primitive non-IEnumerable properties of
/// a configuration type are registered as singletons in the built service provider.
/// </summary>
public class ServiceContainerExtensionsTests
{
    /// <summary>
    /// Creates a fresh <see cref="ServiceContainer"/> for use in each test via the shared test factory.
    /// </summary>
    /// <returns>A new <see cref="ServiceContainer"/> instance.</returns>
    private static ServiceContainer CreateContainer() => TestContainerFactory.Create();

    /// <summary>
    /// After AddConfiguration with a fully populated Config instance, both the root Config and
    /// the nested Val (Nested property — reference type) are resolvable from the provider.
    /// This anchors that the registration ran and that non-excluded properties are registered.
    /// </summary>
    [Fact]
    public void AddConfiguration_ReferenceTypeProperties_AreResolvable()
    {
        var container = CreateContainer();
        var cfg = new Config { Nested = new Val { Plain = 3 } };

        container.AddConfiguration(cfg);

        var sp = container.BuildServiceProvider();
        sp.Resolve<Config>().Nested.Plain.Is(3);
        sp.Resolve<Val>().Plain.Is(3);
    }

    /// <summary>
    /// The SomeConfig abstract reference-type property (Abstract) is also registered,
    /// proving that abstract reference types are not excluded by the predicate.
    /// </summary>
    [Fact]
    public void AddConfiguration_AbstractReferenceTypeProperty_IsResolvable()
    {
        var container = CreateContainer();
        var cfg = new Config { Abstract = new ConfigOne { Value = 7 } };

        container.AddConfiguration(cfg);

        var sp = container.BuildServiceProvider();
        sp.Resolve<SomeConfig>().IsNotDefault();
    }

    /// <summary>
    /// The Plain int property (value type) is excluded by the IsValueType predicate.
    /// Resolving int from the provider throws InvalidOperationException because int
    /// is never registered.
    /// </summary>
    [Fact]
    public void AddConfiguration_ValueTypeProperty_IsNotRegistered()
    {
        var container = CreateContainer();
        container.AddConfiguration(new Config { Plain = 5 });

        var sp = container.BuildServiceProvider();

        Wrap.It(() => sp.Resolve<int>()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// The Enum SomeEnum property is excluded by the IsEnum predicate.
    /// Resolving SomeEnum from the provider throws InvalidOperationException because
    /// enum types are never registered.
    /// </summary>
    [Fact]
    public void AddConfiguration_EnumProperty_IsNotRegistered()
    {
        var container = CreateContainer();
        container.AddConfiguration(new Config());

        var sp = container.BuildServiceProvider();

        Wrap.It(() => sp.Resolve<SomeEnum>()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// The Array int[] property is excluded by the IEnumerable&lt;&gt; predicate.
    /// Resolving int[] from the provider throws InvalidOperationException because
    /// array types implement IEnumerable&lt;T&gt; and are filtered out.
    /// </summary>
    [Fact]
    public void AddConfiguration_ArrayProperty_IsNotRegistered()
    {
        var container = CreateContainer();
        container.AddConfiguration(new Config());

        var sp = container.BuildServiceProvider();

        Wrap.It(() => sp.Resolve<int[]>()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// The List&lt;Val&gt; property is excluded by the IEnumerable&lt;&gt; predicate.
    /// Resolving List&lt;Val&gt; from the provider throws InvalidOperationException because
    /// List&lt;T&gt; implements IEnumerable&lt;T&gt; and is filtered out.
    /// </summary>
    [Fact]
    public void AddConfiguration_ListProperty_IsNotRegistered()
    {
        var container = CreateContainer();
        container.AddConfiguration(new Config());

        var sp = container.BuildServiceProvider();

        Wrap.It(() => sp.Resolve<List<Val>>()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// The Dictionary&lt;string, Val&gt; property is excluded by the IEnumerable&lt;&gt; predicate
    /// (Dictionary implements IEnumerable&lt;KeyValuePair&lt;,&gt;&gt;).
    /// Resolving Dictionary&lt;string, Val&gt; from the provider throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void AddConfiguration_DictionaryProperty_IsNotRegistered()
    {
        var container = CreateContainer();
        container.AddConfiguration(new Config());

        var sp = container.BuildServiceProvider();

        Wrap.It(() => sp.Resolve<Dictionary<string, Val>>()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// A string property is excluded by the IEnumerable&lt;&gt; predicate (string implements
    /// IEnumerable&lt;char&gt;), despite being a non-value, non-enum, non-primitive reference type.
    /// SomeConfig.Type (reached by recursion into the Abstract property) is such a property;
    /// resolving string from the provider throws because string is never registered.
    /// </summary>
    [Fact]
    public void AddConfiguration_StringProperty_IsNotRegistered()
    {
        var container = CreateContainer();
        container.AddConfiguration(new Config { Abstract = new ConfigOne { Value = 1 } });

        var sp = container.BuildServiceProvider();

        Wrap.It(() => sp.Resolve<string>()).Throws<InvalidOperationException>();
    }
}
