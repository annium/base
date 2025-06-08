using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Operations.Serialization.Tests.Base;

/// <summary>
/// Base class for testing StatusResult serialization functionality.
/// </summary>
public abstract class StatusResultTestBase : TestBase
{
    /// <summary>
    /// Initializes a new instance of the StatusResultTestBase class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    protected StatusResultTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests basic StatusResult serialization and deserialization.
    /// </summary>
    /// <typeparam name="T">The serialization format type.</typeparam>
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

    /// <summary>
    /// Tests StatusResult with data serialization and deserialization.
    /// </summary>
    /// <typeparam name="T">The serialization format type.</typeparam>
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
