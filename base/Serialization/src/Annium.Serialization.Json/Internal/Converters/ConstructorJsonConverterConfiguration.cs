using System;
using System.Collections.Generic;
using System.Reflection;

namespace Annium.Serialization.Json.Internal.Converters;

internal class ConstructorJsonConverterConfiguration
{
    public ConstructorInfo Constructor { get; }
    public List<ParameterItem> Parameters { get; }
    public IReadOnlyCollection<PropertyInfo> Properties { get; }

    public ConstructorJsonConverterConfiguration(
        ConstructorInfo constructor,
        List<ParameterItem> parameters,
        IReadOnlyCollection<PropertyInfo> properties
    )
    {
        Constructor = constructor;
        Parameters = parameters;
        Properties = properties;
    }

    internal class ParameterItem
    {
        public Type Type { get; }
        public string Name { get; }

        public ParameterItem(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        public override string ToString() => $"{Type.FriendlyName()} {Name}";
    }
}