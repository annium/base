using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IAsyncServiceScope : IAsyncDisposable
{
    IServiceProvider ServiceProvider { get; }
}