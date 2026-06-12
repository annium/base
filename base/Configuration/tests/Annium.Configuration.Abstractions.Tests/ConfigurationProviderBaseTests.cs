using System.Collections.Generic;
using System.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Configuration.Abstractions.Tests;

/// <summary>
/// Tests covering <see cref="ConfigurationProviderBase"/> protected contract — exercised via
/// minimal in-file stub subclasses that use every helper
/// (<c>Init</c>, <c>Push</c>, <c>Pop</c>, <c>Set</c>, <c>SetAt</c>, <c>Path</c>, <c>Result</c>).
/// </summary>
public class ConfigurationProviderBaseTests
{
    /// <summary>
    /// Calling <see cref="ConfigurationProviderBase.Read"/> twice on the same instance must
    /// produce identical, non-accumulated results. <see cref="ConfigurationProviderBase.Result"/>
    /// returns a snapshot each call, so the first-call dictionary is captured here as a
    /// snapshot copy and compared against the second-call result — a mutation removing
    /// <c>_data.Clear()</c> from <c>Init()</c> would cause the second call's count to grow.
    /// </summary>
    [Fact]
    public void Read_CalledTwice_ProducesIdenticalNonAccumulatedResult()
    {
        var provider = new BalancedStubProvider();

        var first = provider.Read();
        // Snapshot first to a list of (path, value) tuples — Result returns a fresh
        // Dictionary<string[], string> with reference-equality keys, so we match by sequence.
        var firstSnapshot = first.Select(kv => (Path: kv.Key, Value: kv.Value)).ToList();
        var second = provider.Read();

        firstSnapshot.Count.Is(4);
        second.Count.Is(firstSnapshot.Count);

        // Explicit per-path + value assertions (catches Set / SetAt mutations independently).
        FindByPath(firstSnapshot, "a").Is("v-a");
        FindByPath(firstSnapshot, "a", "b").Is("v-ab");
        FindByPath(firstSnapshot, "a", "b", "c").Is("v-abc");
        FindByPath(firstSnapshot, "absolute", "leaf").Is("v-abs");

        // Second read must contain exactly the same paths + values — not extra entries that
        // would survive a missing _data.Clear() in Init() (would fail the Count assertion above)
        // and not different values either.
        foreach (var (path, value) in firstSnapshot)
        {
            var match = second.FirstOrDefault(kv => kv.Key.SequenceEqual(path));
            match.Key.IsNotDefault();
            match.Value.Is(value);
        }
    }

    /// <summary>Lookup helper: find a value in a (path,value) list by sequence-equal path.</summary>
    /// <param name="snapshot">The (path, value) pairs captured from a provider read.</param>
    /// <param name="path">The key path segments to match by sequence equality.</param>
    /// <returns>The string value associated with the first entry whose path matches.</returns>
    private static string FindByPath(List<(string[] Path, string Value)> snapshot, params string[] path) =>
        snapshot.First(t => t.Path.SequenceEqual(path)).Value;

    /// <summary>
    /// Verifies the <c>Push</c> / <c>Pop</c> / <c>Path</c> machinery: after a Push the Path
    /// extends; after a matching Pop the Path contracts; nested sequences produce the expected
    /// dotted-path order (FIFO from outermost Push).
    /// </summary>
    [Fact]
    public void PushPop_ContextStack_BuildsPathInOuterToInnerOrder()
    {
        var provider = new BalancedStubProvider();
        provider.Read();

        provider.CapturedPaths.Has(3);
        provider.CapturedPaths[0].SequenceEqual(new[] { "a" }).IsTrue();
        provider.CapturedPaths[1].SequenceEqual(new[] { "a", "b" }).IsTrue();
        provider.CapturedPaths[2].SequenceEqual(new[] { "a", "b", "c" }).IsTrue();
    }

    /// <summary>
    /// Verifies that <c>Init()</c> clears the context stack even when a prior <c>Read()</c>
    /// left segments pushed (e.g. exception mid-read before matching Pop). Without
    /// <c>_context.Clear()</c> in <c>Init()</c>, the second <c>Read()</c> would observe a
    /// non-empty <c>Path</c> at its first <c>Push</c> and produce a wrongly-prefixed key.
    /// </summary>
    [Fact]
    public void Init_AfterUnbalancedPush_ClearsContextStack()
    {
        var provider = new UnbalancedStubProvider();

        // First Read leaves "stale" on the stack (no matching Pop). Path captured after
        // Push("stale") + Push("fresh") is ["stale", "fresh"].
        provider.Read();
        provider.LastPathSeenInRead.SequenceEqual(new[] { "stale", "fresh" }).IsTrue();

        // Second Read: Init() must clear the stack. If _context.Clear() was removed from
        // Init(), the leftover "stale" from the first Read would still be present, so
        // Push("stale") + Push("fresh") would yield ["stale", "stale", "fresh"] —
        // catching the mutation.
        provider.Read();
        provider.LastPathSeenInRead.SequenceEqual(new[] { "stale", "fresh" }).IsTrue();
        provider.FirstPushPath.SequenceEqual(new[] { "stale" }).IsTrue();
    }

    /// <summary>
    /// Minimal subclass exercising every protected helper. Balanced Push/Pop.
    /// </summary>
    private sealed class BalancedStubProvider : ConfigurationProviderBase
    {
        /// <summary>Snapshots of <see cref="ConfigurationProviderBase.Path"/> captured during Read.</summary>
        public List<string[]> CapturedPaths { get; } = new();

        /// <summary>
        /// Executes a balanced Push/Pop sequence, captures intermediate <c>Path</c> values,
        /// and sets absolute and relative keys to populate the result dictionary.
        /// </summary>
        /// <returns>A snapshot dictionary mapping string-array paths to their configured values.</returns>
        public override IReadOnlyDictionary<string[], string> Read()
        {
            Init();
            CapturedPaths.Clear();

            Push("a");
            CapturedPaths.Add(Path);
            Set("v-a");

            Push("b");
            CapturedPaths.Add(Path);
            Set("v-ab");

            Push("c");
            CapturedPaths.Add(Path);
            Set("v-abc");

            Pop();
            Pop();
            Pop();

            SetAt(new[] { "absolute", "leaf" }, "v-abs");

            return Result;
        }
    }

    /// <summary>
    /// Subclass that leaves <c>"stale"</c> on the context stack after its first <c>Read</c>
    /// (no matching Pop) — used to verify <c>Init()</c> resets the stack on subsequent reads.
    /// </summary>
    private sealed class UnbalancedStubProvider : ConfigurationProviderBase
    {
        /// <summary>The Path captured immediately after the second Push of the most recent Read.</summary>
        public string[] LastPathSeenInRead { get; private set; } = System.Array.Empty<string>();

        /// <summary>The Path captured immediately after the first Push of the most recent Read.</summary>
        public string[] FirstPushPath { get; private set; } = System.Array.Empty<string>();

        /// <summary>
        /// Pushes <c>"stale"</c> onto the context stack without a matching Pop, intentionally
        /// leaving residual state to verify that the next call to <c>Init()</c> clears it.
        /// </summary>
        /// <returns>A snapshot dictionary mapping string-array paths to their configured values.</returns>
        public override IReadOnlyDictionary<string[], string> Read()
        {
            Init();

            // Leave "stale" on the stack — caller never sees the matching Pop in this Read.
            Push("stale");
            // After the first push the path should be ["stale"] OR — if Init failed to clear
            // _context — would include whatever was left from a prior Read. Capture
            // the second push's path (after a fresh Push) and the first push's path on its own.
            FirstPushPath = Path;

            Push("fresh");
            LastPathSeenInRead = Path;
            Set("v-fresh");
            Pop();

            // Intentionally NO matching Pop for "stale" — Init must clear it on next call.
            return Result;
        }
    }
}
