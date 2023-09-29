using System.Text.Json;
using Annium.Net.Types.Serialization.Json.Internal.Converters;

// ReSharper disable CheckNamespace

namespace Annium.Core.DependencyInjection;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions ConfigureForNetTypes(
        this JsonSerializerOptions options
    )
    {
        options.Converters.Insert(0, new NamespaceJsonConverter());
        return options;
    }
}