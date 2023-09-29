using System;

// ReSharper disable once CheckNamespace
namespace Annium.Testing;

public class AssertionFailedException : Exception
{
    public AssertionFailedException(string message) : base(message)
    {
    }
}