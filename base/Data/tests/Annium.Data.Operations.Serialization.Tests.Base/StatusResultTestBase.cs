using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit.Abstractions;

namespace Annium.Data.Operations.Serialization.Tests.Base;

public abstract class StatusResultTestBase : TestBase
{
    protected StatusResultTestBase(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    public void Simple_Base<T>()
    {
        // arrange
        var serializer = Get<ISerializer<T>>();
        var result = Result.Status("good").Error("plain").Error("label", "another");

        // act
        var serialized = serializer.Serialize(result);
        var deserialized = serializer.Deserialize<IStatusResult<string>>(serialized);

        // assert
        deserialized.IsOk.Is(result.IsOk);
        deserialized.HasErrors.Is(result.HasErrors);
        deserialized.Status.Is(result.Status);
        deserialized.PlainErrors.IsEqual(result.PlainErrors);
        deserialized.LabeledErrors.IsEqual(result.LabeledErrors);
    }

    public void Data_Base<T>()
    {
        // arrange
        var serializer = Get<ISerializer<T>>();
        var value = 5;
        var result = Result.Status("good", value).Error("plain").Error("label", "another");

        // act
        var serialized = serializer.Serialize(result);
        var deserialized = serializer.Deserialize<IStatusResult<string, int>>(serialized);

        // assert
        deserialized.IsOk.Is(result.IsOk);
        deserialized.HasErrors.Is(result.HasErrors);
        deserialized.Status.Is(result.Status);
        deserialized.Data.Is(result.Data);
        deserialized.PlainErrors.IsEqual(result.PlainErrors);
        deserialized.LabeledErrors.IsEqual(result.LabeledErrors);
    }
}