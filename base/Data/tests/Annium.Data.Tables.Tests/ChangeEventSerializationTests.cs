using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Tables.Tests;

public class ChangeEventSerializationTests
{
    [Fact]
    public void InitEvent_Ok()
    {
        // arrange
        var serializer = GetSerializer();
        var e = ChangeEvent.Init(new[] { 1, 2 });

        // act
        var back = serializer.Deserialize<IChangeEvent<int>>(serializer.Serialize(e));

        // assert
        var n = back.As<InitEvent<int>>();
        n.Values.IsEqual(new[] { 1, 2 });
    }

    [Fact]
    public void AddEvent_Ok()
    {
        // arrange
        var serializer = GetSerializer();
        var e = ChangeEvent.Add(3);

        // act
        var back = serializer.Deserialize<IChangeEvent<int>>(serializer.Serialize(e));

        // assert
        back.As<AddEvent<int>>().Value.Is(3);
    }

    [Fact]
    public void UpdateEvent_Ok()
    {
        // arrange
        var serializer = GetSerializer();
        var e = ChangeEvent.Update(3);

        // act
        var back = serializer.Deserialize<IChangeEvent<int>>(serializer.Serialize(e));

        // assert
        back.As<UpdateEvent<int>>().Value.Is(3);
    }

    [Fact]
    public void DeleteEvent_Ok()
    {
        // arrange
        var serializer = GetSerializer();
        var e = ChangeEvent.Delete(3);

        // act
        var back = serializer.Deserialize<IChangeEvent<int>>(serializer.Serialize(e));

        // assert
        back.As<DeleteEvent<int>>().Value.Is(3);
    }

    private ISerializer<string> GetSerializer()
    {
        var container = new ServiceContainer();
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddSerializers().WithJson(isDefault: true);
        container.AddLogging();

        var sp = container.BuildServiceProvider();

        sp.UseLogging(route => route.UseInMemory());

        return sp.Resolve<ISerializer<string>>();
    }
}