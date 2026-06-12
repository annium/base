namespace Annium.Core.Mapper.Internal.Resolvers;

/// <summary>
/// Base class for map resolvers that depend on an <see cref="IRepacker"/> to rewrite configured
/// member-mapping lambdas against the resolver-local source expression. Carries the injected repacker
/// so the concrete assignment / constructor resolvers do not each repeat the field and constructor.
/// </summary>
internal abstract class RepackerMapResolverBase
{
    /// <summary>
    /// The expression repacker for repackaging member-mapping expressions.
    /// </summary>
    protected readonly IRepacker Repacker;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepackerMapResolverBase"/> class.
    /// </summary>
    /// <param name="repacker">The expression repacker.</param>
    protected RepackerMapResolverBase(IRepacker repacker)
    {
        Repacker = repacker;
    }
}
