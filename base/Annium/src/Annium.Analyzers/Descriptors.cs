using Microsoft.CodeAnalysis;

namespace Annium.Analyzers;

/// <summary>
/// Diagnostic descriptors emitted by analyzers in this assembly.
/// </summary>
/// <remarks>
/// <para>
/// <b>ID prefix policy:</b>
/// </para>
/// <list type="bullet">
/// <item><description><c>LOG</c> — diagnostics that target the Annium logging APIs (`LogSubjectExtensions`)</description></item>
/// <item><description><c>AN</c> — Annium naming / convention diagnostics that apply more broadly than a single API surface</description></item>
/// </list>
/// <para>
/// Tests consume these descriptors via <c>InternalsVisibleTo("Annium.Analyzers.Tests")</c>; fields are
/// <c>internal</c> to match the enclosing class and prevent accidental leakage if the assembly visibility
/// ever changes.
/// </para>
/// </remarks>
internal static class Descriptors
{
    /// <summary>
    /// Diagnostic descriptor for detecting non-constant log message templates.
    /// </summary>
    internal static readonly DiagnosticDescriptor Log0001DynamicLogMessageTemplate = new(
        id: "LOG0001",
        title: "Log message template must be constant string",
        messageFormat: "Call message template is non-constant",
        category: "Logging",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    /// <summary>
    /// Diagnostic descriptor for detecting malformed (unbalanced-brace) log message templates.
    /// </summary>
    internal static readonly DiagnosticDescriptor Log0003MalformedLogMessageTemplate = new(
        id: "LOG0003",
        title: "Log message template is malformed",
        // no format args are supplied, so the message is used verbatim — single braces render as-is
        messageFormat: "Log message template has unbalanced '{' / '}' braces",
        category: "Logging",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    /// <summary>
    /// Diagnostic descriptor for detecting explicit values passed to caller-info parameters of logging extension methods.
    /// </summary>
    internal static readonly DiagnosticDescriptor Log0002ExplicitCallerArgument = new(
        id: "LOG0002",
        title: "Caller-info argument must not be specified explicitly",
        messageFormat: "Argument bound to '{0}' overrides a compiler-injected caller-info value",
        category: "Logging",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    /// <summary>
    /// Diagnostic descriptor for enforcing exception class naming convention.
    /// </summary>
    internal static readonly DiagnosticDescriptor An0001ExceptionNameFormat = new(
        id: "AN0001",
        title: "Exception class name should end with Exception",
        messageFormat: "{0} class name should end with Exception",
        category: "Naming",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
}
