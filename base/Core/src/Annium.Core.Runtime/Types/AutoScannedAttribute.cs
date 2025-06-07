using System;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Runtime.Types;

/// <summary>
/// Includes marked Assembly into scanning process, performed on startup by <see cref="Internal.Types.AssembliesCollector"/>
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class AutoScannedAttribute : Attribute
{
    /// <summary>
    /// The assemblies that this assembly depends on for scanning
    /// </summary>
    public Assembly[] Dependencies { get; }

    /// <summary>
    /// Initializes a new instance of AutoScannedAttribute with the specified dependency types
    /// </summary>
    /// <param name="dependencies">Types whose assemblies should be included as dependencies</param>
    public AutoScannedAttribute(params Type[] dependencies)
    {
        Dependencies = dependencies.Select(x => x.Assembly).Distinct().ToArray();
    }
}
