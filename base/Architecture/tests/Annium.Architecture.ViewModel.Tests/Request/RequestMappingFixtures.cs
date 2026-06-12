using Annium.Architecture.Base;

namespace Annium.Architecture.ViewModel.Tests.Request;

/// <summary>
/// Shared fixture types for request-side mapping pipe handler tests.
/// </summary>
internal static class RequestMappingFixtures
{
    /// <summary>Test view-model request that maps to <see cref="TestRequestOut"/>.</summary>
    internal class TestRequestIn : IRequest<TestRequestOut> { }

    /// <summary>Underlying domain request that the handler maps into.</summary>
    internal class TestRequestOut { }

    /// <summary>Response type returned by the next delegate.</summary>
    internal class TestResponse { }
}
