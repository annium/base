using Annium.Core.DependencyInjection;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Core.Mapper.Tests;

public class ContextualProfileTest : TestBase
{
    public ContextualProfileTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false).AddProfile<ContextualProfile>());
    }

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

    private class ContextualProfile : Profile
    {
        public ContextualProfile()
        {
            Map<SomePayload, SomeModel>().For<InnerModel>(x => x.X, ctx => x => ctx.Map<InnerModel>(x));
            Map<InnerPayload, InnerModel>(x => x == InnerPayload.A ? InnerModel.C : InnerModel.D);
            Map<OuterPayload, OuterModel>(ctx => x => new OuterModel(ctx.Map<InnerModel>(x.X)));
        }
    }

    private record SomePayload(InnerPayload X, int Value);

    private record SomeModel(InnerModel X, int Value);

    private record OuterPayload(InnerPayload X);

    private record OuterModel(InnerModel X);

    private enum InnerPayload
    {
        A,
        B
    }

    private enum InnerModel
    {
        C,
        D
    }
}