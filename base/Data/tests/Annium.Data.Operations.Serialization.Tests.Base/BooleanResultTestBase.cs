using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Operations.Serialization.Tests.Base;

/// <summary>
/// Base class for testing BooleanResult serialization functionality.
/// </summary>
public abstract class BooleanResultTestBase : TestBase
{
    /// <summary>
    /// Initializes a new instance of the BooleanResultTestBase class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    protected BooleanResultTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests basic BooleanResult serialization and deserialization.
    /// </summary>
    /// <typeparam name="T">The serialization format type.</typeparam>
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

    /// <summary>
    /// Tests BooleanResult with data serialization and deserialization.
    /// </summary>
    /// <typeparam name="T">The serialization format type.</typeparam>
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
