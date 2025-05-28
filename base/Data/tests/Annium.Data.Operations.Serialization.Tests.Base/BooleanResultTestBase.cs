using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Operations.Serialization.Tests.Base;

public abstract class BooleanResultTestBase : TestBase
{
    protected BooleanResultTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    public void Simple_Base<T>()
    {
        // arrange
        var serializer = Get<ISerializer<T>>();
        var result = Result.Success().Error("plain").Error("label", "another");

        // act
        var serialized = serializer.Serialize(result);
        var deserialized = serializer.Deserialize<IBooleanResult>(serialized);

        // assert
        deserialized.IsOk.Is(result.IsOk);
        deserialized.HasErrors.Is(result.HasErrors);
        deserialized.IsSuccess.Is(result.IsSuccess);
        deserialized.IsFailure.Is(result.IsFailure);
        deserialized.PlainErrors.IsEqual(result.PlainErrors);
        deserialized.LabeledErrors.IsEqual(result.LabeledErrors);
    }

    public void Data_Base<T>()
    {
        // arrange
        var serializer = Get<ISerializer<T>>();
        var value = 5;
        var result = Result.Failure(value).Error("plain").Error("label", "another");

        // act
        var serialized = serializer.Serialize(result);
        var deserialized = serializer.Deserialize<IBooleanResult<int>>(serialized);

        // assert
        deserialized.IsOk.Is(result.IsOk);
        deserialized.HasErrors.Is(result.HasErrors);
        deserialized.IsSuccess.Is(result.IsSuccess);
        deserialized.IsFailure.Is(result.IsFailure);
        deserialized.Data.Is(result.Data);
        deserialized.PlainErrors.IsEqual(result.PlainErrors);
        deserialized.LabeledErrors.IsEqual(result.LabeledErrors);
    }
}
