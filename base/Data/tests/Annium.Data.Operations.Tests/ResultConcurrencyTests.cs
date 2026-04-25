using System.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Operations.Tests;

/// <summary>
/// Concurrency tests for <see cref="Result"/> verifying that concurrent readers can iterate
/// <c>LabeledErrors</c> / <c>PlainErrors</c> while a writer mutates the collections without
/// seeing <see cref="System.InvalidOperationException"/> or corruption.
/// </summary>
public class ResultConcurrencyTests
{
    /// <summary>
    /// 1 writer mutating errors in a loop + 4 readers iterating the exposed collections in a
    /// loop; 1000 iterations each. Asserts no exceptions and no ghost entries.
    /// </summary>
    [Fact]
    public async Task ConcurrentReadWrite_NoExceptionsNoCorruption()
    {
        // arrange
        const int iterations = 1000;
        var result = Result.Create();

        // act — writer mutates plain and labeled errors continuously; readers iterate snapshots
        var ct = TestContext.Current.CancellationToken;
        var writer = Task.Run(
            () =>
            {
                for (var i = 0; i < iterations; i++)
                {
                    result.Error($"plain-{i}");
                    result.Error($"label-{i}", $"labeled-{i}");
                }
            },
            ct
        );

        var readers = Enumerable
            .Range(0, 4)
            .Select(_ =>
                Task.Run(
                    () =>
                    {
                        for (var i = 0; i < iterations; i++)
                        {
                            // each getter returns a snapshot — safe to iterate even as writer mutates
                            foreach (var e in result.PlainErrors)
                            {
                                _ = e.Length;
                            }
                            foreach (var (label, errors) in result.LabeledErrors)
                            {
                                _ = label.Length;
                                foreach (var e in errors)
                                {
                                    _ = e.Length;
                                }
                            }
                        }
                    },
                    ct
                )
            )
            .ToArray();

        await Task.WhenAll(new[] { writer }.Concat(readers));

        // assert — final snapshot has exactly one entry per unique (i) pair written
        result.PlainErrors.Count.Is(iterations);
        result.LabeledErrors.Count.Is(iterations);
    }
}
