using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Operations.Serialization.Tests.Base;

/// <summary>
/// Base class for testing Result serialization functionality.
/// </summary>
public abstract class ResultTestBase : TestBase
{
    /// <summary>
    /// Initializes a new instance of the ResultTestBase class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    protected ResultTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests basic Result serialization and deserialization.
    /// </summary>
    /// <typeparam name="T">The serialization format type.</typeparam>
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

    /// <summary>
    /// Tests Result with data serialization and deserialization.
    /// </summary>
    /// <typeparam name="T">The serialization format type.</typeparam>
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
