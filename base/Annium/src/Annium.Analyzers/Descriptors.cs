using Microsoft.CodeAnalysis;

namespace Annium.Analyzers;

internal static class Descriptors
{
    public static readonly DiagnosticDescriptor Pg0001ExceptionNameFormat =
        new(
            id: "PG0001",
            title: "Exception class name should end with Exception",
            messageFormat: "{0} class name should end with Exception",
            category: "Naming",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );
}
