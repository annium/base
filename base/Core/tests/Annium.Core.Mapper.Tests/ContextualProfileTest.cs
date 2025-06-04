using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests;

/// <summary>
/// Tests for contextual profile-based mapping in the mapper.
/// </summary>
/// <remarks>
/// Verifies that the mapper can:
/// - Map objects using contextual profiles
/// - Handle context-specific mapping rules
/// - Preserve context information during mapping
/// - Apply different mapping rules based on context
/// </remarks>
public class ContextualProfileTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContextualProfileTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    /// <remarks>
    /// Registers the mapper with a contextual profile that defines complex mapping rules.
    /// </remarks>
    public ContextualProfileTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false).AddProfile<ContextualProfile>());
    }

    /// <summary>
    /// Tests that contextual mapping works correctly with nested objects.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Nested objects are correctly mapped using context
    /// - Enum values are transformed according to mapping rules
    /// - The mapping preserves the object structure
    /// </remarks>
    [Fact]
    public void ContextualMapping_With_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var payload = new OuterPayload(InnerPayload.B);

        // act
        var result = mapper.Map<OuterModel>(payload);

        // assert
        result.As<OuterModel>().X.Is(InnerModel.D);
    }

    /// <summary>
    /// Tests that contextual mapping works correctly with field-level transformations.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Individual fields can be mapped using context
    /// - The mapping rules are applied correctly at the field level
    /// - The result matches the expected transformation
    /// </remarks>
    [Fact]
    public void ContextualMapping_Field_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var payload = new OuterPayload(InnerPayload.B);

        // act
        var result = mapper.Map<OuterModel>(payload);

        // assert
        result.As<OuterModel>().X.Is(InnerModel.D);
    }

    /// <summary>
    /// Profile that defines contextual mapping rules for various types.
    /// </summary>
    /// <remarks>
    /// Configures mappings for:
    /// - SomePayload to SomeModel with context-aware field mapping
    /// - InnerPayload to InnerModel with conditional enum conversion
    /// - OuterPayload to OuterModel with nested object mapping
    /// </remarks>
    private class ContextualProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the ContextualProfile class.
        /// </summary>
        public ContextualProfile()
        {
            Map<SomePayload, SomeModel>().For<InnerModel>(x => x.X, ctx => x => ctx.Map<InnerModel>(x));
            Map<InnerPayload, InnerModel>(x => x == InnerPayload.A ? InnerModel.C : InnerModel.D);
            Map<OuterPayload, OuterModel>(ctx => x => new OuterModel(ctx.Map<InnerModel>(x.X)));
        }
    }

    /// <summary>
    /// Record representing a payload with nested enum value and integer.
    /// </summary>
    private record SomePayload(InnerPayload X, int Value);

    /// <summary>
    /// Record representing a model with nested enum value and integer.
    /// </summary>
    private record SomeModel(InnerModel X, int Value);

    /// <summary>
    /// Record representing a payload with nested enum value.
    /// </summary>
    private record OuterPayload(InnerPayload X);

    /// <summary>
    /// Record representing a model with nested enum value.
    /// </summary>
    private record OuterModel(InnerModel X);

    /// <summary>
    /// Enum representing possible values in the payload.
    /// </summary>
    private enum InnerPayload
    {
        /// <summary>
        /// First value.
        /// </summary>
        A,

        /// <summary>
        /// Second value.
        /// </summary>
        B,
    }

    /// <summary>
    /// Enum representing possible values in the model.
    /// </summary>
    private enum InnerModel
    {
        /// <summary>
        /// First value.
        /// </summary>
        C,

        /// <summary>
        /// Second value.
        /// </summary>
        D,
    }
}
