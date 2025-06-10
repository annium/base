using System;
using Annium.Core.DependencyInjection.Descriptors;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests;

/// <summary>
/// Tests for service container functionality
/// </summary>
public class ServiceContainerTest : TestBase
{
    /// <summary>
    /// Verifies that adding a service to the container writes to collection immediately
    /// </summary>
    [Fact]
    public void Add_WritesToCollectionImmediately()
    {
        // arrange
        var instance = new A();

        // act
        Container.Add(instance).AsSelf().Singleton();
        Build();

        // assert
        Get<A>().Is(instance);
    }

    /// <summary>
    /// Verifies that the container correctly identifies when it contains a type descriptor
    /// </summary>
    [Fact]
    public void ContainsType_Works()
    {
        // arrange
        Container.Add<A>().AsSelf().Singleton();

        // assert
        Container.Contains(ServiceDescriptor.Type(typeof(A), typeof(A), ServiceLifetime.Singleton)).IsTrue();
    }

    /// <summary>
    /// Verifies that the container correctly identifies when it contains a factory descriptor
    /// </summary>
    [Fact]
    public void ContainsFactory_Works()
    {
        // arrange
        static B Factory(IServiceProvider _) => new();
        Container.Add(Factory).AsSelf().Singleton();

        // assert
        Container.Contains(ServiceDescriptor.Factory(typeof(B), Factory, ServiceLifetime.Singleton)).IsTrue();
    }

    /// <summary>
    /// Verifies that the container correctly identifies when it contains an instance descriptor
    /// </summary>
    [Fact]
    public void ContainsInstance_Works()
    {
        // arrange
        var instance = new B();
        Container.Add(instance).AsSelf().Singleton();

        // assert
        Container.Contains(ServiceDescriptor.Instance(typeof(B), instance, ServiceLifetime.Singleton)).IsTrue();
    }

    /// <summary>
    /// Test record B that inherits from A
    /// </summary>
    private sealed record B : A;

    /// <summary>
    /// Test record A
    /// </summary>
    private record A;
}
