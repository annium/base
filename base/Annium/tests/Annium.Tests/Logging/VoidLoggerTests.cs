using System;
using System.Collections.Generic;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Logging;

/// <summary>
/// Smoke tests for the documented null-logger singleton: Instance is a singleton; Log and Error
/// silently no-op across all levels and exception shapes.
/// </summary>
public class VoidLoggerTests
{
    /// <summary>
    /// Two reads of <see cref="VoidLogger.Instance"/> return the same reference.
    /// </summary>
    [Fact]
    public void Instance_IsSingleton()
    {
        ReferenceEquals(VoidLogger.Instance, VoidLogger.Instance).IsTrue();
    }

    /// <summary>
    /// <c>Log</c> at every level must complete without throwing.
    /// </summary>
    [Fact]
    public void Log_AllLevels_DoesNotThrow()
    {
        ILogger logger = VoidLogger.Instance;
        var data = new List<object?>();

        // Reaching here without an exception is the assertion.
        foreach (var level in new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error })
            logger.Log(this, "file.cs", "Member", 42, level, "msg", data);
    }

    /// <summary>
    /// <c>Error</c> with both populated and null-message exceptions must complete without throwing.
    /// </summary>
    [Fact]
    public void Error_WithException_DoesNotThrow()
    {
        ILogger logger = VoidLogger.Instance;
        var data = new List<object?>();

        logger.Error(this, "file.cs", "Member", 42, new InvalidOperationException("boom"), data);
        logger.Error(this, "file.cs", "Member", 42, new Exception(), data);
    }
}
