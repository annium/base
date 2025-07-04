﻿using System;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

/// <summary>
/// Attribute to specify allowed values for command line arguments
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ValuesAttribute : BaseAttribute
{
    /// <summary>
    /// Gets the array of allowed values for the argument
    /// </summary>
    public string[] Values { get; }

    public ValuesAttribute(params string[] values)
    {
        Values = values;
    }
}
