using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Shared;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Logging.InMemory.Tests;

/// <summary>
/// Concurrency stress tests for <see cref="InMemoryLogHandler{TContext}"/> verifying that
/// it can absorb messages from many parallel writers without loss, duplication, or exceptions.
/// </summary>
public class InMemoryLogHandlerStressTests
{
    /// <summary>
    /// 100 writer tasks × 100 messages each must produce exactly 10_000 messages in <c>Logs</c>,
    /// with no duplicates and no missing sequence numbers.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Handle_100WritersX100Messages_AllMessagesPreserved()
    {
        // arrange
        const int writerCount = 100;
        const int messagesPerWriter = 100;
        const int expectedTotal = writerCount * messagesPerWriter;
        var handler = new InMemoryLogHandler<DefaultLogContext>();

        // act — 100 tasks racing to enqueue; each uses a distinct writer index so every
        // message produced is unique (writer, sequence) → any duplicates or losses are visible
        var writers = Enumerable
            .Range(0, writerCount)
            .Select(writerIndex =>
                Task.Run(() =>
                {
                    for (var seq = 0; seq < messagesPerWriter; seq++)
                        handler
                            .HandleAsync(new[] { BuildMessage(writerIndex, seq) }, CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                })
            )
            .ToArray();

        await Task.WhenAll(writers);

        // assert
        var snapshot = handler.Logs;
        snapshot.Count.Is(expectedTotal);

        // no duplicates — each (writer, seq) pair must appear exactly once
        var pairs = new HashSet<(int, int)>();
        foreach (var msg in snapshot)
        {
            var pair = ParsePair(msg.Message);
            pairs.Add(pair).IsTrue();
        }
        pairs.Count.Is(expectedTotal);
    }

    /// <summary>
    /// Constructs a synthetic log message carrying the writer and sequence number in
    /// <see cref="LogMessage{TContext}.Message"/> so the test can verify every message
    /// survived the trip through the handler.
    /// </summary>
    /// <param name="writerIndex">Producing writer index</param>
    /// <param name="sequence">Per-writer sequence number</param>
    /// <returns>A log message with the (writer, sequence) encoded</returns>
    private static LogMessage<DefaultLogContext> BuildMessage(int writerIndex, int sequence) =>
        new(
            new DefaultLogContext(),
            Instant.FromUnixTimeTicks(0),
            "test",
            "id",
            LogLevel.Info,
            0,
            $"{writerIndex}:{sequence}",
            null,
            string.Empty,
            new Dictionary<string, object?>(),
            "type",
            "member",
            0
        );

    /// <summary>
    /// Parses the synthetic <c>"writer:seq"</c> message format produced by
    /// <see cref="BuildMessage"/> back into its components.
    /// </summary>
    /// <param name="message">Log message body produced by <see cref="BuildMessage"/></param>
    /// <returns>Tuple of writer index and sequence number</returns>
    private static (int Writer, int Seq) ParsePair(string message)
    {
        var parts = message.Split(':');
        return (int.Parse(parts[0]), int.Parse(parts[1]));
    }
}
