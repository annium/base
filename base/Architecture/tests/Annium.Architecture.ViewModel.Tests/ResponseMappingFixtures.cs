using Annium.Architecture.Base;

namespace Annium.Architecture.ViewModel.Tests;

/// <summary>
/// Shared fixture types for response-side mapping pipe handler tests.
/// </summary>
internal static class ResponseMappingFixtures
{
    /// <summary>Test request marker used as the TRequest type parameter.</summary>
    internal class TestRequest { }

    /// <summary>Source DTO returned by the upstream handler before mapping.</summary>
    internal class TestSource { }

    /// <summary>View-model target the response mapping pipe handler maps into.</summary>
    internal class TestTarget : IResponse<TestSource> { }
}
