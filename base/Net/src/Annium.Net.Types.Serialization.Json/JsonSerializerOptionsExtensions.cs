using System.Text.Json;
using Annium.Net.Types.Serialization.Json.Internal.Converters;

// ReSharper disable CheckNamespace

namespace Annium.Core.DependencyInjection;

/// <summary>
/// Extension methods for configuring JsonSerializerOptions to support Net.Types serialization.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Configures JsonSerializerOptions to support serialization of Net.Types models.
    /// Adds necessary converters for Namespace and other Net.Types components.
    /// </summary>
    /// <param name="options">The JsonSerializerOptions to configure</param>
    /// <returns>The configured JsonSerializerOptions for method chaining</returns>
    public static JsonSerializerOptions ConfigureForNetTypes(this JsonSerializerOptions options)
    {
        options.Converters.Insert(0, new NamespaceJsonConverter());
        return options;
    }
}
