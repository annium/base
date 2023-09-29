using System;
using System.Linq;
using System.Reflection;

namespace Annium.Extensions.Arguments.Internal;

internal class ConfigurationProcessor : IConfigurationProcessor
{
    public (PropertyInfo property, TAttribute attribute)[] GetPropertiesWithAttribute<TAttribute>(Type type)
        where TAttribute : BaseAttribute => type.GetProperties()
        .Select(e => (property: e, attribute: e.GetCustomAttribute<TAttribute>()!))
        .Where(e => e.property.CanWrite && e.attribute != null!)
        .ToArray();
}