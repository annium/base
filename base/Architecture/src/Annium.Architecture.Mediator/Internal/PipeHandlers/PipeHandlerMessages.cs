namespace Annium.Architecture.Mediator.Internal.PipeHandlers;

/// <summary>
/// Diagnostic message constants surfaced by the mediator pipe handlers. Placed on a non-generic
/// internal type so both production handlers and tests can reference the strings without having
/// to close the generic base types' type parameters.
/// </summary>
internal static class PipeHandlerMessages
{
    /// <summary>
    /// Diagnostic message returned when a downstream handler throws an unhandled exception and
    /// the failure is converted to <c>UncaughtError</c>.
    /// </summary>
    internal const string InternalError = "An internal error occurred";

    /// <summary>
    /// Diagnostic message returned when a null request reaches the validation pipe handler.
    /// </summary>
    internal const string NullRequest = "Request is empty";
}
