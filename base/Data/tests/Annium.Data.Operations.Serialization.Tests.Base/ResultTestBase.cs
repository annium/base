using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit.Abstractions;

namespace Annium.Data.Operations.Serialization.Tests.Base;

public abstract class ResultTestBase : TestBase
{
    protected ResultTestBase(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    public void Simple_Base<T>()
    {
        // arrange
        var serializer = Get<ISerializer<T>>();
        var result = Result.New().Error("plain").Error("label", "another");

        // act
        var serialized = serializer.Serialize(result);
        var deserialized = serializer.Deserialize<IResult>(serialized);

        // assert
        deserialized.IsOk.Is(result.IsOk);
        deserialized.HasErrors.Is(result.HasErrors);
        deserialized.PlainErrors.IsEqual(result.PlainErrors);
        deserialized.LabeledErrors.IsEqual(result.LabeledErrors);
    }

    public void Data_Base<T>()
    {
        // arrange
        var serializer = Get<ISerializer<T>>();
        var value = 5;
        var result = Result.New(value).Error("plain").Error("label", "another");

        // act
        var serialized = serializer.Serialize(result);
        var deserialized = serializer.Deserialize<IResult<int>>(serialized);

        // assert
        deserialized.IsOk.Is(result.IsOk);
        deserialized.HasErrors.Is(result.HasErrors);
        deserialized.Data.Is(result.Data);
        deserialized.PlainErrors.IsEqual(result.PlainErrors);
        deserialized.LabeledErrors.IsEqual(result.LabeledErrors);
    }
}