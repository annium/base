using System;
using NodaTime;

namespace Annium;

public interface IActionScheduler
{
    /// <summary>
    ///     Executes some code asynchronously within given amount of time.
    /// </summary>
    /// <param name="handle">
    ///     Code to execute when time comes.
    /// </param>
    /// <param name="timeout">
    ///     The timeout at which the code will be run, in milliseconds.
    /// </param>
    Action Delay(Action handle, int timeout);

    /// <summary>
    ///     Executes some code asynchronously within given amount of time.
    /// </summary>
    /// <param name="handle">
    ///     Code to execute when time comes.
    /// </param>
    /// <param name="timeout">
    ///     The timeout at which the code will be run, as <see cref="Duration"/>.
    /// </param>
    Action Delay(Action handle, Duration timeout);

    /// <summary>
    ///     Executes some code asynchronously within given amount of time.
    /// </summary>
    /// <param name="handle">
    ///     Code to execute when time comes.
    /// </param>
    /// <param name="interval">
    ///     The interval at which the code will be run, in milliseconds.
    /// </param>
    Action Interval(Action handle, int interval);

    /// <summary>
    ///     Executes some code asynchronously within given amount of time.
    /// </summary>
    /// <param name="handle">
    ///     Code to execute when time comes.
    /// </param>
    /// <param name="interval">
    ///     The interval at which the code will be run, as <see cref="Duration"/>.
    /// </param>
    Action Interval(Action handle, Duration interval);
}