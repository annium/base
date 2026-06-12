using System;
using System.Collections.Generic;
using Annium.Core.Runtime.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Runtime.Tests.Types;

/// <summary>
/// Tests for ITypeManager.ResolveBySignature covering best-match selection, no-match, tighter-type
/// preference (scoring = matches*100 - Size), and ambiguity behaviour.
/// TypeSignature is internal; all behaviour is exercised indirectly through the public interface.
/// </summary>
public class SignatureResolutionTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the SignatureResolutionTests class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public SignatureResolutionTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that a signature exactly matching only one candidate resolves to that candidate.
    /// SigNarrow has { PropA, PropB }; SigWide has { PropA, PropB, PropC }.
    /// Querying for { PropC } only matches SigWide.
    /// </summary>
    [Fact]
    public void ResolveBySignature_UniquePropertyMatch_ReturnsMatchingType()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // act
        var result = manager.ResolveBySignature(new[] { nameof(SigWide.PropC) }, typeof(SigBase));

        // assert
        result.Is(typeof(SigWide));
    }

    /// <summary>
    /// Tests that when no candidate's property set overlaps the query, resolution returns null.
    /// </summary>
    [Fact]
    public void ResolveBySignature_NoPropertyOverlap_ReturnsNull()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // act
        var result = manager.ResolveBySignature(new[] { "NonExistentProp", "AnotherMissingProp" }, typeof(SigBase));

        // assert
        (result is null).IsTrue();
    }

    /// <summary>
    /// Tests that when two candidates overlap the same query keys, the candidate with fewer total
    /// properties wins (scoring = matches*100 - Size prefers smaller types on equal match counts).
    /// SigNarrow (Size=2) scores 200-2=198 vs SigWide (Size=3) scores 200-3=197 for a query
    /// of { PropA, PropB }.
    /// </summary>
    [Fact]
    public void ResolveBySignature_TieInMatchCount_PrefersTypeWithFewerProperties()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // act — both candidates match PropA+PropB; SigNarrow is tighter
        var result = manager.ResolveBySignature(
            new[] { nameof(SigNarrow.PropA), nameof(SigNarrow.PropB) },
            typeof(SigBase)
        );

        // assert
        result.Is(typeof(SigNarrow));
    }

    /// <summary>
    /// Tests that exact=false returns the best-scoring candidate even when multiple candidates match.
    /// SigNarrow scores higher than SigWide for a { PropA, PropB } query, so SigNarrow is returned.
    /// </summary>
    [Fact]
    public void ResolveBySignature_ExactFalse_ReturnsBestScoringCandidate()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // act
        var result = manager.ResolveBySignature(
            new[] { nameof(SigNarrow.PropA), nameof(SigNarrow.PropB) },
            typeof(SigBase),
            exact: false
        );

        // assert
        result.Is(typeof(SigNarrow));
    }

    /// <summary>
    /// Tests that exact=true throws InvalidOperationException when the query matches more than one
    /// candidate with distinct scores (scores differ so ambiguity guard is not hit, but
    /// SingleOrDefault throws on the multi-element result list).
    /// </summary>
    [Fact]
    public void ResolveBySignature_ExactTrueWithMultipleMatches_ThrowsInvalidOperationException()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // act + assert — PropA and PropB both hit SigNarrow and SigWide, scores differ (198 vs 197),
        // private method returns both; SingleOrDefault on 2 items throws
        Wrap.It(() =>
                manager.ResolveBySignature(
                    new[] { nameof(SigNarrow.PropA), nameof(SigNarrow.PropB) },
                    typeof(SigBase),
                    exact: true
                )
            )
            .Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Tests that exact=true succeeds (and resolves) when only a single candidate matches the query.
    /// PropC is present only on SigWide, so it is the sole match.
    /// </summary>
    [Fact]
    public void ResolveBySignature_ExactTrueWithSingleMatch_ReturnsMatchingType()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // act
        var result = manager.ResolveBySignature(new[] { nameof(SigWide.PropC) }, typeof(SigBase), exact: true);

        // assert
        result.Is(typeof(SigWide));
    }

    /// <summary>
    /// Tests that resolving by ambiguous top score (two candidates with equal score) throws
    /// TypeResolutionException. SigTie1 and SigTie2 each have exactly one distinct property
    /// plus a shared one; querying the shared property gives both a score of 1*100-2=98.
    /// </summary>
    [Fact]
    public void ResolveBySignature_AmbiguousTopScore_ThrowsTypeResolutionException()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // act + assert — SigTie1 { TiePropShared, TiePropOne } and
        // SigTie2 { TiePropShared, TiePropTwo } both score 100-2=98 for { TiePropShared }
        Wrap.It(() => manager.ResolveBySignature(new[] { nameof(SigTie1.TiePropShared) }, typeof(SigTieBase)))
            .Throws<TypeResolutionException>();
    }
}

/// <summary>Base class for signature-resolution fixture types used in best-match and no-match tests.</summary>
file class SigBase;

/// <summary>Narrow fixture type: PropA + PropB (Size=2).</summary>
file class SigNarrow : SigBase
{
    /// <summary>Gets or sets the first shared property.</summary>
    public int PropA { get; set; }

    /// <summary>Gets or sets the second shared property.</summary>
    public int PropB { get; set; }
}

/// <summary>Wide fixture type: PropA + PropB + PropC (Size=3).</summary>
file class SigWide : SigBase
{
    /// <summary>Gets or sets the first shared property.</summary>
    public int PropA { get; set; }

    /// <summary>Gets or sets the second shared property.</summary>
    public int PropB { get; set; }

    /// <summary>Gets or sets the third, unique property that distinguishes SigWide from SigNarrow.</summary>
    public int PropC { get; set; }
}

/// <summary>Base class for ambiguous-score fixture types.</summary>
file class SigTieBase;

/// <summary>First tie fixture: TiePropShared + TiePropOne (Size=2).</summary>
file class SigTie1 : SigTieBase
{
    /// <summary>Gets or sets the shared property present in all tie types.</summary>
    public int TiePropShared { get; set; }

    /// <summary>Gets or sets the unique property for SigTie1.</summary>
    public int TiePropOne { get; set; }
}

/// <summary>Second tie fixture: TiePropShared + TiePropTwo (Size=2).</summary>
file class SigTie2 : SigTieBase
{
    /// <summary>Gets or sets the shared property present in all tie types.</summary>
    public int TiePropShared { get; set; }

    /// <summary>Gets or sets the unique property for SigTie2.</summary>
    public int TiePropTwo { get; set; }
}
