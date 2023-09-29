using System;
using System.Reflection;

namespace Annium.Extensions.Arguments.Internal;

internal interface IConfigurationProcessor
{
    (PropertyInfo property, TAttribute attribute)[] GetPropertiesWithAttribute<TAttribute>(Type type)
        where TAttribute : BaseAttribute;
}